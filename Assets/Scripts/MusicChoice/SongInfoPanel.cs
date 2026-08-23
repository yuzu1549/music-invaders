using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongInfoPanel : MonoBehaviour
{
    [Header("曲情報")]
    [SerializeField] private Image jacketImage;
    [SerializeField] private TMP_Text songTitleText;

    [Header("難易度")]
    [SerializeField] private DifficultyBox easyBox;
    [SerializeField] private DifficultyBox normalBox;
    [SerializeField] private DifficultyBox difficultBox;

    [Header("難易度の星")]
    [SerializeField] private TMP_Text easyStarsText;
    [SerializeField] private TMP_Text normalStarsText;
    [SerializeField] private TMP_Text hardStarsText;

    /// <summary>
    /// 曲名とジャケット画像を更新する
    /// </summary>
    public void SetSongInfo(
        string title,
        Sprite jacketSprite
    )
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

            jacketImage.color =
                new Color(
                    0.45f,
                    0.45f,
                    0.45f,
                    1.0f
                );
        }
    }

    /// <summary>
    /// 選択中の難易度を更新する
    /// 0 = Easy
    /// 1 = Normal
    /// 2 = Hard
    /// </summary>
    public void SetDifficultySelected(
        int selectedDifficultyIndex
    )
    {
        easyBox.SetSelected(
            selectedDifficultyIndex == 0
        );

        normalBox.SetSelected(
            selectedDifficultyIndex == 1
        );

        difficultBox.SetSelected(
            selectedDifficultyIndex == 2
        );
    }

    /// <summary>
    /// Easy / Normal / Hard の星数を更新する
    /// </summary>
    public void SetDifficultyStars(
        int easyStars,
        int normalStars,
        int hardStars
    )
    {
        if (easyStarsText != null)
        {
            easyStarsText.text =
                CreateStarText(easyStars);
        }

        if (normalStarsText != null)
        {
            normalStarsText.text =
                CreateStarText(normalStars);
        }

        if (hardStarsText != null)
        {
            hardStarsText.text =
                CreateStarText(hardStars);
        }
    }

    /// <summary>
    /// 例：
    /// 1 → ★☆☆☆☆
    /// 3 → ★★★☆☆
    /// 5 → ★★★★★
    /// </summary>
    private string CreateStarText(
        int starCount
    )
    {
        // 0～5の範囲に制限
        starCount = Mathf.Clamp(
            starCount,
            0,
            5
        );

        string result = "";

        for (int i = 0; i < 5; i++)
        {
            if (i < starCount)
            {
                result += "★";
            }
            else
            {
                result += "☆";
            }
        }

        return result;
    }
}