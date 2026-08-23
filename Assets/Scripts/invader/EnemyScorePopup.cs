using TMPro;
using UnityEngine;

public class EnemyScorePopup : MonoBehaviour
{
    [Header("獲得スコアを表示するテキスト")]
    [SerializeField] private TMP_Text scoreText;

    [Header("表示してから消えるまでの秒数")]
    [Min(0.01f)]
    [SerializeField] private float displaySeconds = 0.8f;

    [Header("表示中に上方向へ移動する距離")]
    [Min(0f)]
    [SerializeField] private float riseDistance = 0.5f;

    private Vector3 startPosition;
    private Color startColor;
    private float elapsedSeconds;
    private bool isShowing;

    private void Awake()
    {
        if (scoreText == null)
        {
            scoreText = GetComponentInChildren<TMP_Text>();
        }
    }

    private void Update()
    {
        if (!isShowing)
        {
            return;
        }

        elapsedSeconds += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedSeconds / displaySeconds);

        transform.position = startPosition +
            Vector3.up * (riseDistance * progress);

        Color currentColor = startColor;
        currentColor.a = Mathf.Lerp(startColor.a, 0f, progress);
        scoreText.color = currentColor;

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 獲得スコアを設定して表示を開始する。
    /// </summary>
    /// <param name="score">表示する獲得スコア</param>
    public void Show(int score)
    {
        if (scoreText == null)
        {
            Debug.LogError($"{name}: スコア表示用テキストが設定されていません。");
            Destroy(gameObject);
            return;
        }

        scoreText.text = score.ToString();
        startPosition = transform.position;
        startColor = scoreText.color;
        elapsedSeconds = 0f;
        isShowing = true;
    }
}
