using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// オプション画面の表示と設定値の変更を制御する。
/// </summary>
public class OptionsOverlayController : MonoBehaviour
{
    private const float NoteSpeedSmallStep = 0.1f;
    private const float NoteSpeedLargeStep = 1.0f;
    private const int TimingOffsetSmallStepMs = 1;
    private const int TimingOffsetLargeStepMs = 10;
    private const int VolumeSmallStepPercent = 1;
    private const int VolumeLargeStepPercent = 10;

    [Header("オプション画面のルートオブジェクト")]
    [SerializeField] private GameObject optionsOverlay;

    [Space(15)]
    [Header("ノーツ速度を操作するスライダー")]
    [SerializeField] private Slider noteSpeedSlider;
    [Header("ノーツ速度の現在値を表示するテキスト")]
    [SerializeField] private TMP_Text noteSpeedValueText;

    [Space(15)]
    [Header("タイミング調整を操作するスライダー")]
    [SerializeField] private Slider timingOffsetSlider;
    [Header("タイミング調整の現在値を表示するテキスト")]
    [SerializeField] private TMP_Text timingOffsetValueText;

    [Space(15)]
    [Header("BGM 音量を操作するスライダー")]
    [SerializeField] private Slider bgmVolumeSlider;
    [Header("BGM 音量の現在値を表示するテキスト")]
    [SerializeField] private TMP_Text bgmVolumeValueText;

    [Header("SE 音量を操作するスライダー")]
    [SerializeField] private Slider seVolumeSlider;
    [Header("SE 音量の現在値を表示するテキスト")]
    [SerializeField] private TMP_Text seVolumeValueText;

    private bool isInitializing; // Slider 初期化中かどうか

    private void Start()
    {
        isInitializing = true;

        SetupSliderRanges();
        LoadSettingsToSliders();
        RefreshTexts();
        CloseOptions();

        isInitializing = false;
    }

    /// <summary>
    /// オプション画面を表示する。
    /// </summary>
    public void OpenOptions()
    {
        optionsOverlay.SetActive(true);
    }

    /// <summary>
    /// オプション画面を非表示にする。
    /// </summary>
    public void CloseOptions()
    {
        optionsOverlay.SetActive(false);
    }

    /// <summary>
    /// ノーツ速度を保存する。
    /// </summary>
    /// <param name="value">変更後のノーツ速度</param>
    public void SetNoteSpeed(float value)
    {
        if (isInitializing)
        {
            return;
        }

        GameSettings.NoteSpeed = value;
        noteSpeedSlider.SetValueWithoutNotify(GameSettings.NoteSpeed);
        RefreshTexts();
        GameSettings.Save();
    }

    /// <summary>
    /// ノーツ速度を 0.1 下げる。
    /// </summary>
    public void DecreaseNoteSpeedSmall()
    {
        SetNoteSpeed(GameSettings.NoteSpeed - NoteSpeedSmallStep);
    }

    /// <summary>
    /// ノーツ速度を 0.1 上げる。
    /// </summary>
    public void IncreaseNoteSpeedSmall()
    {
        SetNoteSpeed(GameSettings.NoteSpeed + NoteSpeedSmallStep);
    }

    /// <summary>
    /// ノーツ速度を 1.0 下げる。
    /// </summary>
    public void DecreaseNoteSpeedLarge()
    {
        SetNoteSpeed(GameSettings.NoteSpeed - NoteSpeedLargeStep);
    }

    /// <summary>
    /// ノーツ速度を 1.0 上げる。
    /// </summary>
    public void IncreaseNoteSpeedLarge()
    {
        SetNoteSpeed(GameSettings.NoteSpeed + NoteSpeedLargeStep);
    }

    /// <summary>
    /// タイミング調整値を保存する。
    /// </summary>
    /// <param name="value">変更後のタイミング調整値</param>
    public void SetTimingOffset(float value)
    {
        if (isInitializing)
        {
            return;
        }

        GameSettings.TimingOffsetMs = Mathf.RoundToInt(value);
        timingOffsetSlider.SetValueWithoutNotify(GameSettings.TimingOffsetMs);
        RefreshTexts();
        GameSettings.Save();
    }

    /// <summary>
    /// タイミング調整値を 1ms 下げる。
    /// </summary>
    public void DecreaseTimingOffsetSmall()
    {
        SetTimingOffset(GameSettings.TimingOffsetMs - TimingOffsetSmallStepMs);
    }

    /// <summary>
    /// タイミング調整値を 1ms 上げる。
    /// </summary>
    public void IncreaseTimingOffsetSmall()
    {
        SetTimingOffset(GameSettings.TimingOffsetMs + TimingOffsetSmallStepMs);
    }

    /// <summary>
    /// タイミング調整値を 10ms 下げる。
    /// </summary>
    public void DecreaseTimingOffsetLarge()
    {
        SetTimingOffset(GameSettings.TimingOffsetMs - TimingOffsetLargeStepMs);
    }

    /// <summary>
    /// タイミング調整値を 10ms 上げる。
    /// </summary>
    public void IncreaseTimingOffsetLarge()
    {
        SetTimingOffset(GameSettings.TimingOffsetMs + TimingOffsetLargeStepMs);
    }

    /// <summary>
    /// BGM 音量を保存する。
    /// </summary>
    /// <param name="value">変更後の BGM 音量</param>
    public void SetBgmVolume(float value)
    {
        if (isInitializing)
        {
            return;
        }

        GameSettings.BgmVolumePercent = Mathf.RoundToInt(value);
        bgmVolumeSlider.SetValueWithoutNotify(GameSettings.BgmVolumePercent);
        RefreshTexts();
        GameSettings.Save();
    }

