using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DifficultyBox : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text difficultyNameText;

    private Color normalColor = new Color(0.20f, 0.18f, 0.35f, 1.0f);
    private Color selectedColor = new Color(1.0f, 0.1f, 0.65f, 1.0f);

    public void SetSelected(bool isSelected)
    {
        backgroundImage.color = isSelected ? selectedColor : normalColor;
    }

    private void Start()
    {
        SetSelected(false);
    }
}