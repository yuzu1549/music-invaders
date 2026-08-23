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
    public string artistName; // アーティスト名の公開プロパティ
    public int score; // スコアの公開プロパティ
    public int maxScore; // 最大スコアの公開プロパティ
    public int perfectCount; // パーフェクトの公開プロパティ
    public int goodCount; // グッドの公開プロパティ
    public int missCount; // ミスの公開プロパティ
    public event Action OnGameStatsChanged; // スコアまたは判定数の変更を通知するイベント
    public event Action<string> OnMusicPlayed; // 音楽再生を通知するイベント

    private JudgementManager registeredJudgementManager;

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
        if (scene.name == "GameScene")
        {
            isGameOver = false;
            isGameCleared = false;
            score = 0;
            perfectCount = 0;
            goodCount = 0;
            missCount = 0;
            RegisterJudgementHandler(); // 判定結果に応じてスコアやカウントを更新するメソッドを登録

            OnGameStatsChanged?.Invoke(); // スコアや判定数の初期化を通知
        }
    }

    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        UnregisterJudgementHandler();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    private void RegisterJudgementHandler()
    {
        JudgementManager currentJudgementManager = JudgementManager.Instance;

        if (currentJudgementManager == null)
        {
            return;
        }

        if (registeredJudgementManager == currentJudgementManager)
        {
            return;
        }

        UnregisterJudgementHandler();

        currentJudgementManager.OnJudgement += RecordJudgement;
        registeredJudgementManager = currentJudgementManager;
    }

    private void UnregisterJudgementHandler()
    {
        if (registeredJudgementManager == null)
        {
            registeredJudgementManager = null;
            return;
        }

        registeredJudgementManager.OnJudgement -= RecordJudgement;
        registeredJudgementManager = null;
    }

    /// <summary>
    /// ゲームをスタートするメソッド
    /// </summary>
    public void StartGame(string musicTitle, string artistName, string difficulty)
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
        this.artistName = artistName;
        this.difficulty = normalizedDifficulty;

        GameSceneArgs.SelectedMusic = musicTitle;
        GameSceneArgs.SelectedArtist = artistName;
        GameSceneArgs.SelectedDifficulty = normalizedDifficulty;

		SceneManager.LoadScene("GameScene");



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
    /// 判定結果に応じて判定数を更新する。
    /// </summary>
    /// <param name="judgement">判定結果</param>
    private void RecordJudgement(string judgement)
    {
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

        OnGameStatsChanged?.Invoke();
    }

    /// <summary>
    /// 敵撃破によって獲得したスコアを加算する。
    /// </summary>
    /// <param name="scoreAmount">加算するスコア</param>
    public void AddEnemyDefeatScore(int scoreAmount)
    {
        if (scoreAmount <= 0)
        {
            return;
        }

        score += scoreAmount;
        OnGameStatsChanged?.Invoke();
    }
}
