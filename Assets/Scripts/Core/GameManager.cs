using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    private bool isGameOver = false; // ゲームオーバーフラグ
    public string musicTitle; // 音楽タイトルの公開プロパティ

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
    public void StartGame(AudioClip musicClip, string difficulty)
    {
        SceneManager.LoadScene("integration");

        if (musicClip != null)
        {
            AudioManager.Instance.PlayBGM(musicClip);
        }

        // ゲーム全体を再開
        Time.timeScale = 1f;
        isGameOver = false;

        if (difficulty == "Easy")
        {
            // Easyモードの設定を行う
            Debug.Log("Easy mode selected");
        }
        else if (difficulty == "Normal")
        {
            // Normalモードの設定を行う
            Debug.Log("Normal mode selected");
        }
        else if (difficulty == "Hard")
        {
            // Hardモードの設定を行う
            Debug.Log("Hard mode selected");
        }
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
