using UnityEngine;
using TMPro;

/// 画面上部右側にライフ・スコア・判定数を表示する。
public class LifeUI : MonoBehaviour
{
    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI lifeText;

    [Header("Life")]
    [SerializeField] private int maxLife = 3;
    [SerializeField] private int currentLife = 3;

    [Header("Temporary Display")]
    [SerializeField] private string scoreText = "xxx";
    [SerializeField] private string perfectText = "xxx";
    [SerializeField] private string goodText = "xxx";
    [SerializeField] private string missText = "xxx";

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (lifeText == null) return;

        string heartText = "";

        for (int i = 0; i < maxLife; i++)
        {
            heartText += i < currentLife ? "♥ " : "♡ ";
        }

        lifeText.text =
            $"Leave Life: {heartText}\n" +
            $"Score: {scoreText}\n" +
            $"P:{perfectText}, G:{goodText}, M:{missText}";
    }
}