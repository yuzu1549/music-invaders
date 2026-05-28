using UnityEngine;
using TMPro;

/// <summary>
/// GameScene上に曲情報・再生時間・オプション設定を表示する。
/// </summary>
public class GameSceneUI : MonoBehaviour
{
    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI gameInfoText;

    [Header("Music Info")]
    [SerializeField] private string songTitle = "Music Invaders";
    [SerializeField] private string artistName = "Hokuto";
    [SerializeField] private string difficulty = "Hard";

    [Header("Time")]
    [SerializeField] private string currentTimeText = "00:00";
    [SerializeField] private string totalTimeText = "00:00";

    private void Start()
    {
        UpdateGameInfoText();
    }

    /// <summary>
    /// GameSceneの左上に表示する情報を更新する。
    /// </summary>
    private void UpdateGameInfoText()
    {
        gameInfoText.text =
            $"Title: {songTitle}\n" +
            $"Artist: {artistName}\n" +
            $"Difficulty: {difficulty}\n" +
            $"Time: {currentTimeText} / {totalTimeText}\n\n" +
            $"Options\n" +
            $"Notes Speed: {GameSettings.NoteSpeed:F1}\n" +
            $"Timing Offset: {GameSettings.TimingOffsetMs}ms";
    }
}