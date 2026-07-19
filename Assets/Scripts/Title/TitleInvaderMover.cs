using UnityEngine;

public class TitleInvaderMover : MonoBehaviour
{
    [Header("左右に動く距離")]
    [SerializeField]
    private float moveDistance = 70f;

    [Header("移動速度")]
    [SerializeField]
    private float moveSpeed = 1f;

    private RectTransform rectTransform;
    private Vector2 initialPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError(
                "TitleInvaderMoverはUIの親オブジェクトに付けてください。",
                this
            );

            enabled = false;
            return;
        }

        initialPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        float x =
            Mathf.Sin(Time.time * moveSpeed)
            * moveDistance;

        rectTransform.anchoredPosition =
            initialPosition + new Vector2(x, 0f);
    }
}