using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    public bool isGameOver = false; // ゲームオーバーフラグ
    public bool isGameCleared = false; // ゲームクリアフラグ
    public string musicTitle; // 音楽タイトルの公開プロパティ
    public string difficulty; // 難易度の公開プロパティ
    public int score; // スコアの公開プロパティ
    public int perfectCount; // パーフェクトの公開プロパティ
    public int goodCount; // グッドの公開プロパティ
    public int missCount; // ミスの公開プロパティ
    public event Action<string> OnScoreChanged; // 判定結果を通知するイベント
    public event Action<string> OnMusicPlayed; // 音楽再生を通知するイベント

    public bool isJudgementHandlerRegistered = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded; // シーンがロードされたときに呼ばれるイベントに登録
    }

    private void Start()
    {
        RegisterJudgementHandler();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Integration")
        {
            isGameOver = false;
            isGameCleared = false;
            score = 0;
            perfectCount = 0;
            goodCount = 0;
            missCount = 0;
            RegisterJudgementHandler(); // 判定結果に応じてスコアやカウントを更新するメソッドを登録

            OnScoreChanged?.Invoke(""); // スコアやカウントの初期化を通知
        }
    }

    private void RegisterJudgementHandler()
    {
        if (isJudgementHandlerRegistered)
        {
            return;
        }

        if (JudgementManager.Instance != null)
        {
            JudgementManager.Instance.OnJudgement += CostCount;
            isJudgementHandlerRegistered = true;
        }
    }

    /// <summary>
    /// ゲームをスタートするメソッド
    /// </summary>
    public void StartGame(string musicTitle, string difficulty)
    {
        string normalizedDifficulty = difficulty;

        if (string.IsNullOrWhiteSpace(normalizedDifficulty))
        {
            normalizedDifficulty = "Normal";
        }
        else if (normalizedDifficulty == "Difficult")
        {
            normalizedDifficulty = "Hard";
        }

        this.musicTitle = musicTitle;
        this.difficulty = normalizedDifficulty;
		GameSceneArgs.SelectedMusic = musicTitle;
		GameSceneArgs.SelectedDifficulty = normalizedDifficulty;

		SceneManager.LoadScene("Integration");

        

        // ゲーム全体を再開
        Time.timeScale = 1f;
        isGameOver = false;
        isGameCleared = false;
        OnMusicPlayed?.Invoke(musicTitle);

        if (normalizedDifficulty == "Easy")
        {
            // Easyモードの設定を行う
            Debug.Log("Easy mode selected");
        }
        else if (normalizedDifficulty == "Normal")
        {
            // Normalモードの設定を行う
            Debug.Log("Normal mode selected");
        }
        else if (normalizedDifficulty == "Hard")
        {
            // Hardモードの設定を行う
            Debug.Log("Hard mode selected");
        }
    }


    /// <summary>
    /// 判定結果に応じてスコアやカウントを更新するメソッド
    /// </summary>
    /// <param name="judgement">判定結果</param>
    public void CostCount(string judgement)
    {
        //Debug.Log($"判定結果: {judgement}");
        switch (judgement)
        {
            case "PERFECT":
                perfectCount++;
                break;
            case "GOOD":
                goodCount++;
                break;
            case "MISS":
                missCount++;
                break;
        }

        score = perfectCount * 20 + goodCount * 10 + missCount * 0; // スコア計算（例: PERFECT=20点, GOOD=10点, MISS=0点）

        OnScoreChanged?.Invoke(judgement);
    }



}
