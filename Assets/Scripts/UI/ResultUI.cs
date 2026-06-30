using UnityEngine;

public class ResultUI : MonoBehaviour
{
    [Header("結果表示用のテキストオブジェクト")]
    [SerializeField] private GameObject gameResultText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
                
            }
            else if (GameManager.Instance.isGameCleared)
            {
                // ゲームクリア時の処理
                gameResultText.GetComponent<TMPro.TextMeshProUGUI>().text = "GAME CLEAR";
            }
        }
    }
}
