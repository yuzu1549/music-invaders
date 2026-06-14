using UnityEngine;
using TMPro;

/// IntegrationScenes上に曲情報・時間・オプション・スコア情報を表示する。
public class IntegrationSceneUI : MonoBehaviour
{
    [Header("Left UI Text")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI artistText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI optionTitleText;
    [SerializeField] private TextMeshProUGUI notesSpeedText;
    [SerializeField] private TextMeshProUGUI timingOffsetText;

    [Header("Right UI Text")]
    [SerializeField] private TextMeshProUGUI lifeText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI perfectCountText;
    [SerializeField] private TextMeshProUGUI goodCountText;
    [SerializeField] private TextMeshProUGUI missCountText;

    [Header("Music Clip")]
    [SerializeField] private AudioClip musicClip;

    [Header("Music Info")]
    [SerializeField] private string artistName = "Unknown";
    [SerializeField] private string difficulty = "Unknown";

    [Header("Test Timer")]
    [SerializeField] private bool startTimerOnAwake = true;

    [Header("Test Score Info")]
    [SerializeField] private int score = 0;

    private float currentTime = 0f;
    private bool isTimerRunning = false;

    private void Start()
    {
        if (startTimerOnAwake)
        {
            isTimerRunning = true;
        }

        UpdateAllTexts();
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            currentTime += Time.deltaTime;

            if (musicClip != null && currentTime > musicClip.length)
            {
                currentTime = musicClip.length;
                isTimerRunning = false;
            }
        }

        UpdateAllTexts();
    }

    private void UpdateAllTexts()
    {
        UpdateMusicTexts();
        UpdateOptionTexts();
        UpdateScoreTexts();
    }

    private void UpdateMusicTexts()
    {
        string songTitle = "Unknown";
        string currentTimeText = FormatTime(currentTime);
        string totalTimeText = "00:00";

        if (musicClip != null)
        {
            songTitle = musicClip.name;
            totalTimeText = FormatTime(musicClip.length);
        }

        if (titleText != null)
        {
            titleText.text = $"Title: {songTitle}";
            titleText.fontSize = 24;
        }

        if (artistText != null)
        {
            artistText.text = $"Artist: {artistName}";
            artistText.fontSize = 24;
        }

        if (difficultyText != null)
        {
            difficultyText.text = $"Difficulty: {difficulty}";
            difficultyText.fontSize = 24;
        }

        if (timeText != null)
        {
            timeText.text = $"Time:\n{currentTimeText} / {totalTimeText}";
            timeText.fontSize = 32;
        }
    }

    private void UpdateOptionTexts()
    {
        if (optionTitleText != null)
        {
            optionTitleText.text = "Option";
            optionTitleText.fontSize = 28;
        }

        if (notesSpeedText != null)
        {
            notesSpeedText.text = $"Notes Speed: {GameSettings.NoteSpeed:F1}";
            notesSpeedText.fontSize = 24;
        }

        if (timingOffsetText != null)
        {
            timingOffsetText.text = $"Timing Offset: {GameSettings.TimingOffsetMs} ms";
            timingOffsetText.fontSize = 24;
        }
    }

    private void UpdateScoreTexts()
    {
        if (lifeText != null)
        {
            lifeText.text = "Life:♥♥♥";
            lifeText.fontSize = 40;
        }

        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
            scoreText.fontSize = 40;
        }

        if (perfectCountText != null)
        {
            perfectCountText.text = "P: 0";
            perfectCountText.fontSize = 40;
        }

        if (goodCountText != null)
        {
            goodCountText.text = "G: 0";
            goodCountText.fontSize = 40;
        }

        if (missCountText != null)
        {
            missCountText.text = "M: 0";
            missCountText.fontSize = 40;
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return $"{minutes:00}:{seconds:00}";
    }
}