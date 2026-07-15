using UnityEngine;

public class Pseudo3DNote : MonoBehaviour, IPoolable
{
	[Header("判定調整")]
	[Tooltip("0にすると見た目の判定ラインとパーフェクトが完全に一致します！")]
	public float judgeOffset = 0.0f; // 🔥デフォルトを0に修正しました

	[Header("レーン設定")]
	public int lane = 0;

	[Header("移動・速度設定")]
	public float baseDuration = 2.0f;
	public float noteSpeed = 1.0f;

	[Header("見た目の速度調整（0=2D的, 1=3D的）")]
	[Range(0f, 1f)]
	public float perspectiveBlend = 0.5f;

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
		if (actualDuration <= 0f) return;

		timer += Time.deltaTime;

		float t = timer / actualDuration;
		float missWindow = (JudgementManager.Instance != null) ? JudgementManager.Instance.missWindow : 0.1666f;

		// 判定ラインを過ぎて、MISSの猶予時間が過ぎるまで生き残る
		if (timer <= actualDuration + missWindow)
		{
			float linearT = t;

			float startZ = 1f / startScaleX;
			float endZ = 1f / endScaleX;
			float currentZ = Mathf.Lerp(startZ, endZ, t);
			float currentScaleVal = 1f / currentZ;
			float true3DT = (currentScaleVal - startScaleX) / (endScaleX - startScaleX);

			float moveT = Mathf.Lerp(linearT, true3DT, perspectiveBlend);

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
		return timer - actualDuration;
	}
}
