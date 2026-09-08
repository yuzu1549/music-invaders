using UnityEngine;

public static class HighScoreStorage
{
    private const string KeyPrefix = "HighScore";

    /// <summary>
    /// 最高スコアを取得する
    /// </summary>
    /// <param name="songId"></param>
    /// <param name="difficulty"></param>
    /// <returns></returns>
    public static int Get(string songId, string difficulty)
    {
        return PlayerPrefs.GetInt(CreateKey(songId, difficulty), 0);
    }

    /// <summary>
    /// 最高記録を更新した場合は true を返す。
    /// </summary>
    /// <param name="songId"></param>
    /// <param name="difficulty"></param>
    /// <param name="score"></param>
    /// <returns></returns>
    public static bool TryUpdate(
        string songId,
        string difficulty,
        int score)
    {
        string key = CreateKey(songId, difficulty);
        int currentHighScore = PlayerPrefs.GetInt(key, 0);

        if (score <= currentHighScore)
        {
            return false;
        }

        PlayerPrefs.SetInt(key, score);
        PlayerPrefs.Save();
        return true;
    }

    /// <summary>
    /// キーを生成する
    /// </summary>
    /// <param name="songId"></param>
    /// <param name="difficulty"></param>
    /// <returns></returns>
    private static string CreateKey(
        string songId,
        string difficulty)
    {
        return $"{KeyPrefix}.{songId}.{NormalizeDifficulty(difficulty)}";
    }

    /// <summary>
    /// 難易度の表記を正規化する
    /// </summary>
    /// <param name="difficulty"></param>
    /// <returns></returns>
    private static string NormalizeDifficulty(string difficulty)
    {
        return difficulty == "Difficult" ? "Hard" : difficulty;
    }
}