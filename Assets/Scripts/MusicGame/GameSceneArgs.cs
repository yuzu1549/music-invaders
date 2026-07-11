using UnityEngine;

// static（静的）クラスにすることで、シーンが変わっても中身が消えなくなります
public static class GameSceneArgs
{
	public static string SelectedMusic;  // 選ばれた曲
	public static string SelectedDifficulty; // 選ばれた難易度
}
