using UnityEngine;
using MusicInvaders.Data;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [System.Serializable]
    public class ResultText
    {
        [Header("結果表示用のテキストオブジェクト")]
        public GameObject gameResultText;
        [Header("曲名表示用のテキストオブジェクト")]
        public GameObject songTitleText;
        [Header("難易度表示用のテキストオブジェクト")]
        public GameObject difficultyText;
        [Header("スコア表示用のテキストオブジェクト")]
        public GameObject scoreText;
        [Header("パーフェクト表示用のテキストオブジェクト")]
        public GameObject perfectText;
        [Header("グッド表示用のテキストオブジェクト")]
        public GameObject goodText;
        [Header("ミス表示用のテキストオブジェクト")]
        public GameObject missText;
        [Header("ランク表示用のテキストオブジェクト")]
        public GameObject rankText;
    }

    [System.Serializable]
    public class RankSprite
    {
        [Header("ランクSのスプライト")]
        public Sprite rankS;
        [Header("ランクAのスプライト")]
        public Sprite rankA;
        [Header("ランクBのスプライト")]
        public Sprite rankB;
        [Header("ランクCのスプライト")]
        public Sprite rankC;
        [Header("ランクDのスプライト")]
        public Sprite rankD;
    }

    [Header("結果表示用のテキスト")]
    [SerializeField]
    private ResultText resultText;

    [Header("ランク表示用のスプライト")]
    [SerializeField]
    private RankSprite rankSprites;

    [Header("ランク表示用のImage")]
    [SerializeField]
    private Image rankImage;

    [Header("曲データベース")]
    [SerializeField] private SongDatabaseSO songDatabase;


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
            SongInfoTextUpdate();
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
            resultText.gameResultText.GetComponent<TMPro.TextMeshProUGUI>().text = "GAME OVER";
            resultText.gameResultText.GetComponent<TMPro.TextMeshProUGUI>().color = Color.red;
            GameManager.Instance.isGameOver = false; // フラグをリセット
            
        }
        else if (GameManager.Instance.isGameCleared)
        {
            // ゲームクリア時の処理
            resultText.gameResultText.GetComponent<TMPro.TextMeshProUGUI>().text = "GAME CLEAR";
            resultText.gameResultText.GetComponent<TMPro.TextMeshProUGUI>().color = Color.yellow;
            GameManager.Instance.isGameCleared = false; // フラグをリセット
        }
    }

    private void SongInfoTextUpdate()
    {
        // 曲名と難易度の表示
        resultText.songTitleText.GetComponent<TMPro.TextMeshProUGUI>().text = GameManager.Instance.musicTitle;
        resultText.difficultyText.GetComponent<TMPro.TextMeshProUGUI>().text = GameManager.Instance.difficulty;
    }

    /// <summary>
    /// スコア、パーフェクト、グッド、ミスの表示を更新する
    /// </summary>
    private void ScoreJudgeUpdate()
    {
        // スコア、パーフェクト、グッド、ミスの表示
        resultText.scoreText.GetComponent<TMPro.TextMeshProUGUI>().text = "スコア：" + GameManager.Instance.score;
        resultText.perfectText.GetComponent<TMPro.TextMeshProUGUI>().text = "Perfect：" + GameManager.Instance.perfectCount;
        resultText.goodText.GetComponent<TMPro.TextMeshProUGUI>().text = "Good：" + GameManager.Instance.goodCount;
        resultText.missText.GetComponent<TMPro.TextMeshProUGUI>().text = "Miss：" + GameManager.Instance.missCount;
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
        resultText.rankText.GetComponent<TMPro.TextMeshProUGUI>().text = "ランク：";
        switch (rank)
        {
            case "S":
                rankImage.sprite = rankSprites.rankS;
                break;
            case "A":
                rankImage.sprite = rankSprites.rankA;
                break;
            case "B":
                rankImage.sprite = rankSprites.rankB;
                break;
            case "C":
                rankImage.sprite = rankSprites.rankC;
                break;
            case "D":
                rankImage.sprite = rankSprites.rankD;
                break;
            default:
                Debug.LogWarning("不明なランク: " + rank);
                break;
        }
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
