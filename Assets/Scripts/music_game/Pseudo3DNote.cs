using UnityEngine;


public class Pseudo3DNote : MonoBehaviour, IPoolable
{
	[Header("判定調整")]
	// プラスの数値を設定すると、その秒数だけ判定が手前（早め）になります
	public float judgeOffset = 0.05f;

	[Header("レーン設定")]
	public int lane = 0;

	[Header("移動・速度設定")]
	public float baseDuration = 2.0f;
	public float noteSpeed = 1.0f;

	[Header("座標設定（下から真ん中の場合）")]
	public float startY = -5f;      // 出現位置（下）
	public float endY = 0f;         // 目標位置（真ん中）
	public float startSpread = 2f;  // 下での左右の広がり
	public float endSpread = 0.2f;  // 真ん中での左右の広がり

	[Header("サイズ設定（手前から奥へ）")]
	public float startScaleX = 2.0f; // 出現時（大）
	public float endScaleX = 0.5f;   // 消滅時（小）
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

	/*
	public void Init(int targetLane)
	{
		this.lane = targetLane;
		actualDuration = baseDuration / noteSpeed;

		float direction = (lane == 0) ? -1f : 1f;
		// startPos = 下の広がり / endPos = 真ん中の広がり
		startPos = new Vector3(startSpread * direction, startY, 0);
		endPos = new Vector3(endSpread * direction, endY, 0);

		transform.localPosition = startPos;
		transform.localScale = new Vector3(startScaleX, startScaleY, 1f);
	}
	*/

	// ★修正：第2引数に float calculatedDuration を追加
	public void Init(int targetLane, float calculatedDuration)
	{
		this.lane = targetLane;

		// ★修正：自分のインスペクターの値ではなく、マネージャーから貰った正しい時間を代入する
		this.actualDuration = calculatedDuration;

		// laneが -1 ならマイナス、1ならプラス、0ならX座標は0になる
		float direction = (float)this.lane;
		startPos = new Vector3(startSpread * direction, startY, 0);
		endPos = new Vector3(endSpread * direction, endY, 0);

		transform.localPosition = startPos;
		transform.localScale = new Vector3(startScaleX, startScaleY, 1f);
	}

	// ★追加：判定マネージャー（JudgmentManager）から叩かれた時に呼ぶ用
	public void HitAndDespawn()
	{
		// ★プールに返す前に、問答無用で強制的に非表示にする（Updateもこれで止まります）
		gameObject.SetActive(false);

		if (_pool != null)
		{
			_pool.Return(gameObject);
		}
	}

	void Update()
	{
		timer += Time.deltaTime;

		// ★最重要：tの計算は純粋に 0.0 ～ 1.0 を超えて進むようにする
		//例: actualDurationが2秒なら、2.1666秒まで動く (2.1666 / 2.0 = t=1.0833)
		float t = timer / actualDuration;

		// 判定ライン（t = 1.0）を過ぎて、さらにMISS猶予（秒数換算）が過ぎるまで生き残る
		if (timer <= actualDuration + 0.1666f)
		{
			// ★重要：Mathf.SinやClamp01を外し、tそのものを使う
			// これによって移動の計算が純粋な「時間：座標＝1：1」になります
			float moveT = t;

			// ★最重要：LerpUnclamped を使う！
			// これによって、t が 1.0 を超えたとき、判定ライン（endPos）を通り過ぎて
			// そのまま画面の手前に突き抜けていく挙動になります。
			transform.localPosition = Vector3.LerpUnclamped(startPos, endPos, moveT);

			// サイズの計算は、突き抜けすぎると変になるので、Clamp01をかけてライン上で止めます
			float clampedT = Mathf.Clamp01(t);
			float currentScaleX = Mathf.Lerp(startScaleX, endScaleX, clampedT);
			float currentScaleY = Mathf.Lerp(startScaleY, endScaleY, clampedT);
			transform.localScale = new Vector3(currentScaleX, currentScaleY, 1f);
		}
		else
		{
			// 猶予時間を過ぎても叩かれなかったら「見逃しMISS」
			Debug.Log("MISS... (見逃し)");
			HitAndDespawn();
		}
	}


	public float GetTimeDiff()
	{
		// マイナスなら早押し(Early)、プラスなら遅押し(Late)、0なら完全なジャスト
		return (timer - actualDuration) + judgeOffset;
	}

}
