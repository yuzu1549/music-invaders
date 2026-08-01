using UnityEngine;

public class Pseudo3DNote : MonoBehaviour, IPoolable
{
	[Header("判定調整")]
	[Tooltip("0にすると見た目の判定ラインとパーフェクトが完全に一致します！")]
	public float judgeOffset = 0.0f; 

	[Header("レーン設定")]
	public int lane = 0;


	[Header("見た目の速度調整（0=2D的, 1=3D的）")]
	[Range(0f, 1f)]
	public float perspectiveBlend = 0.5f;

	[Header("見た目の設定（色分け）")]
	private Color leftColor = Color.white; // 左レーンの色
	private Color rightColor = Color.black; // 右レーンの色

	[Header("座標設定（下から真ん中の場合）")]
	public float startY = -5f;
	public float endY = 0f;
	public float startSpread = 2f;
	public float endSpread = 0.2f;

	[Header("サイズ設定（手前から奥へ）")]
	public float startScaleX = 2.0f;
	public float endScaleX = 0.5f;
	public float startScaleY = 0.2f;
	public float endScaleY = 0.05f;

	private float timer = 0f;
	private float judgementDuration;
	private float movementDuration;
	private Vector3 startPos;
	private Vector3 endPos;
	private ObjectPool _pool;

	public void SetPool(ObjectPool pool) => _pool = pool;
	public void OnSpawn() { timer = 0f; gameObject.SetActive(true); }
	public void OnDespawn() { gameObject.SetActive(false); }

	public void Init(int targetLane, float calculatedDuration)
	{
		Init(targetLane, calculatedDuration, endY, 0f);
	}

	public void Init(int targetLane, float calculatedDuration, float targetY)
	{
		Init(targetLane, calculatedDuration, judgementLineY, 0f);
	}

	public void Init(
		int targetLane,
		float calculatedDuration,
		float judgementLineY,
		float elapsedTime)
	{
		this.lane = targetLane;

		// 🔥修正：インスペクターの割り当てミスを防ぐため、自分自身の画像を自動で取得する
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		if (sr == null)
		{
			sr = GetComponentInChildren<SpriteRenderer>();
		}

		// レーンによって色を切り替える
		if (sr != null)
		{
			if (this.lane < 0) // レーンが -1 などの場合
			{
				sr.color = leftColor; // 左なら緑
			}
			else if (this.lane > 0) // レーンが 1 などの場合
			{
				sr.color = rightColor; // 右なら青
			}
			else
			{
				sr.color = Color.white; // レーンが 0（真ん中）の場合は白にする
			}
		}

		float direction = (float)this.lane;
		startPos = new Vector3(startSpread * direction, startY, 0);
		endPos = new Vector3(endSpread * direction, endY, 0);

		SetDurations(calculatedDuration, judgementLineY);
		timer = elapsedTime;

		UpdateVisual();
	}

	/// <summary>
	/// 再開時のノーツ速度とタイミング調整を未判定ノーツへ反映する。
	/// </summary>
	/// <param name="calculatedDuration">変更後のノーツ表示時間</param>
	/// <param name="timingOffsetDeltaSeconds">
	/// 変更前後のタイミング調整値の差（秒）
	/// </param>
	/// <param name="judgementLineY">判定線のローカル Y 座標</param>
	/// <param name="shouldResolveMiss">
	/// 強制 MISS 時刻を過ぎたノーツを確定する場合は true
	/// </param>
	public void ApplyPlaybackSettings(
		float calculatedDuration,
		float timingOffsetDeltaSeconds,
		float judgementLineY,
		bool shouldResolveMiss)
	{
		float adjustedTimeDiff = GetTimeDiff() + timingOffsetDeltaSeconds;

		SetDurations(calculatedDuration, judgementLineY);
		timer = judgementDuration + adjustedTimeDiff;

		if (shouldResolveMiss && GetMissWindow() < adjustedTimeDiff)
		{
			MissAndDespawn();
			return;
		}

		UpdateVisual();
	}

	public void HitAndDespawn()
	{
		gameObject.SetActive(false);
		if (_pool != null)
		{
			_pool.Return(gameObject);
		}
	}

