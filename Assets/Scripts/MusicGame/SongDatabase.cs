using UnityEngine;
using System.Collections.Generic;

// 譜面のデータ（難易度名とファイル）
[System.Serializable]
public class ChartData
{
	public string difficultyName; // 例："Hard"
	public TextAsset chartFile;   // テキストファイル
}

// 1曲分のデータ（曲名、音源、譜面のリスト）
[System.Serializable]
public class SongData
{
	public string songName;       // 例："test1"
	public AudioClip bgm;         // 音源ファイル
	public float bpm = 120.0f;

	[Header("音源のオフセット（秒）")]
	[SerializeField] private float songOffsetSeconds = 0f;

	[Header("敵が動き始める拍")]
	[Tooltip("0の場合、曲オフセットの位置から敵が動き始めます")]
	[Min(0)]
	[SerializeField] private int enemyStartBeat = 0;

	public List<ChartData> charts; // この曲の各難易度の譜面リスト

	public float SongOffsetSeconds => songOffsetSeconds;
	public int EnemyStartBeat => Mathf.Max(0, enemyStartBeat);
}

// データベース本体
public class SongDatabase : MonoBehaviour
{
	[Header("ここに全楽曲のデータを登録します")]
	public List<SongData> songs = new List<SongData>();

	// 登録されたリストの中から、名前が一致する曲を探し出す機能
	public SongData FindSong(string name)
	{
		return songs.Find(s => s.songName == name);
	}

	public ChartData FindChart(string songName,string difficulty)
	{
		SongData song = FindSong(songName);

		if (song == null || song.charts == null)
		{
			return null;
		}

		string normalizedDifficulty =
			difficulty == "Difficult" ? "Hard" : difficulty;

		return song.charts.Find(
			chart => chart.difficultyName == normalizedDifficulty
		);
	}
}
