using UnityEngine;

public class ResultUI : MonoBehaviour
{
    [Header("結果表示用のテキストオブジェクト")]
    [SerializeField] private GameObject gameResultText;
    [Header("スコア表示用のテキストオブジェクト")]
    [SerializeField] private GameObject scoreText;
    [Header("パーフェクト表示用のテキストオブジェクト")]
    [SerializeField] private GameObject perfectText;
    [Header("グッド表示用のテキストオブジェクト")]
    [SerializeField] private GameObject goodText;
    [Header("ミス表示用のテキストオブジェクト")]
    [SerializeField] private GameObject missText;

    void Start()
    {
        Result();
    }

    private void Result()
    {
        // ゲームオーバーまたはゲームクリアの状態を監視
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isGameOver)
            {
                // ゲームオーバー時の処理
                gameResultText.GetComponent<TMPro.TextMeshProUGUI>().text = "GAME OVER";
                GameManager.Instance.isGameOver = false; // フラグをリセット
                
            }
            else if (GameManager.Instance.isGameCleared)
            {
                // ゲームクリア時の処理
                gameResultText.GetComponent<TMPro.TextMeshProUGUI>().text = "GAME CLEAR";
                GameManager.Instance.isGameCleared = false; // フラグをリセット
            }

            // スコア、パーフェクト、グッド、ミスの表示
            scoreText.GetComponent<TMPro.TextMeshProUGUI>().text = "スコア：" + GameManager.Instance.score;
            perfectText.GetComponent<TMPro.TextMeshProUGUI>().text = "Perfect：" + GameManager.Instance.perfectCount;
            goodText.GetComponent<TMPro.TextMeshProUGUI>().text = "Good：" + GameManager.Instance.goodCount;
            missText.GetComponent<TMPro.TextMeshProUGUI>().text = "Miss：" + GameManager.Instance.missCount;

            GameManager.Instance.score = 0; // スコアをリセット
            GameManager.Instance.perfectCount = 0; // パーフェクト数をリセット
            GameManager.Instance.goodCount = 0; // グッド数をリセット
            GameManager.Instance.missCount = 0; // ミス数をリセット
        }
    }
}
