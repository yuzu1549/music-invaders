using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIFollowTarget : MonoBehaviour
{
	[Header("追従する対象")]
	public Transform target;

	[Header("位置の微調整（ピクセル単位）")]
	public Vector3 screenOffset = new Vector3(0, 50f, 0);

	private RectTransform rectTransform;
	private RectTransform parentRect;
	private Canvas parentCanvas;
	private Camera mainCamera;

	void Start()
	{
		rectTransform = GetComponent<RectTransform>();
		mainCamera = Camera.main;
		parentCanvas = GetComponentInParent<Canvas>();

		if (rectTransform.parent != null)
		{
			parentRect = rectTransform.parent.GetComponent<RectTransform>();
		}
	}

	void LateUpdate()
	{
		if (target == null || mainCamera == null || parentCanvas == null || parentRect == null) return;

		// 🔥修正：スクリーン座標（ピクセル）に変換した直後に、オフセットを足す！
		Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position) + screenOffset;

		// カメラの後ろにいる場合は処理しない
		if (screenPos.z < 0) return;

		Camera uiCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera;

		// スクリーン座標を、UI専用の正しい座標（World Point）に変換して適用
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, screenPos, uiCamera, out Vector3 worldPos))
		{
			// 🔥修正：ここではそのまま代入するだけ
			rectTransform.position = worldPos;
		}
	}
}
