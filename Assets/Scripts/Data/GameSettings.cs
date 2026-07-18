using UnityEngine;

/// <summary>
/// ゲーム設定の保存と読み込みを行う。
/// </summary>
public static class GameSettings
{
    private const string NoteSpeedKey = "NoteSpeed";	// ノーツ速度
    private const string TimingOffsetMsKey = "TimingOffsetMs";	// タイミングオフセット（ms）
    private const string BgmVolumePercentKey = "BgmVolumePercent";	// BGM音量
    private const string SeVolumePercentKey = "SeVolumePercent";	// SE音量

	[Header("ノーツ速度")]

	[Header("最小ノーツ速度")]
    public const float MinNoteSpeed = 1.0f;
	[Header("最大ノーツ速度")]
    public const float MaxNoteSpeed = 25.0f;
	[Header("デフォルトノーツ速度")]
    public const float DefaultNoteSpeed = 5.0f;

	[Space(15)]
	[Header("タイミング調整")]
	[Header("最小タイミングオフセット")]
    public const int MinTimingOffsetMs = -500;
	[Header("最大タイミングオフセット")]
    public const int MaxTimingOffsetMs = 500;
	[Header("デフォルトタイミングオフセット")]
    public const int DefaultTimingOffsetMs = 0;

	[Space(15)]
    [Header("音量設定")]
	[Header("最小音量")]
    public const int MinVolumePercent = 0;
	[Header("最大音量")]
    public const int MaxVolumePercent = 100;
	[Header("デフォルト音量")]
    public const int DefaultVolumePercent = 50;

    public static float NoteSpeed
    {
        get
        {
            float noteSpeed = PlayerPrefs.GetFloat(
                NoteSpeedKey,
                DefaultNoteSpeed);

            if (noteSpeed < MinNoteSpeed || MaxNoteSpeed < noteSpeed)
            {
                NoteSpeed = DefaultNoteSpeed;
                Save();
                return DefaultNoteSpeed;
            }

            return RoundToOneDecimal(noteSpeed);
        }
        set => PlayerPrefs.SetFloat(
            NoteSpeedKey,
            Mathf.Clamp(RoundToOneDecimal(value), MinNoteSpeed, MaxNoteSpeed));
    }

    public static int TimingOffsetMs
    {
        get => PlayerPrefs.GetInt(TimingOffsetMsKey, DefaultTimingOffsetMs);
        set => PlayerPrefs.SetInt(
            TimingOffsetMsKey,
            Mathf.Clamp(value, MinTimingOffsetMs, MaxTimingOffsetMs));
    }

    public static int BgmVolumePercent
    {
        get => PlayerPrefs.GetInt(BgmVolumePercentKey, DefaultVolumePercent);
        set => PlayerPrefs.SetInt(
            BgmVolumePercentKey,
            Mathf.Clamp(value, MinVolumePercent, MaxVolumePercent));
    }

    public static int SeVolumePercent
    {
        get => PlayerPrefs.GetInt(SeVolumePercentKey, DefaultVolumePercent);
        set => PlayerPrefs.SetInt(
            SeVolumePercentKey,
            Mathf.Clamp(value, MinVolumePercent, MaxVolumePercent));
    }

    public static float BgmVolumeNormalized
    {
        get => BgmVolumePercent / 100f;
    }

    public static float SeVolumeNormalized
    {
        get => SeVolumePercent / 100f;
    }

    /// <summary>
    /// ノーツが画面に表示されてから判定位置へ到達するまでの時間を秒で返す。
    /// </summary>
    /// <returns>ノーツ表示時間</returns>
    public static float GetNoteVisibleDurationSeconds()
    {
        return 5.0f / NoteSpeed;
    }

    /// <summary>
    /// 現在の設定を保存する。
    /// </summary>
    public static void Save()
    {
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 小数第1位に丸める。
    /// </summary>
    /// <param name="value">丸める値</param>
    /// <returns>小数第1位に丸めた値</returns>
    private static float RoundToOneDecimal(float value)
    {
        return Mathf.Round(value * 10f) / 10f;
    }
}
