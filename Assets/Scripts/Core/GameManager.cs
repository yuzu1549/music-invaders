using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    public bool isGameOver = false; // ゲームオーバーフラグ
    public bool isGameCleared = false; // ゲームクリアフラグ
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
        isGameCleared = false;

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
        if (isGameOver || isGameCleared)
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

        StartCoroutine(LoadResultScene());

        Debug.Log("Game Over");
    }

    /// <summary>
    /// ゲームクリア処理を行うメソッド
    /// </summary>
    public void GameClear()
    {
        if (isGameCleared || isGameOver)
        {
            return;
        }

        isGameCleared = true;

        if (gameOverText != null)
        {
            gameOverText.text = "GAME CLEAR";
            gameOverText.fontSize = 100;
            gameOverText.gameObject.SetActive(true);
        }

        // ゲーム全体を停止
        Time.timeScale = 0f;

        StartCoroutine(LoadResultScene());

        Debug.Log("Game Clear");
    }

    private IEnumerator LoadResultScene()
    {
        yield return new WaitForSecondsRealtime(4f); // 4秒待機（リアルタイムで待機）
        Time.timeScale = 1f; // 時間を元に戻す
        SceneManager.LoadScene("ResultScene");
    }



}
