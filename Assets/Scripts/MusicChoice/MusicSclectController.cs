using UnityEngine;
using UnityEngine.UI;

public class MusicSelectController : MonoBehaviour
{
    [Header("曲選択UI")]
    [Tooltip("上・中央・下の順番で3つ登録してください")]
    [SerializeField] private SongItem[] songItems;

    [SerializeField] private SongInfoPanel songInfoPanel;

    [Header("曲移動ボタン")]
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    [Header("難易度ボタン")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;

    [Header("READY画面")]
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button readyBackButton;

    [Header("設定画面")]
    [Tooltip("OptionsPanelではなく、親のOptionsOverlayを登録してください")]
    [SerializeField] private GameObject settingsPanel;

    private readonly string[] songNames =
    {
        "ShiningStar",
        "title2",
        "title3"
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

    // 中央に表示されている曲
    private int centerSongIndex = 0;

    // 最初の難易度
    private int selectedDifficultyIndex = 1;

    private string currentSongName = string.Empty;
    private string currentDifficultyName = string.Empty;

    public string CurrentSongName => currentSongName;
    public string CurrentDifficultyName => currentDifficultyName;

    // SongItemが3つなので中央はElement 1
    private const int centerItemIndex = 1;

    private bool isSettingsOpen = false;
    private bool isReadyOpen = false;

    private void Start()
    {
        RegisterButtonEvents();

        isSettingsOpen = false;
        isReadyOpen = false;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (readyPanel != null)
        {
            readyPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "Ready Panelが登録されていません。"
            );
        }

        UpdateSelectedValues();
        UpdateSongList();
        UpdateRightPanel();
        UpdateDifficultySelection(false);
    }

    /// <summary>
    /// ボタンのイベントを登録する
    /// </summary>
    private void RegisterButtonEvents()
    {
        if (upButton != null)
        {
            upButton.onClick.AddListener(MoveUp);
        }
        else
        {
            Debug.LogWarning(
                "Up Buttonが登録されていません。"
            );
        }

        if (downButton != null)
        {
            downButton.onClick.AddListener(MoveDown);
        }
        else
        {
            Debug.LogWarning(
                "Down Buttonが登録されていません。"
            );
        }

        if (easyButton != null)
        {
            easyButton.onClick.AddListener(SelectEasy);
        }
        else
        {
            Debug.LogWarning(
                "Easy Buttonが登録されていません。"
            );
        }

        if (normalButton != null)
        {
            normalButton.onClick.AddListener(SelectNormal);
        }
        else
        {
            Debug.LogWarning(
                "Normal Buttonが登録されていません。"
            );
        }

        if (hardButton != null)
        {
            hardButton.onClick.AddListener(SelectHard);
        }
        else
        {
            Debug.LogWarning(
                "Hard Buttonが登録されていません。"
            );
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(StartSelectedGame);
        }
        else
        {
            Debug.LogWarning(
                "Play Buttonが登録されていません。"
            );
        }

        if (readyBackButton != null)
        {
            readyBackButton.onClick.AddListener(CloseReadyPanel);
        }
        else
        {
            Debug.LogWarning(
                "Ready Back Buttonが登録されていません。"
            );
        }
    }

    /// <summary>
    /// ボタンのイベントを解除する
    /// </summary>
    private void OnDestroy()
    {
        if (upButton != null)
        {
            upButton.onClick.RemoveListener(MoveUp);
        }

        if (downButton != null)
        {
            downButton.onClick.RemoveListener(MoveDown);
        }

        if (easyButton != null)
        {
            easyButton.onClick.RemoveListener(SelectEasy);
        }

        if (normalButton != null)
        {
            normalButton.onClick.RemoveListener(SelectNormal);
        }

        if (hardButton != null)
        {
            hardButton.onClick.RemoveListener(SelectHard);
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveListener(StartSelectedGame);
        }

        if (readyBackButton != null)
        {
            readyBackButton.onClick.RemoveListener(CloseReadyPanel);
        }
    }

    /// <summary>
    /// 上の三角ボタンを押したとき
    /// </summary>
    public void MoveUp()
    {
        if (isSettingsOpen || isReadyOpen)
        {
            return;
        }

        centerSongIndex--;

        if (centerSongIndex < 0)
        {
            centerSongIndex = songNames.Length - 1;
        }

        UpdateMusicDisplay();

        Debug.Log(
            "上へ移動：" +
            currentSongName
        );
    }

    /// <summary>
    /// 下の三角ボタンを押したとき
    /// </summary>
    public void MoveDown()
    {
        if (isSettingsOpen || isReadyOpen)
        {
            return;
        }

        centerSongIndex++;

        if (centerSongIndex >= songNames.Length)
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
    /// 曲表示を更新する
    /// </summary>
    private void UpdateMusicDisplay()
    {
        UpdateSelectedValues();
        UpdateSongList();
        UpdateRightPanel();
    }

    public void SelectEasy()
    {
        SelectDifficultyAndShowReady(0);
    }

    public void SelectNormal()
    {
        SelectDifficultyAndShowReady(1);
    }

    public void SelectHard()
    {
        SelectDifficultyAndShowReady(2);
    }

    /// <summary>
    /// 難易度を選択し、READY画面を表示する
    /// </summary>
    private void SelectDifficultyAndShowReady(
        int difficultyIndex
    )
    {
        if (isSettingsOpen || isReadyOpen)
        {
            return;
        }

        if (difficultyIndex < 0 ||
            difficultyIndex >= difficultyNames.Length)
        {
            Debug.LogWarning(
                "存在しない難易度が指定されました。"
            );

            return;
        }

        selectedDifficultyIndex = difficultyIndex;

        UpdateSelectedValues();
        UpdateDifficultySelection(true);
        ShowReadyPanel();

        Debug.Log(
            "選択：" +
            currentSongName +
            " / " +
            currentDifficultyName
        );
    }

    /// <summary>
    /// READY画面を表示する
    /// </summary>
    private void ShowReadyPanel()
    {
        isReadyOpen = true;

        if (readyPanel != null)
        {
            readyPanel.SetActive(true);
        }
    }

    /// <summary>
    /// BACKボタンでREADY画面を閉じる
    /// </summary>
    public void CloseReadyPanel()
    {
        isReadyOpen = false;

        if (readyPanel != null)
        {
            readyPanel.SetActive(false);
        }

        // 難易度の選択表示を通常状態に戻す
        UpdateDifficultySelection(false);

        Debug.Log(
            "READY画面を閉じました。"
        );
    }

    /// <summary>
    /// 選択した曲と難易度でゲームを開始する
    /// </summary>
    public void StartSelectedGame()
    {
        if (!isReadyOpen || isSettingsOpen)
        {
            return;
        }

        UpdateSelectedValues();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame(
                currentSongName,
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
    /// 現在の曲名と難易度名を更新する
    /// </summary>
    private void UpdateSelectedValues()
    {
        currentSongName =
            songNames[centerSongIndex];

        currentDifficultyName =
            difficultyNames[selectedDifficultyIndex];
    }

    /// <summary>
    /// 上・中央・下の曲名を更新する
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

        for (int i = 0; i < songItems.Length; i++)
        {
            if (songItems[i] == null)
            {
                continue;
            }

            int offset = i - centerItemIndex;

            int songIndex =
                centerSongIndex + offset;

            while (songIndex < 0)
            {
                songIndex += songNames.Length;
            }

            while (songIndex >= songNames.Length)
            {
                songIndex -= songNames.Length;
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
    /// 右側の曲情報を更新する
    /// </summary>
    private void UpdateRightPanel()
    {
        if (songInfoPanel == null)
        {
            return;
        }

        songInfoPanel.SetSongInfo(
            songNames[centerSongIndex],
            jacketSprites[centerSongIndex]
        );
    }

    /// <summary>
    /// 難易度表示を更新する
    /// </summary>
    private void UpdateDifficultySelection(
        bool isSelected
    )
    {
        if (songInfoPanel == null)
        {
            return;
        }

        songInfoPanel.SetDifficultySelected(
            selectedDifficultyIndex,
            isSelected
        );
    }

    /// <summary>
    /// 設定画面を開く
    /// 別のスクリプトやButtonのOnClickから使用可能
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            return;
        }

        isSettingsOpen = true;
        settingsPanel.SetActive(true);
    }

    /// <summary>
    /// 設定画面を閉じる
    /// 別のスクリプトやButtonのOnClickから使用可能
    /// </summary>
    public void CloseSettings()
    {
        isSettingsOpen = false;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}