using UnityEngine;

public class TitleFloatingObject : MonoBehaviour
{
    [Header("上下移動の幅")]
    [SerializeField]
    private float moveDistance = 8f;

    [Header("上下移動の速さ")]
    [SerializeField]
    private float moveSpeed = 1.5f;

    [Header("回転する角度")]
    [SerializeField]
    private float rotateDistance = 4f;

    private RectTransform rectTransform;

    private Vector2 startPosition;
    private float startRotation;
    private float offset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        startPosition = rectTransform.anchoredPosition;
        startRotation = rectTransform.localEulerAngles.z;

        offset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        float wave =
            Mathf.Sin(Time.time * moveSpeed + offset);

        rectTransform.anchoredPosition =
            startPosition
            + Vector2.up * wave * moveDistance;

        rectTransform.localRotation = Quaternion.Euler(
            0f,
            0f,
            startRotation + wave * rotateDistance
        );
    }
}