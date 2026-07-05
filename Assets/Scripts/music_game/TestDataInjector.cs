using UnityEngine;

public class TestDataInjector : MonoBehaviour
{
	[Header("テスト用の仮データ")]
	[Tooltip("Resources/Audio/ に入っている曲のファイル名（拡張子なし）")]
	public string testSongName = "test1";

	public string testDifficulty = "Hard";

	void Awake()
	{
		// 箱が空っぽなら、文字データをねじ込む
		if (string.IsNullOrEmpty(GameSceneArgs.SelectedMusic))
		{
			GameSceneArgs.SelectedMusic = testSongName;
			GameSceneArgs.SelectedDifficulty = testDifficulty;

			Debug.Log($"🔧【テスト作動】ダミーデータを注入しました！ 曲名: {testSongName} / 難易度: {testDifficulty}");
		}
	}
}
