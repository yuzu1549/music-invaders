using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameFinish : MonoBehaviour
{
    [Header("ゲームオーバー・ゲームクリア表示用のテキスト")]
    [SerializeField] private TMPro.TextMeshProUGUI gameOverText;
    [Header("ゲームクリア・ゲームオーバー時のBGM")]
    [SerializeField] private AudioClip gameClearSound;
    [SerializeField] private AudioClip gameOverSound;
    private NoteManager noteManager;

    private void Awake()
    {
        noteManager = FindFirstObjectByType<NoteManager>();
        if (noteManager == null)
        {
            Debug.LogWarning("NoteManager がシーンに見つかりません。BGM 操作は AudioManager にフォールバックします。");
        }
    }


    /// <summary>
    /// ゲームオーバー処理を行うメソッド
    /// </summary>
    public void GameOver()
    {
        if (GameManager.Instance.isGameOver || GameManager.Instance.isGameCleared)
        {
            return;
        }

        GameManager.Instance.isGameOver = true;

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER";
            gameOverText.fontSize = 100;
            gameOverText.gameObject.SetActive(true);
        }

        // ゲーム全体を停止
        Time.timeScale = 0f;
        if (noteManager != null)
        {
            noteManager.StopMusic(); // NoteManager 管理の BGM を停止
        }
        else
        {
            AudioManager.Instance.PauseBGM(); // フォールバック
        }

        if (gameOverSound != null)
        {
            AudioManager.Instance.PlaySE(gameOverSound); // ゲームオーバー音を再生
        }

        StartCoroutine(LoadResultScene());

        Debug.Log("Game Over");
    }

    /// <summary>
    /// ゲームクリア処理を行うメソッド
    /// </summary>
    public void GameClear()
    {
        if (GameManager.Instance.isGameCleared || GameManager.Instance.isGameOver)
        {
            return;
        }

        GameManager.Instance.isGameCleared = true;

        if (gameOverText != null)
        {
            gameOverText.text = "GAME CLEAR";
            gameOverText.fontSize = 100;
            gameOverText.gameObject.SetActive(true);
        }

        // ゲーム全体を停止
        Time.timeScale = 0f;
        if (noteManager != null)
        {
            noteManager.StopMusic(); // NoteManager 管理の BGM を停止
        }
        else
        {
            AudioManager.Instance.PauseBGM(); // フォールバック
        }

        if (gameClearSound != null)
        {
            Debug.Log("Playing game clear sound");
            AudioManager.Instance.PlaySE(gameClearSound); // ゲームクリア音を再生
        }

        StartCoroutine(LoadResultScene());

        Debug.Log("Game Clear");
    }

    private IEnumerator LoadResultScene()
    {
        yield return new WaitForSecondsRealtime(4f); // 4秒待機（リアルタイムで待機）
        GameManager.Instance.UnregisterJudgementHandler();
        //GameManager.Instance.isGameOver = false; // ゲームオーバーフラグをリセット
        //GameManager.Instance.isGameCleared = false; // ゲームクリアフラグをリセット
        gameOverText.gameObject.SetActive(false); // 結果表示を非表示にする
        Time.timeScale = 1f; // 時間を元に戻す
        SceneManager.LoadScene("ResultScene");
    }
}
