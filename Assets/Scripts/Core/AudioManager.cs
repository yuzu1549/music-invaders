using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音量調整に使用する AudioMixer")]
    [SerializeField] private AudioMixer audioMixer;
    [Header("BGM 再生用 AudioSource")]
    [SerializeField] private AudioSource bgmSource;
    [Header("SE 再生用 AudioSource")]
    [SerializeField] private AudioSource seSource;

    private const string MasterVolumeKey = "MasterVolume";
    private const string BGMVolumeKey = "BGMVolume";
    private const string SEVolumeKey = "SEVolume";
    private const float MinVolumeRatio = 0.0001f;
    private const float MaxVolumeRatio = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumeSettings();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ApplyVolumeSettings();
    }

    /// <summary>
    /// BGM を再生する。
    /// </summary>
    /// <param name="clip">再生する BGM</param>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    /// <summary>
    /// SE を再生する。
    /// </summary>
    /// <param name="clip">再生する SE</param>
    /// <param name="volumeScale">SE ごとの音量倍率</param>
    public void PlaySE(AudioClip clip, float volumeScale = 1.0f)
    {
        if (clip == null) return;
        if (seSource == null) return;

        float safeVolumeScale = Mathf.Clamp01(volumeScale);
        seSource.PlayOneShot(clip, safeVolumeScale);
    }

    /// <summary>
    /// Master 音量を設定する。
    /// </summary>
    /// <param name="value">0.0 から 1.0 の音量</param>
    public void SetMasterVolume(float value)
    {
        SetVolume(MasterVolumeKey, value);
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
    }

    /// <summary>
    /// BGM 音量を設定する。
    /// </summary>
    /// <param name="value">0.0 から 1.0 の音量</param>
    public void SetBGMVolume(float value)
    {
        SetVolume(BGMVolumeKey, value);
    }

    /// <summary>
    /// SE 音量を設定する。
    /// </summary>
    /// <param name="value">0.0 から 1.0 の音量</param>
    public void SetSEVolume(float value)
    {
        SetVolume(SEVolumeKey, value);
    }

    /// <summary>
    /// GameSettings に保存されている BGM / SE 音量を AudioMixer に反映する。
    /// </summary>
    public void ApplyVolumeSettings()
    {
        SetBGMVolume(GameSettings.BgmVolumeNormalized);
        SetSEVolume(GameSettings.SeVolumeNormalized);
    }

    /// <summary>
    /// 保存済みの音量設定を読み込んで AudioMixer に反映する。
    /// </summary>
    private void LoadVolumeSettings()
    {
        SetVolume(MasterVolumeKey, PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        ApplyVolumeSettings();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyVolumeSettings();
    }

    /// <summary>
    /// AudioMixer の指定パラメータへ音量を反映する。
    /// </summary>
    /// <param name="parameterName">AudioMixer に公開されているパラメータ名</param>
    /// <param name="value">0.0 から 1.0 の音量</param>
    private void SetVolume(string parameterName, float value)
    {
        if (audioMixer == null) return;

        value = Mathf.Clamp(value, MinVolumeRatio, MaxVolumeRatio);

        float volumeDb = Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(parameterName, volumeDb);
    }
}
