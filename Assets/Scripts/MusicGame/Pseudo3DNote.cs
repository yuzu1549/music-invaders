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
		Init(targetLane, calculatedDuration, endY);
	}

	public void Init(int targetLane, float calculatedDuration, float targetY)
	{
		this.lane = targetLane;

		// 1. 判定ラインに到達するまでの時間
		this.judgementDuration = calculatedDuration;

		// 2. 判定ラインの位置（targetY）から逆算した、全体の移動時間
		float timeRatio = GetNormalizedTimeAtY(targetY);
		this.movementDuration = calculatedDuration / timeRatio;

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

		transform.localPosition = startPos;
		transform.localScale = new Vector3(startScaleX, startScaleY, 1f);
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

		float t = timer / movementDuration;
		float missWindow = (JudgementManager.Instance != null) ? JudgementManager.Instance.missWindow : 0.1666f;

		// 判定ラインを過ぎて、MISSの猶予時間が過ぎるまで生き残る
		if (timer <= judgementDuration + missWindow)
		{
			float moveT = GetMoveProgress(t);

			transform.localPosition = Vector3.LerpUnclamped(startPos, endPos, moveT);

			float clampedT = Mathf.Clamp01(moveT);
			float currentScaleX_val = Mathf.Lerp(startScaleX, endScaleX, clampedT);
			float currentScaleY_val = Mathf.Lerp(startScaleY, endScaleY, clampedT);
			transform.localScale = new Vector3(currentScaleX_val, currentScaleY_val, 1f);
		}
		else
		{
			Debug.Log("MISS... (見逃し)");
			if (JudgementManager.Instance != null)
			{
				JudgementManager.Instance.DisplayMiss();
			}
			HitAndDespawn();
		}
	}

	public float GetTimeDiff()
	{
		// 純粋な時間差だけをマネージャーに返す
		return timer - judgementDuration;
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
