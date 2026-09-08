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
    [Header("ランク表示用のテキストオブジェクト")]
    [SerializeField] private GameObject rankText;

    void Start()
    {
        Result();
    }

    /// <summary>
    /// 結果を表示する
    /// </summary>
    private void Result()
    {
        // ゲームオーバーまたはゲームクリアの状態を監視
        if (GameManager.Instance != null)
        {
            TitleTextUpdate();
            ScoreJudgeUpdate();
            HighScoreUpdate();
            RankUpdate();

        }
    }

    /// <summary>
    /// タイトルテキストを更新する
    /// </summary>
    private void TitleTextUpdate()
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
    }

    /// <summary>
    /// スコア、パーフェクト、グッド、ミスの表示を更新する
    /// </summary>
    private void ScoreJudgeUpdate()
    {
        // スコア、パーフェクト、グッド、ミスの表示
        scoreText.GetComponent<TMPro.TextMeshProUGUI>().text = "スコア：" + GameManager.Instance.score;
        perfectText.GetComponent<TMPro.TextMeshProUGUI>().text = "Perfect：" + GameManager.Instance.perfectCount;
        goodText.GetComponent<TMPro.TextMeshProUGUI>().text = "Good：" + GameManager.Instance.goodCount;
        missText.GetComponent<TMPro.TextMeshProUGUI>().text = "Miss：" + GameManager.Instance.missCount;
    }

    /// <summary>
    /// 最高スコアを更新する
    /// </summary>
    private void HighScoreUpdate()
    {
        int currentScore = GameManager.Instance.score;

        // 最高スコアの保存
        HighScoreStorage.TryUpdate(
            GameManager.Instance.musicTitle,
            GameManager.Instance.difficulty,
            currentScore
        );
    }

    /// <summary>
    /// ランクを更新する
    /// </summary>
    private void RankUpdate()
    {
        string rank = new ScoreRankCalculator().Calculate(GameManager.Instance.score, GameManager.Instance.maxScore);
        rankText.GetComponent<TMPro.TextMeshProUGUI>().text = "ランク：" + rank;
    }

    /// <summary>
    /// 結果画面が閉じられるときにスコアをリセットする
    /// </summary>
    private void OnDestroy()
    {
        // 結果画面が閉じられるときにスコアをリセット
        GameManager.Instance.score = 0;
        GameManager.Instance.perfectCount = 0;
        GameManager.Instance.goodCount = 0;
        GameManager.Instance.missCount = 0;
    }
}
