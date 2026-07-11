using UnityEngine;

public class SimpleHitEffect : MonoBehaviour
{
	private SpriteRenderer spriteRenderer;

	[Header("エフェクトの設定")]
	[Tooltip("エフェクトが消えるまでの時間（秒）")]
	public float duration = 0.25f;

	[Tooltip("開始時の大きさ")]
	public Vector3 startScale = new Vector3(0.05f, 0.05f, 1f);

	[Tooltip("最終的な大きさ")]
	public Vector3 targetScale = new Vector3(0.2f, 0.2f, 1f);

	private float timer = 0f;
	private Color startColor;

	void Start()
	{
		// 自分のSpriteRendererコンポーネントを取得
		spriteRenderer = GetComponent<SpriteRenderer>();

		if (spriteRenderer != null)
		{
			startColor = spriteRenderer.color;
		}

		// 初期サイズに設定
		transform.localScale = startScale;
	}

	void Update()
	{
		// 経過時間をカウント
		timer += Time.deltaTime;

		// アニメーションの進行度（0.0 〜 1.0）を計算
		float progress = timer / duration;

		if (progress <= 1f)
		{
			// 🔥 演出1：サイズをだんだん大きくする
			// 普通に大きくするより、最初が速く、後半ゆっくり広がる（Ease Out）と音ゲーっぽくなります
			float easeOutProgress = 1f - Mathf.Pow(1f - progress, 3f);
			transform.localScale = Vector3.Lerp(startScale, targetScale, easeOutProgress);

			// 🔥 演出2：だんだん透明（フェードアウト）にする
			if (spriteRenderer != null)
			{
				Color newColor = startColor;
				newColor.a = Mathf.Lerp(startColor.a, 0f, progress); // アルファ値を徐々に0へ
				spriteRenderer.color = newColor;
			}
		}
		else
		{
			// 時間が来たら、メモリ節約のために自分自身を消滅させる（超重要！）
			Destroy(gameObject);
		}
	}
}
