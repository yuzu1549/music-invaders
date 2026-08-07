using UnityEngine;

// static（静的）クラスにすることで、シーンが変わっても中身が消えなくなります
public static class GameSceneArgs
{
    public static string SelectedMusic;
    public static string SelectedArtist;
    public static string SelectedDifficulty;
}
