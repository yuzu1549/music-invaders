using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    private bool isGameOver = false; // ゲームオーバーフラグ

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    /// <summary>
    /// ゲームをスタートするメソッド
    /// </summary>
    public void StartGame(AudioClip musicClip)
    {
        if (musicClip != null)
        {
            AudioManager.Instance.PlayBGM(musicClip);
        }

        // ゲーム全体を再開
        Time.timeScale = 1f;
        isGameOver = false;
    }


    /// <summary>
    /// ゲームオーバー処理を行うメソッド
    /// </summary>
    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER";
            gameOverText.fontSize = 100;
            gameOverText.gameObject.SetActive(true);
        }

        // ゲーム全体を停止
        Time.timeScale = 0f;

        Debug.Log("Game Over");
    }



}
