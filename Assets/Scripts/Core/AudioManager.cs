using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("オーディオミキサー")]
    [SerializeField] private AudioMixer audioMixer;
    [Header("オーディオソース")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    private const string MasterVolumeKey = "MasterVolume";
    private const string BGMVolumeKey = "BGMVolume";
    private const string SEVolumeKey = "SEVolume";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumeSettings(); // ボリューム設定の読み込み
    }

    /// <summary>
    /// BGMを再生するメソッド
    /// </summary>
    /// <param name="clip"></param>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    /// <summary>
    /// SEを再生するメソッド
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySE(AudioClip clip)
    {
        if (clip == null) return;

        seSource.PlayOneShot(clip);
    }

    /// <summary>
    /// マスターボリュームを設定するメソッド
    /// </summary>
    /// <param name="value"></param>
    public void SetMasterVolume(float value)
    {
        SetVolume(MasterVolumeKey, value);
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
    }

    /// <summary>
    /// BGMのボリュームを設定するメソッド
    /// </summary>
    /// <param name="value"></param>
    public void SetBGMVolume(float value)
    {
        SetVolume(BGMVolumeKey, value);
        PlayerPrefs.SetFloat(BGMVolumeKey, value);
    }

    /// <summary>
    /// SEのボリュームを設定するメソッド
    /// </summary>
    /// <param name="value"></param>
    public void SetSEVolume(float value)
    {
        SetVolume(SEVolumeKey, value);
        PlayerPrefs.SetFloat(SEVolumeKey, value);
    }

    /// <summary>
    /// 保存されたボリューム設定を読み込むメソッド
    /// </summary>
    private void LoadVolumeSettings()
    {
        SetVolume(MasterVolumeKey, PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        SetVolume(BGMVolumeKey, PlayerPrefs.GetFloat(BGMVolumeKey, 1f));
        SetVolume(SEVolumeKey, PlayerPrefs.GetFloat(SEVolumeKey, 1f));
    }

    /// <summary>
    /// ボリュームを設定する共通メソッド
    /// </summary>
    /// <param name="parameterName"></param>
    /// <param name="value"></param>
    private void SetVolume(string parameterName, float value)
    {
        // ボリューム値を0.0001から1の範囲にクランプ
        value = Mathf.Clamp(value, 0.0001f, 1f);

        // デシベルに変換してオーディオミキサーに設定
        float volumeDb = Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(parameterName, volumeDb);
    }
}