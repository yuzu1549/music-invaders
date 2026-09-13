using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GlobalButtonSE : MonoBehaviour
{
    private static GlobalButtonSE instance;

    [Header("全ボタン共通SE")]
    [SerializeField] private AudioClip buttonSE;

    [Header("SE音量")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1.0f;

    private AudioSource audioSource;


    private void Awake()
    {
        // すでに存在している場合は重複させない
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // シーンが変わっても消さない
        DontDestroyOnLoad(gameObject);


        // AudioSource取得
        audioSource = GetComponent<AudioSource>();

        // 無ければ自動追加
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;


        // シーン読み込み時の処理を登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void Start()
    {
        // 最初のシーンのButtonにもSEを設定
        RegisterAllButtons();
    }


    /// <summary>
    /// シーンが読み込まれたとき
    /// </summary>
    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        RegisterAllButtons();
    }


    /// <summary>
    /// 現在のシーンにある全Buttonへ
    /// SEを自動登録
    /// </summary>
    private void RegisterAllButtons()
    {
        Button[] buttons =
            FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );


        foreach (Button button in buttons)
        {
            if (button == null)
            {
                continue;
            }


            // 念のため二重登録を防ぐ
            button.onClick.RemoveListener(
                PlayButtonSE
            );


            // 全ButtonにSE追加
            button.onClick.AddListener(
                PlayButtonSE
            );
        }


        Debug.Log(
            "Button SE 登録完了：" +
            buttons.Length +
            "個"
        );
    }


    /// <summary>
    /// Button共通SE
    /// </summary>
    public void PlayButtonSE()
    {
        if (audioSource == null)
        {
            return;
        }


        if (buttonSE == null)
        {
            Debug.LogWarning(
                "Button SEが設定されていません。"
            );

            return;
        }


        audioSource.PlayOneShot(
            buttonSE,
            volume
        );
    }


    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -=
                OnSceneLoaded;

            instance = null;
        }
    }
}