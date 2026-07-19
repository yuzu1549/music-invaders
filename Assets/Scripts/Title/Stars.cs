using UnityEngine;
using UnityEngine.UI;

public class TitleStarGenerator : MonoBehaviour
{
    [Header("生成する星の数")]
    [SerializeField]
    private int starCount = 60;

    [Header("星の最小サイズ")]
    [SerializeField]
    private float minSize = 2f;

    [Header("星の最大サイズ")]
    [SerializeField]
    private float maxSize = 6f;

    [Header("星の透明度")]
    [SerializeField]
    [Range(0f, 1f)]
    private float minAlpha = 0.25f;

    [SerializeField]
    [Range(0f, 1f)]
    private float maxAlpha = 0.9f;

    private RectTransform parentRect;

    private void Awake()
    {
        parentRect = GetComponent<RectTransform>();

        CreateStars();
    }

    private void CreateStars()
    {
        for (int i = 0; i < starCount; i++)
        {
            GameObject starObject = new GameObject(
                $"Star_{i + 1}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(TitleStarTwinkle)
            );

            starObject.transform.SetParent(transform, false);

            RectTransform starRect =
                starObject.GetComponent<RectTransform>();

            starRect.anchorMin = new Vector2(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f)
            );

            starRect.anchorMax = starRect.anchorMin;
            starRect.anchoredPosition = Vector2.zero;

            float size = Random.Range(minSize, maxSize);

            starRect.sizeDelta = new Vector2(size, size);

            Image starImage = starObject.GetComponent<Image>();

            float alpha = Random.Range(minAlpha, maxAlpha);

            starImage.color = new Color(
                Random.Range(0.7f, 1f),
                Random.Range(0.8f, 1f),
                1f,
                alpha
            );

            starImage.raycastTarget = false;
        }
    }
}