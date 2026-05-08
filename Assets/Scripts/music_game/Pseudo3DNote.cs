using UnityEngine;

public class Pseudo3DNote : MonoBehaviour, IPoolable
{
	[Header("レーン設定")]
	public int lane = 0;

	[Header("移動・速度設定")]
	public float baseDuration = 2.0f;
	public float noteSpeed = 1.0f;

	[Header("座標設定")]
	public float startY = 200f;
	public float endY = -400f;
	public float startSpread = 20f;
	public float endSpread = 400f;

	[Header("サイズ設定（横幅）")]
	public float minScaleX = 0.2f;
	public float maxScaleX = 1.0f;

	[Header("サイズ設定（縦幅/厚み）")]
	public float minScaleY = 0.05f;
	public float maxScaleY = 0.2f;

	// --- ここから下は内部処理用の変数 ---
	private float timer = 0f;
	private float actualDuration;
	private Vector3 startPos;
	private Vector3 endPos;
	private ObjectPool _pool;

	// --- IPoolableの実装 ---
	public void SetPool(ObjectPool pool) => _pool = pool;

	public void OnSpawn()
	{
		timer = 0f;
		gameObject.SetActive(true);
	}

	public void OnDespawn()
	{
		gameObject.SetActive(false);
	}

	// --- 初期化処理 ---
	public void Init(int targetLane)
	{
		this.lane = targetLane;
		actualDuration = baseDuration / noteSpeed;

		float direction = (lane == 0) ? -1f : 1f;
		startPos = new Vector3(startSpread * direction, startY, 0);
		endPos = new Vector3(endSpread * direction, endY, 0);

		transform.localPosition = startPos;
		transform.localScale = new Vector3(minScaleX, minScaleY, 1f);
	}

	// --- 毎フレームの移動処理 ---
	void Update()
	{
		timer += Time.deltaTime;
		float t = timer / actualDuration;
		float curveT = t * t;

		if (curveT <= 1f)
		{
			// 移動
			transform.localPosition = Vector3.Lerp(startPos, endPos, curveT);

			// サイズ変更
			float currentScaleX = Mathf.Lerp(minScaleX, maxScaleX, curveT);
			float currentScaleY = Mathf.Lerp(minScaleY, maxScaleY, curveT);
			transform.localScale = new Vector3(currentScaleX, currentScaleY, 1f);
		}
		else
		{
			// 時間切れになったらプールに返す
			if (_pool != null)
			{
				_pool.Return(gameObject);
			}
			else
			{
				gameObject.SetActive(false); // プールがない場合の保険
			}
		}
	}
}
