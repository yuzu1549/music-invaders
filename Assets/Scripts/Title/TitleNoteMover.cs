using UnityEngine;

public class TitleNoteMover : MonoBehaviour
{
    [Header("上下に動く距離")]
    [SerializeField]
    private float moveDistance = 15f;

    [Header("動く速さ")]
    [SerializeField]
    private float moveSpeed = 0.6f;

    [Header("動き始める位置のずれ")]
    [SerializeField]
    private float phaseOffset = 0f;

    private RectTransform rectTransform;
    private Vector2 initialPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError(
                "TitleNoteMoverはUIの音符オブジェクトに付けてください。",
                this
            );

            enabled = false;
            return;
        }

        initialPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        float y = Mathf.Sin(
            Time.time * moveSpeed + phaseOffset
        ) * moveDistance;

        rectTransform.anchoredPosition =
            initialPosition + new Vector2(0f, y);
    }
}