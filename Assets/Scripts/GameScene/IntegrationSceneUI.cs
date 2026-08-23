using UnityEngine;
using TMPro;

/// IntegrationScenes上に曲情報・時間・オプション・スコア情報を表示する。
public class IntegrationSceneUI : MonoBehaviour
{
    [Header("Left UI Text")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI artistText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI rankText;
    //[SerializeField] private TextMeshProUGUI timeText;
    //[SerializeField] private TextMeshProUGUI optionTitleText;
    //[SerializeField] private TextMeshProUGUI notesSpeedText;
    //[SerializeField] private TextMeshProUGUI timingOffsetText;

    [Header("Right UI Text")]
    [SerializeField] private TextMeshProUGUI lifeText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI perfectCountText;
    [SerializeField] private TextMeshProUGUI goodCountText;
    [SerializeField] private TextMeshProUGUI missCountText;

    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI gameOverText;

    //[Header("Music Clip")]
    //[SerializeField] private AudioClip musicClip;

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicAudioSource;

    [Header("Test Timer")]
    [SerializeField] private bool startTimerOnAwake = true;

    [Header("Player Health")]
    [SerializeField] private PlayerHealth playerHealth;

    private float currentTime = 0f;
    private bool isTimerRunning = false;
    private bool isGameOver = false;

    private void Start()
    {
        // 前回のゲームオーバーなどで止まったままにならないようにする
        Time.timeScale = 1f;

        if (startTimerOnAwake)
        {
            isTimerRunning = true;
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        UpdateAllTexts();
        UpdateScoreTexts();

        GameManager.Instance.OnGameStatsChanged += UpdateScoreTexts;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStatsChanged -= UpdateScoreTexts;
        }
    }

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        if (isTimerRunning)
        {
            currentTime += Time.deltaTime;

            //if (musicClip != null && currentTime > musicClip.length)
            //{
                //currentTime = musicClip.length;
                //isTimerRunning = false;
            //
            //}
        }

        UpdateAllTexts();
    }

    private void UpdateAllTexts()
    {
        UpdateMusicTexts();
        UpdateOptionTexts();
        UpdateLifeText();
    }

    private void UpdateMusicTexts()
    {
        //string currentTimeText = FormatTime(currentTime);

        //if (musicClip != null)
        //{
            //songTitle = musicClip.name;
            //totalTimeText = FormatTime(musicClip.length);
        //}

        if (titleText != null)
        {
            if (string.IsNullOrEmpty(GameManager.Instance.musicTitle))
            {
                titleText.text = "Title: Unknown";
            }
            else
            {
                titleText.text = $"Title: {GameManager.Instance.musicTitle}";
            }
            titleText.fontSize = 40;
        }

        if (artistText != null)
        {
            if (string.IsNullOrEmpty(GameManager.Instance.artistName))
            {
                artistText.text = "Artist\nUnknown";
            }
            else
            {
                artistText.text = $"Artist\n{GameManager.Instance.artistName}";
            }

            artistText.fontSize = 40;
        }

        if (difficultyText != null)
        {
            if (string.IsNullOrEmpty(GameManager.Instance.difficulty))
            {
                difficultyText.text = "Difficulty: Unknown";
            }
            else
            {
                difficultyText.text = $"Difficulty: {GameManager.Instance.difficulty}";
            }
            difficultyText.fontSize = 40;
        }

        if (highScoreText != null)
        {
            int highScore = HighScoreStorage.Get(
                GameManager.Instance.musicTitle,
                GameManager.Instance.difficulty
            );

            highScoreText.text = $"最高スコア：{highScore}";
            highScoreText.fontSize = 32;
        }

        // 最高スコアと最高ランクのUI
        if (rankText != null)
        {
            int highScore = HighScoreStorage.Get(
                GameManager.Instance.musicTitle,
                GameManager.Instance.difficulty
            );

            rankText.text = $"ランク：{ScoreRankCalculator.Calculate(highScore,GameManager.Instance.maxScore)}";
            rankText.fontSize = 32;
        }

        //[if (timeText != null)
        //{
            //timeText.text = $"Time:\n{currentTimeText} / {totalTimeText}";
            //timeText.fontSize = 32;
        //}
    }

    private void UpdateOptionTexts()
    {
        // if (optionTitleText != null)
        // {
        //     optionTitleText.text = "Option";
        //     optionTitleText.fontSize = 28;
        // }

        // if (notesSpeedText != null)
        // {
        //     notesSpeedText.text = $"Notes Speed: {GameSettings.NoteSpeed:F1}";
        //     notesSpeedText.fontSize = 24;
        // }

        // if (timingOffsetText != null)
        // {
        //     timingOffsetText.text = $"Timing Offset: {GameSettings.TimingOffsetMs} ms";
        //     timingOffsetText.fontSize = 24;
        // }
    }

    private void UpdateScoreTexts()
    {

        if (scoreText != null)
        {
            scoreText.text = $"Score: {GameManager.Instance.score}";
            scoreText.fontSize = 40;
        }

        if (perfectCountText != null)
        {
            perfectCountText.text = $"P: {GameManager.Instance.perfectCount}";
            perfectCountText.fontSize = 40;
        }

        if (goodCountText != null)
        {
            goodCountText.text = $"G: {GameManager.Instance.goodCount}";
            goodCountText.fontSize = 40;
        }

        if (missCountText != null)
        {
            missCountText.text = $"M: {GameManager.Instance.missCount}";
            missCountText.fontSize = 40;
        }
    }

    private void UpdateLifeText()
    {
        if (lifeText != null)
        {
            int life = 3;

            if (playerHealth != null)
            {
                life = playerHealth.currentHealth;
            }

            if (life < 0)
            {
                life = 0;
            }

            lifeText.text = "Life:" + new string('♥', life);
            lifeText.fontSize = 40;
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return $"{minutes:00}:{seconds:00}";
    }
}
