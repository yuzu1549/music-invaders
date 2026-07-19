using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TitleStarTwinkle : MonoBehaviour
{
    [SerializeField]
    private float minSpeed = 0.8f;

    [SerializeField]
    private float maxSpeed = 2.0f;

    private Image starImage;

    private float speed;
    private float startAlpha;
    private float offset;

    private void Awake()
    {
        starImage = GetComponent<Image>();

        speed = Random.Range(minSpeed, maxSpeed);
        startAlpha = starImage.color.a;
        offset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        float wave =
            Mathf.Sin(Time.time * speed + offset) * 0.5f + 0.5f;

        Color color = starImage.color;

        color.a = Mathf.Lerp(
            startAlpha * 0.25f,
            startAlpha,
            wave
        );

        starImage.color = color;
    }
}