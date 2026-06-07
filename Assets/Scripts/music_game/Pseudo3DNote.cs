using UnityEngine;

public class Pseudo3DNote : MonoBehaviour, IPoolable
{
	[Header("判定調整")]
	public float judgeOffset = 0.05f;

	[Header("レーン設定")]
	public int lane = 0;

	[Header("移動・速度設定")]
	public float baseDuration = 2.0f;
	public float noteSpeed = 1.0f;

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
	private float actualDuration;
	private Vector3 startPos;
	private Vector3 endPos;
	private ObjectPool _pool;

	public void SetPool(ObjectPool pool) => _pool = pool;
	public void OnSpawn() { timer = 0f; gameObject.SetActive(true); }
	public void OnDespawn() { gameObject.SetActive(false); }

	public void Init(int targetLane, float calculatedDuration)
	{
		this.lane = targetLane;
		this.actualDuration = calculatedDuration;

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
		// ★追加：まだマネージャーから時間をもらっていない（または 0秒）の時は処理を止める！
		// これで野良ノーツがいてもエラー（NaN）を出さなくなります。
		if (actualDuration <= 0f) return;

		timer += Time.deltaTime;

		float t = timer / actualDuration;

		// ★修正：ハードコード（0.1666f）をやめて、JudgmentManagerに設定されたMISS判定時間を自動で使うようにします
		// 万が一マネージャーがいない場合のエラーを防ぐため、存在しない場合は 0.1666f を使います
		float missWindow = (JudgmentManager.Instance != null) ? JudgmentManager.Instance.missWindow : 0.1666f;

		// 判定ラインを過ぎて、MISSの猶予時間が過ぎるまで生き残る
		if (timer <= actualDuration + missWindow)
		{
			float moveT = t;
			transform.localPosition = Vector3.LerpUnclamped(startPos, endPos, moveT);

			float clampedT = Mathf.Clamp01(t);
			float currentScaleX = Mathf.Lerp(startScaleX, endScaleX, clampedT);
			float currentScaleY = Mathf.Lerp(startScaleY, endScaleY, clampedT);
			transform.localScale = new Vector3(currentScaleX, currentScaleY, 1f);
		}
		else
		{
			// 猶予時間を過ぎても叩かれなかったら「見逃しMISS」
			Debug.Log("MISS... (見逃し)");

			// ★追加：JudgmentManagerに「見逃し用MISSの画面表示」を命令する
			if (JudgmentManager.Instance != null)
			{
				JudgmentManager.Instance.DisplayMiss();
			}

			HitAndDespawn();
		}
	}

	public float GetTimeDiff()
	{
		return (timer - actualDuration) + judgeOffset;
	}
}
