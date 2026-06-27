using UnityEngine;
using UnityEngine.InputSystem;

public class MusicSelectController : MonoBehaviour
{
    [SerializeField] private SongItem[] songItems;
    [SerializeField] private SongInfoPanel songInfoPanel;

    private string[] songNames =
    {
        "title1",
        "title2",
        "title3",
        "title4",
        "title5"
    };

    private Sprite[] jacketSprites =
    {
        null, null, null, null, null
    };

    private string[] difficultyNames =
    {
        "Easy",
        "Normal",
        "Difficult"
    };

    private int centerSongIndex = 0;
    private int selectedDifficultyIndex = 1;

    private const int centerItemIndex = 2;

    // false：曲選択中、true：難易度選択中
    private bool isDifficultySelectMode = false;

    private void Start()
    {
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

        if (isDifficultySelectMode == false)
        {
            UpdateMusicSelectMode();
        }
        else
        {
            UpdateDifficultySelectMode();
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

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            StartDifficultySelect();
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

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            DecideMusicAndDifficulty();
        }
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
        isDifficultySelectMode = true;

        // Enterを押して難易度選択に入ったら、Normalから始める
        selectedDifficultyIndex = 1;

        UpdateDifficultySelection();

        Debug.Log("難易度選択に移動：" + songNames[centerSongIndex]);
    }

    private void DecideMusicAndDifficulty()
    {
        string songName = songNames[centerSongIndex];
        string difficultyName = difficultyNames[selectedDifficultyIndex];

        Debug.Log("決定：" + songName + " / " + difficultyName);
    }

    private void UpdateSongList()
    {
        for (int i = 0; i < songItems.Length; i++)
        {
            int offset = i - centerItemIndex;
            int songIndex = centerSongIndex + offset;

            if (songIndex < 0)
            {
                songIndex += songNames.Length;
            }

            if (songIndex >= songNames.Length)
            {
                songIndex -= songNames.Length;
            }

            songItems[i].SetTitle(songNames[songIndex]);
            songItems[i].SetSelected(i == centerItemIndex);
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

        songInfoPanel.SetDifficultySelected(selectedDifficultyIndex, isDifficultySelectMode);
    }
}