	void Update()
	{
		if (movementDuration <= 0f || judgementDuration <= 0f) return;

		timer += Time.deltaTime;

		// 判定ラインを過ぎて、MISSの猶予時間が過ぎるまで生き残る
		if (timer <= judgementDuration + GetMissWindow())
		{
			UpdateVisual();
		}
		else
		{
			MissAndDespawn();
		}
	}

	public float GetTimeDiff()
	{
		// 純粋な時間差だけをマネージャーに返す
		return timer - judgementDuration;
	}

	/// <summary>
	/// 表示時間と判定線から、判定時刻と移動時間を設定する。
	/// </summary>
	private void SetDurations(float calculatedDuration, float judgementLineY)
	{
		judgementDuration = calculatedDuration;

		float judgementNormalizedTime = GetNormalizedTimeAtY(judgementLineY);
		movementDuration = judgementDuration / judgementNormalizedTime;
	}

	/// <summary>
	/// 現在の経過時間に対応する位置と大きさを反映する。
	/// </summary>
	private void UpdateVisual()
	{
		if (movementDuration <= 0f)
		{
			return;
		}

		float moveT = GetMoveProgress(timer / movementDuration);
		transform.localPosition = Vector3.LerpUnclamped(startPos, endPos, moveT);

		float clampedMoveT = Mathf.Clamp01(moveT);
		float currentScaleX =
			Mathf.Lerp(startScaleX, endScaleX, clampedMoveT);
		float currentScaleY =
			Mathf.Lerp(startScaleY, endScaleY, clampedMoveT);
		transform.localScale =
			new Vector3(currentScaleX, currentScaleY, 1f);
	}

	/// <summary>
	/// 現在設定されている強制 MISS までの猶予時間を返す。
	/// </summary>
	/// <returns>強制 MISS までの猶予時間（秒）</returns>
	private float GetMissWindow()
	{
		return (JudgementManager.Instance != null)
			? JudgementManager.Instance.missWindow
			: 0.1666f;
	}

	/// <summary>
	/// 見逃しを記録してノーツをプールへ戻す。
	/// </summary>
	private void MissAndDespawn()
	{
		Debug.Log("MISS... (見逃し)");

		if (JudgementManager.Instance != null)
		{
			JudgementManager.Instance.DisplayMiss();
		}

		HitAndDespawn();
	}

	/// <summary>
	/// 指定した Y 座標へ到達する移動時間の割合を求める。
	/// </summary>
	/// <param name="targetY">判定線のローカル Y 座標</param>
	/// <returns>移動開始を 0、終端到達を 1 とした時間の割合</returns>
	private float GetNormalizedTimeAtY(float targetY)
	{
		if (Mathf.Approximately(startY, endY))
		{
			return 1f;
		}

		float targetMoveProgress = Mathf.InverseLerp(startY, endY, targetY);
		if (targetMoveProgress <= 0f)
		{
			return 0.0001f;
		}

		if (targetMoveProgress >= 1f)
		{
			return 1f;
		}

		float minTime = 0f;
		float maxTime = 1f;

		// 透視補間は非線形なので、二分探索で判定線を通る時刻を求める。
		for (int i = 0; i < 16; i++)
		{
			float middleTime = (minTime + maxTime) * 0.5f;
			if (GetMoveProgress(middleTime) < targetMoveProgress)
			{
				minTime = middleTime;
			}
			else
			{
				maxTime = middleTime;
			}
		}

		return (minTime + maxTime) * 0.5f;
	}

	/// <summary>
	/// 経過時間の割合から、位置とサイズの補間に使う進行度を求める。
	/// </summary>
	/// <param name="normalizedTime">移動時間の割合</param>
	/// <returns>開始位置を 0、終端位置を 1 とした移動進行度</returns>
	private float GetMoveProgress(float normalizedTime)
	{
		if (Mathf.Approximately(startScaleX, endScaleX))
		{
			return normalizedTime;
		}

		float startZ = 1f / startScaleX;
		float endZ = 1f / endScaleX;
		float currentZ = Mathf.Lerp(startZ, endZ, normalizedTime);
		float currentScale = 1f / currentZ;
		float perspectiveProgress =
			(currentScale - startScaleX) / (endScaleX - startScaleX);

		return Mathf.Lerp(normalizedTime, perspectiveProgress, perspectiveBlend);
	}
}
