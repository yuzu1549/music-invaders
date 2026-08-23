using UnityEngine;

public static class ScoreRankCalculator
{
    private const int ScorePerNoteForRank = 200;

    /// <summary>
    /// 最大スコアを計算
    /// </summary>
    /// <param name="songDatabase"></param>
    /// <param name="songName"></param>
    /// <param name="difficulty"></param>
    /// <returns></returns>
    public static int CalculateMaxScore(
        SongDatabase songDatabase,
        string songName,
        string difficulty)
    {
        if (songDatabase == null)
        {
            Debug.LogWarning("SongDatabaseが設定されていません。");
            return 0;
        }

        ChartData chart = songDatabase.FindChart(
            songName,
            difficulty
        );

        if (chart == null || chart.chartFile == null)
        {
            Debug.LogWarning(
                $"譜面が見つかりません: {songName} / {difficulty}"
            );
            return 0;
        }

        int noteCount = CountNotes(chart.chartFile);
        return noteCount * ScorePerNoteForRank;
    }

    /// <summary>
    /// ランク判定
    /// </summary>
    /// <param name="score"></param>
    /// <param name="maxScore"></param>
    /// <returns></returns>
    public static string Calculate(int score, int maxScore)
    {
        if (maxScore <= 0)
        {
            return "-";
        }

        float scoreRate = (float)score / maxScore;

        if (scoreRate >= 0.9f)
        {
            return "S";
        }

        if (scoreRate >= 0.8f)
        {
            return "A";
        }

        if (scoreRate >= 0.7f)
        {
            return "B";
        }

        if (scoreRate >= 0.6f)
        {
            return "C";
        }

        return "D";
    }

    /// <summary>
    /// ノーツの数を数える
    /// </summary>
    /// <param name="chartFile"></param>
    /// <returns></returns>
    private static int CountNotes(TextAsset chartFile)
    {
        int noteCount = 0;
        string[] lines = chartFile.text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] values = line.Trim().Split(',');

            // NoteManager.LoadChart()と同じ条件
            if (values.Length == 2)
            {
                noteCount++;
            }
        }

        return noteCount;
    }
}
