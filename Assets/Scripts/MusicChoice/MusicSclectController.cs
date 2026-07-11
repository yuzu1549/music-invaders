using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MusicSelectController : MonoBehaviour
{
    [Header("曲選択UI")]
    [SerializeField] private SongItem[] songItems;
    [SerializeField] private SongInfoPanel songInfoPanel;

    [Header("READY画面")]
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private Button playButton;

    [Header("設定画面")]
    [Tooltip("OptionsPanelではなく、親のOptionsOverlayを登録してください")]
    [SerializeField] private GameObject settingsPanel;

    [Header("シーン")]
    [SerializeField] private string titleSceneName = "TitleScene";

    private readonly string[] songNames =
    {
        "title1",
        "title2",
        "title3",
        "title4",
        "title5"
    };

    private readonly Sprite[] jacketSprites =
    {
        null,
        null,
        null,
        null,
        null
    };

    private readonly string[] difficultyNames =
    {
        "Easy",
        "Normal",
        "Difficult"
    };

    private int centerSongIndex = 0;
    private int selectedDifficultyIndex = 1;

    private const int centerItemIndex = 2;

    private enum SelectMode
    {
        MusicSelect,
        DifficultySelect,
        Ready
    }

    private SelectMode currentMode = SelectMode.MusicSelect;

    private bool isSettingsOpen = false;

    private void Start()
    {
        currentMode = SelectMode.MusicSelect;
        isSettingsOpen = false;

        if (readyPanel != null)
        {
            readyPanel.SetActive(false);
        }

        // OptionsOverlay全体を非表示にする
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "Settings PanelにOptionsOverlayを登録してください。"
            );
        }

        UpdateSongList();
        UpdateRightPanel();
        UpdateDifficultySelection();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        // 設定画面が開いている場合
        if (isSettingsOpen)
        {
            // SまたはEscで設定画面を閉じる
            if (Keyboard.current.sKey.wasPressedThisFrame ||
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseSettings();
            }

            return;
        }

        // どの選択画面にいてもSキーで設定画面を開く
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            OpenSettings();
            return;
        }

        switch (currentMode)
        {
            case SelectMode.MusicSelect:
                UpdateMusicSelectMode();
                break;

            case SelectMode.DifficultySelect:
                UpdateDifficultySelectMode();
                break;

            case SelectMode.Ready:
                UpdateReadyMode();
                break;
        }
    }

    private void UpdateMusicSelectMode()
    {
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            MoveDown();
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            MoveUp();
        }

        if (IsEnterPressed())
        {
            StartDifficultySelect();
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            BackToTitleScene();
        }
    }

    private void UpdateDifficultySelectMode()
    {
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            selectedDifficultyIndex++;

            if (selectedDifficultyIndex >= difficultyNames.Length)
            {
                selectedDifficultyIndex = 0;
            }

            UpdateDifficultySelection();
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            selectedDifficultyIndex--;

            if (selectedDifficultyIndex < 0)
            {
                selectedDifficultyIndex = difficultyNames.Length - 1;
            }

            UpdateDifficultySelection();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            BackToMusicSelect();
            return;
        }

        if (IsEnterPressed())
        {
            ShowReadyPanel();
        }
    }

    private void UpdateReadyMode()
    {
        if (IsEnterPressed())
        {
            if (playButton != null)
            {
                playButton.onClick.Invoke();
            }
            else
            {
                Debug.LogWarning(
                    "Play Buttonが登録されていません。"
                );
            }

            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            BackToDifficultySelect();
        }
    }

    private bool IsEnterPressed()
    {
        return Keyboard.current.enterKey.wasPressedThisFrame ||
               Keyboard.current.numpadEnterKey.wasPressedThisFrame;
    }

    private void MoveDown()
    {
        centerSongIndex++;

        if (centerSongIndex >= songNames.Length)
        {
            centerSongIndex = 0;
        }

        UpdateSongList();
        UpdateRightPanel();
        UpdateDifficultySelection();
    }

    private void MoveUp()
    {
        centerSongIndex--;

        if (centerSongIndex < 0)
        {
            centerSongIndex = songNames.Length - 1;
        }

        UpdateSongList();
        UpdateRightPanel();
        UpdateDifficultySelection();
    }

    private void StartDifficultySelect()
    {
        currentMode = SelectMode.DifficultySelect;
        selectedDifficultyIndex = 1;

        if (readyPanel != null)
        {
            readyPanel.SetActive(false);
        }

        UpdateDifficultySelection();

        Debug.Log(
            "難易度選択に移動：" +
            songNames[centerSongIndex]
        );
    }

    private void BackToMusicSelect()
    {
        currentMode = SelectMode.MusicSelect;

        if (readyPanel != null)
        {
            readyPanel.SetActive(false);
        }

        UpdateDifficultySelection();

        Debug.Log(
            "曲選択に戻る：" +
            songNames[centerSongIndex]
        );
    }

    private void ShowReadyPanel()
    {
        currentMode = SelectMode.Ready;

        if (readyPanel != null)
        {
            readyPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "Ready Panelが登録されていません。"
            );
        }

        UpdateDifficultySelection();

        Debug.Log(
            "決定：" +
            songNames[centerSongIndex] +
            " / " +
            difficultyNames[selectedDifficultyIndex]
        );
    }

    private void BackToDifficultySelect()
    {
        currentMode = SelectMode.DifficultySelect;

        if (readyPanel != null)
        {
            readyPanel.SetActive(false);
        }

        UpdateDifficultySelection();

        Debug.Log(
            "難易度選択に戻る：" +
            songNames[centerSongIndex] +
            " / " +
            difficultyNames[selectedDifficultyIndex]
        );
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning(
                "Settings PanelにOptionsOverlayを登録してください。"
            );

            return;
        }

        isSettingsOpen = true;
        settingsPanel.SetActive(true);

        Debug.Log("設定画面を開きました");
    }

    public void CloseSettings()
    {
        isSettingsOpen = false;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        Debug.Log("設定画面を閉じました");
    }

    private void BackToTitleScene()
    {
        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogError(
                "Title Scene Nameが設定されていません。"
            );

            return;
        }

        SceneManager.LoadScene(titleSceneName);
    }

    private void UpdateSongList()
    {
        if (songItems == null || songItems.Length == 0)
        {
            return;
        }

        for (int i = 0; i < songItems.Length; i++)
        {
            if (songItems[i] == null)
            {
                continue;
            }

            int offset = i - centerItemIndex;
            int songIndex = centerSongIndex + offset;

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

    private void UpdateDifficultySelection()
    {
        if (songInfoPanel == null)
        {
            return;
        }

        bool isDifficultySelecting =
            currentMode == SelectMode.DifficultySelect ||
            currentMode == SelectMode.Ready;

        songInfoPanel.SetDifficultySelected(
            selectedDifficultyIndex,
            isDifficultySelecting
        );
    }
}