    /// <summary>
    /// BGM 音量を 1% 下げる。
    /// </summary>
    public void DecreaseBgmVolumeSmall()
    {
        SetBgmVolume(GameSettings.BgmVolumePercent - VolumeSmallStepPercent);
    }

    /// <summary>
    /// BGM 音量を 1% 上げる。
    /// </summary>
    public void IncreaseBgmVolumeSmall()
    {
        SetBgmVolume(GameSettings.BgmVolumePercent + VolumeSmallStepPercent);
    }

    /// <summary>
    /// BGM 音量を 10% 下げる。
    /// </summary>
    public void DecreaseBgmVolumeLarge()
    {
        SetBgmVolume(GameSettings.BgmVolumePercent - VolumeLargeStepPercent);
    }

    /// <summary>
    /// BGM 音量を 10% 上げる。
    /// </summary>
    public void IncreaseBgmVolumeLarge()
    {
        SetBgmVolume(GameSettings.BgmVolumePercent + VolumeLargeStepPercent);
    }

    /// <summary>
    /// SE 音量を保存する。
    /// </summary>
    /// <param name="value">変更後の SE 音量</param>
    public void SetSeVolume(float value)
    {
        if (isInitializing)
        {
            return;
        }

        GameSettings.SeVolumePercent = Mathf.RoundToInt(value);
        seVolumeSlider.SetValueWithoutNotify(GameSettings.SeVolumePercent);
        RefreshTexts();
        GameSettings.Save();
    }

    /// <summary>
    /// SE 音量を 1% 下げる。
    /// </summary>
    public void DecreaseSeVolumeSmall()
    {
        SetSeVolume(GameSettings.SeVolumePercent - VolumeSmallStepPercent);
    }

    /// <summary>
    /// SE 音量を 1% 上げる。
    /// </summary>
    public void IncreaseSeVolumeSmall()
    {
        SetSeVolume(GameSettings.SeVolumePercent + VolumeSmallStepPercent);
    }

	/// <summary>
	/// SE 音量を 10% 上げる。
	/// </summary>
	public void IncreaseSeVolumeLarge()
	{
		SetSeVolume(GameSettings.SeVolumePercent + VolumeLargeStepPercent);
	}

    /// <summary>
    /// SE 音量を 10% 下げる。
    /// </summary>
    public void DecreaseSeVolumeLarge()
    {
        SetSeVolume(GameSettings.SeVolumePercent - VolumeLargeStepPercent);
    }

    /// <summary>
    /// Slider の範囲と刻みを初期化する。
    /// </summary>
    private void SetupSliderRanges()
    {
        noteSpeedSlider.minValue = GameSettings.MinNoteSpeed;
        noteSpeedSlider.maxValue = GameSettings.MaxNoteSpeed;
        noteSpeedSlider.wholeNumbers = false;

        timingOffsetSlider.minValue = GameSettings.MinTimingOffsetMs;
        timingOffsetSlider.maxValue = GameSettings.MaxTimingOffsetMs;
        timingOffsetSlider.wholeNumbers = true;

        bgmVolumeSlider.minValue = GameSettings.MinVolumePercent;
        bgmVolumeSlider.maxValue = GameSettings.MaxVolumePercent;
        bgmVolumeSlider.wholeNumbers = true;

        seVolumeSlider.minValue = GameSettings.MinVolumePercent;
        seVolumeSlider.maxValue = GameSettings.MaxVolumePercent;
        seVolumeSlider.wholeNumbers = true;
    }

    /// <summary>
    /// 保存済みの設定値を Slider に反映する。
    /// </summary>
    private void LoadSettingsToSliders()
    {
        noteSpeedSlider.SetValueWithoutNotify(GameSettings.NoteSpeed);
        timingOffsetSlider.SetValueWithoutNotify(GameSettings.TimingOffsetMs);
        bgmVolumeSlider.SetValueWithoutNotify(GameSettings.BgmVolumePercent);
        seVolumeSlider.SetValueWithoutNotify(GameSettings.SeVolumePercent);
    }

    /// <summary>
    /// 現在の設定値を表示テキストに反映する。
    /// </summary>
    private void RefreshTexts()
    {
        noteSpeedValueText.text = $"{GameSettings.NoteSpeed:0.0}";
        timingOffsetValueText.text = FormatTimingOffset(GameSettings.TimingOffsetMs);
        bgmVolumeValueText.text = FormatVolume(GameSettings.BgmVolumePercent);
        seVolumeValueText.text = FormatVolume(GameSettings.SeVolumePercent);
    }

    /// <summary>
    /// タイミング調整値を符号付き ms 表記に変換する。
    /// </summary>
    /// <param name="timingOffsetMs">タイミング調整値</param>
    /// <returns>表示用テキスト</returns>
    private string FormatTimingOffset(int timingOffsetMs)
	{
		string sign = timingOffsetMs > 0 ? "+" : "";
		return $"{sign}{timingOffsetMs}<size=12> </size><size=24>ms</size>";
	}

    /// <summary>
    /// 音量を % 表記に変換する。
    /// </summary>
    /// <param name="volumePercent">音量</param>
    /// <returns>表示用テキスト</returns>
    private string FormatVolume(int volumePercent)
    {
        return $"{volumePercent}<size=12> </size><size=24>%</size>";
    }
}
