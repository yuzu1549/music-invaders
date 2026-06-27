using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongInfoPanel : MonoBehaviour
{
    [SerializeField] private Image jacketImage;
    [SerializeField] private TMP_Text songTitleText;

    [SerializeField] private DifficultyBox easyBox;
    [SerializeField] private DifficultyBox normalBox;
    [SerializeField] private DifficultyBox difficultBox;

    public void SetSongInfo(string title, Sprite jacketSprite)
    {
        songTitleText.text = title;

        if (jacketSprite != null)
        {
            jacketImage.sprite = jacketSprite;
            jacketImage.color = Color.white;
        }
        else
        {
            jacketImage.sprite = null;
            jacketImage.color = new Color(0.45f, 0.45f, 0.45f, 1.0f);
        }
    }

    public void SetDifficultySelected(int selectedDifficultyIndex, bool isDifficultySelectMode)
    {
        if (isDifficultySelectMode == false)
        {
            easyBox.SetSelected(false);
            normalBox.SetSelected(false);
            difficultBox.SetSelected(false);
            return;
        }

        easyBox.SetSelected(selectedDifficultyIndex == 0);
        normalBox.SetSelected(selectedDifficultyIndex == 1);
        difficultBox.SetSelected(selectedDifficultyIndex == 2);
    }
}