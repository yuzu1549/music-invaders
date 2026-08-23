using UnityEngine;
using UnityEngine.UI;

public class MusicSelectController : MonoBehaviour
{
    [Header("曲選択UI")]
    [Tooltip("上・中央・下の順番で3つ登録してください")]
    [SerializeField] private SongItem[] songItems;

    [SerializeField] private SongInfoPanel songInfoPanel;

    [Header("曲プレビュー")]
    [SerializeField] private AudioSource previewAudioSource;
    [SerializeField] private AudioClip[] songClips;

    [Header("曲移動ボタン")]
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    [Header("難易度ボタン")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;

    [Header("決定ボタン")]
    [SerializeField] private Button confirmButton;

    [Header("設定画面")]
    [Tooltip("OptionsPanelではなく、親のOptionsOverlayを登録してください")]
    [SerializeField] private GameObject settingsPanel;

    private readonly string[] songNames =
    {
        "ShiningStar",
        "MereFancy",
        "title3"
    };

    private readonly string[] artistNames =
    {
        "森田交一",
        "kさん",
        "sさん"
    };

    private readonly Sprite[] jacketSprites =
    {
        null,
        null,
        null
    };

    private readonly string[] difficultyNames =
    {
        "Easy",
        "Normal",
        "Hard"
    };

    // 各曲の難易度
    // 列の順番：
    // Easy, Normal, Hard
    private readonly int[,] difficultyStars =
    {
        { 1, 3, 4 }, // ShiningStar
        { 2, 3, 5 }, // title2
        { 1, 3, 5 }  // title3
    };

    // 中央に表示されている曲
    private int centerSongIndex = 0;

    // 初期難易度
    // 0 = Easy
    // 1 = Normal
    // 2 = Hard
    private int selectedDifficultyIndex = 1;

    private string currentSongName =
        string.Empty;

    private string currentArtistName =
        string.Empty;

    private string currentDifficultyName =
        string.Empty;

    public string CurrentSongName =>
        currentSongName;

    public string CurrentDifficultyName =>
        currentDifficultyName;

    // SongItemが3つなので中央はElement 1
    private const int centerItemIndex = 1;

    private bool isSettingsOpen = false;

    private void Start()
    {
        RegisterButtonEvents();

        isSettingsOpen = false;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // 最初からNormal
        selectedDifficultyIndex = 1;

        UpdateSelectedValues();
        UpdateSongList();
        UpdateRightPanel();
        UpdateDifficultySelection();

        PlaySelectedMusic();
    }

    /// <summary>
    /// Buttonイベントを登録
    /// </summary>
    private void RegisterButtonEvents()
    {
        if (upButton != null)
        {
            upButton.onClick.AddListener(
                MoveUp
            );
        }
        else
        {
            Debug.LogWarning(
                "Up Buttonが登録されていません。"
            );
        }

        if (downButton != null)
        {
            downButton.onClick.AddListener(
                MoveDown
            );
        }
        else
        {
            Debug.LogWarning(
                "Down Buttonが登録されていません。"
            );
        }

        if (easyButton != null)
        {
            easyButton.onClick.AddListener(
                SelectEasy
            );
        }
        else
        {
            Debug.LogWarning(
                "Easy Buttonが登録されていません。"
            );
        }

        if (normalButton != null)
        {
            normalButton.onClick.AddListener(
                SelectNormal
            );
        }
        else
        {
            Debug.LogWarning(
                "Normal Buttonが登録されていません。"
            );
        }

        if (hardButton != null)
        {
            hardButton.onClick.AddListener(
                SelectHard
            );
        }
        else
        {
            Debug.LogWarning(
                "Hard Buttonが登録されていません。"
            );
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(
                StartSelectedGame
            );
        }
        else
        {
            Debug.LogWarning(
                "Confirm Buttonが登録されていません。"
            );
        }
    }

    /// <summary>
    /// Buttonイベントを解除
    /// </summary>
    private void OnDestroy()
    {
        if (upButton != null)
        {
            upButton.onClick.RemoveListener(
                MoveUp
            );
        }

        if (downButton != null)
        {
            downButton.onClick.RemoveListener(
                MoveDown
            );
        }

        if (easyButton != null)
        {
            easyButton.onClick.RemoveListener(
                SelectEasy
            );
        }

        if (normalButton != null)
        {
            normalButton.onClick.RemoveListener(
                SelectNormal
            );
        }

        if (hardButton != null)
        {
            hardButton.onClick.RemoveListener(
                SelectHard
            );
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(
                StartSelectedGame
            );
        }
    }

    /// <summary>
    /// 上の三角ボタン
    /// </summary>
    public void MoveUp()
    {
        if (isSettingsOpen)
        {
            return;
        }

        centerSongIndex--;

        if (centerSongIndex < 0)
        {
            centerSongIndex =
                songNames.Length - 1;
        }

        UpdateMusicDisplay();

        Debug.Log(
            "上へ移動：" +
            currentSongName
        );
    }

    /// <summary>
    /// 下の三角ボタン
    /// </summary>
    public void MoveDown()
    {
        if (isSettingsOpen)
        {
            return;
        }

        centerSongIndex++;

        if (centerSongIndex >=
            songNames.Length)
        {
            centerSongIndex = 0;
        }

        UpdateMusicDisplay();

        Debug.Log(
            "下へ移動：" +
            currentSongName
        );
    }

    /// <summary>
    /// 曲に関係する表示をまとめて更新
    /// </summary>
    private void UpdateMusicDisplay()
    {
        UpdateSelectedValues();
        UpdateSongList();
        UpdateRightPanel();
        PlaySelectedMusic();
    }

    public void SelectEasy()
    {
        SelectDifficulty(0);
    }

    public void SelectNormal()
    {
        SelectDifficulty(1);
    }

    public void SelectHard()
    {
        SelectDifficulty(2);
    }

    /// <summary>
    /// 難易度を変更
    /// </summary>
    private void SelectDifficulty(
        int difficultyIndex
    )
    {
        if (isSettingsOpen)
        {
            return;
        }

        if (difficultyIndex < 0 ||
            difficultyIndex >=
            difficultyNames.Length)
        {
            Debug.LogWarning(
                "存在しない難易度が指定されました。"
            );

            return;
        }

        selectedDifficultyIndex =
            difficultyIndex;

        UpdateSelectedValues();
        UpdateDifficultySelection();

        Debug.Log(
            "選択：" +
            currentSongName +
            " / " +
            currentDifficultyName
        );
    }

    /// <summary>
    /// 決定ボタン
    /// </summary>
    public void StartSelectedGame()
    {
        if (isSettingsOpen)
        {
            return;
        }

        UpdateSelectedValues();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame(
                currentSongName,
                currentArtistName,
                currentDifficultyName
            );
        }
        else
        {
            Debug.LogWarning(
                "GameManagerが見つかりませんでした。"
            );
        }
    }

