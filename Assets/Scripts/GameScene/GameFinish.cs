using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameFinish : MonoBehaviour
{
    [Header("ゲームオーバー・ゲームクリア表示用のテキスト")]
    [SerializeField] private TMPro.TextMeshProUGUI gameOverText;
    [Header("暗転用のパネル")]
    [SerializeField] private Image darkPanel1;
    [SerializeField] private Image darkPanel2;
    [SerializeField] private float fadeDuration = 1.0f;

    private NoteManager noteManager;
    [Header("ゲームオーバー・ゲームクリア時のサウンド")]
    [SerializeField] private AudioClip gameOverClip; // ゲームオーバー時の AudioClip をインスペクターで設定
    [SerializeField] private AudioClip gameClearClip; // ゲームクリア時の AudioClip をインスペクターで設定

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
            gameOverText.color = Color.red;
            gameOverText.gameObject.SetActive(true);
        }

        if (darkPanel1 != null)
        {
            darkPanel1.gameObject.SetActive(true);
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

        StartCoroutine(LoadResultScene());
        if (gameOverClip != null)
        {
            AudioManager.Instance.PlaySE(gameOverClip); // ゲームオーバー時のサウンドを再生
        }
        else
        {
            Debug.LogWarning("GameOverClip が設定されていません。ゲームオーバー時のサウンドを再生できません。");
        }

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
            gameOverText.color = Color.yellow;
            gameOverText.gameObject.SetActive(true);
        }

        if (darkPanel1 != null)
        {
            darkPanel1.gameObject.SetActive(true);
        }

        if (gameClearClip != null)
        {
            AudioManager.Instance.PlaySE(gameClearClip); // ゲームクリア時のサウンドを再生
        }
        else
        {
            Debug.LogWarning("GameClearClip が設定されていません。ゲームクリア時のサウンドを再生できません。");
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
        StartCoroutine(LoadResultScene());

        Debug.Log("Game Clear");
    }

    private IEnumerator LoadResultScene()
    {
        yield return new WaitForSecondsRealtime(3f); // 3.5秒待機（リアルタイムで待機）
        // 先に徐々に暗くする
        if (darkPanel2 != null)
        {
            darkPanel2.gameObject.SetActive(true);

            Color color = darkPanel2.color;
            color.a = 0f;
            darkPanel2.color = color;

            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

                color.a = alpha;
                darkPanel2.color = color;

                yield return null;
            }
        }


        //yield return new WaitForSecondsRealtime(1f); // 1秒待機（リアルタイムで待機）
        gameOverText.gameObject.SetActive(false); // 結果表示を非表示にする
        Time.timeScale = 1f; // 時間を元に戻す
        SceneManager.LoadScene("ResultScene");
    }
}
