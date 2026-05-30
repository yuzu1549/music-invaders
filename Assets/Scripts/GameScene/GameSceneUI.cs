using UnityEngine;
using TMPro;

/// GameScene上に曲情報・再生時間・オプション設定を表示する。
public class GameSceneUI : MonoBehaviour
{
    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI gameInfoText;

    [Header("Music Clip")]
    [SerializeField] private AudioClip musicClip;

    [Header("Music Info")]
    [SerializeField] private string artistName = "Unknown Artist";
    [SerializeField] private string difficulty = "Normal";

    [Header("Test Timer")]
    [SerializeField] private bool startTimerOnAwake = true;

    private float currentTime = 0f;
    private bool isTimerRunning = false;

    private void Start()
    {
        if (startTimerOnAwake)
        {
            isTimerRunning = true;
        }

        UpdateGameInfoText();
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

        UpdateGameInfoText();
    }

    private void UpdateGameInfoText()
    {
        if (gameInfoText == null) return;

        string songTitle = "Unknown Title";
        string currentTimeText = FormatTime(currentTime);
        string totalTimeText = "00:00";

        if (musicClip != null)
        {
            songTitle = musicClip.name;
            totalTimeText = FormatTime(musicClip.length);
        }

        gameInfoText.text =
            $"Title: {songTitle}\n" +
            $"Artist: {artistName}\n" +
            $"Difficulty: {difficulty}\n" +
            $"Time: {currentTimeText} / {totalTimeText}\n\n" +
            $"Options\n" +
            $"Notes Speed: {GameSettings.NoteSpeed:F1}\n" +
            $"Timing Offset: {GameSettings.TimingOffsetMs}ms";
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return $"{minutes:00}:{seconds:00}";
    }
}