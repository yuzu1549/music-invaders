using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongItem : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text titleText;

    private Color normalColor = new Color(0.22f, 0.20f, 0.35f, 1.0f);
    private Color selectedColor = new Color(1.0f, 0.1f, 0.65f, 1.0f);

    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            backgroundImage.color = selectedColor;
        }
        else
        {
            backgroundImage.color = normalColor;
        }
    }
}