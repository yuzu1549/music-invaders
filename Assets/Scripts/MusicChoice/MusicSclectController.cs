using UnityEngine;
using UnityEngine.InputSystem;

public class MusicSelectController : MonoBehaviour
{
    [SerializeField] private SongItem[] songItems;

    private string[] songNames =
    {
        "titel1",
        "title2",
        "title3",
        "title4",
        "title5"
    };

    // 真ん中に表示する曲の番号
    private int centerSongIndex = 0;

    // 5つの表示枠のうち、真ん中は2番目
    private const int centerItemIndex = 2;

    private void Start()
    {
        UpdateSongList();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            MoveDown();
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            MoveUp();
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
    }

    private void MoveUp()
    {
        centerSongIndex--;

        if (centerSongIndex < 0)
        {
            centerSongIndex = songNames.Length - 1;
        }

        UpdateSongList();
    }

    private void UpdateSongList()
    {
        for (int i = 0; i < songItems.Length; i++)
        {
            // i が 2 のとき、offset は 0
            // i が 0 のとき、offset は -2
            // i が 4 のとき、offset は 2
            int offset = i - centerItemIndex;

            int songIndex = centerSongIndex + offset;

            // 配列の範囲外になったらローテーションさせる
            if (songIndex < 0)
            {
                songIndex += songNames.Length;
            }

            if (songIndex >= songNames.Length)
            {
                songIndex -= songNames.Length;
            }

            songItems[i].SetTitle(songNames[songIndex]);

            // 真ん中だけ選択状態にする
            songItems[i].SetSelected(i == centerItemIndex);
        }
    }
}