    /// <summary>
    /// 選択中の曲・作者・難易度を更新
    /// </summary>
    private void UpdateSelectedValues()
    {
        currentSongName =
            songNames[centerSongIndex];

        currentArtistName =
            artistNames[centerSongIndex];

        currentDifficultyName =
            difficultyNames[
                selectedDifficultyIndex
            ];
    }

    /// <summary>
    /// 曲一覧を更新
    /// </summary>
    private void UpdateSongList()
    {
        if (songItems == null ||
            songItems.Length != 3)
        {
            Debug.LogWarning(
                "Song ItemsのSizeを3にしてください。"
            );

            return;
        }

        for (
            int i = 0;
            i < songItems.Length;
            i++
        )
        {
            if (songItems[i] == null)
            {
                continue;
            }

            int offset =
                i - centerItemIndex;

            int songIndex =
                centerSongIndex + offset;

            while (songIndex < 0)
            {
                songIndex +=
                    songNames.Length;
            }

            while (
                songIndex >= songNames.Length
            )
            {
                songIndex -=
                    songNames.Length;
            }

            songItems[i].SetTitle(
                songNames[songIndex]
            );

            songItems[i].SetSelected(
                i == centerItemIndex
            );
        }
    }

    /// <summary>
    /// 曲情報を更新
    /// </summary>
    private void UpdateRightPanel()
    {
        if (songInfoPanel == null)
        {
            return;
        }

        // 曲名・ジャケット
        songInfoPanel.SetSongInfo(
            songNames[centerSongIndex],
            jacketSprites[centerSongIndex]
        );

        // 星
        songInfoPanel.SetDifficultyStars(
            difficultyStars[
                centerSongIndex,
                0
            ],
            difficultyStars[
                centerSongIndex,
                1
            ],
            difficultyStars[
                centerSongIndex,
                2
            ]
        );
    }

    /// <summary>
    /// 難易度選択表示
    /// </summary>
    private void UpdateDifficultySelection()
    {
        if (songInfoPanel == null)
        {
            return;
        }

        songInfoPanel.SetDifficultySelected(
            selectedDifficultyIndex
        );
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            return;
        }

        isSettingsOpen = true;
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        isSettingsOpen = false;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 選択曲をプレビュー再生
    /// </summary>
    private void PlaySelectedMusic()
    {
        if (previewAudioSource == null)
        {
            return;
        }

        if (songClips == null ||
            centerSongIndex < 0 ||
            centerSongIndex >=
            songClips.Length)
        {
            return;
        }

        AudioClip selectedClip =
            songClips[centerSongIndex];

        if (selectedClip == null)
        {
            previewAudioSource.Stop();
            return;
        }

        previewAudioSource.Stop();

        previewAudioSource.clip =
            selectedClip;

        previewAudioSource.time = 0f;

        previewAudioSource.Play();
    }
}