using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UI ボタンなどからシーン遷移を行う。
/// </summary>
public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// 指定した名前のシーンを読み込む。
    /// </summary>
    /// <param name="sceneName">読み込むシーン名</param>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("読み込むシーン名が指定されていません。");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// アプリケーションを終了する。
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
