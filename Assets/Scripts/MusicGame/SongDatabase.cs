using UnityEngine;
using System.Collections.Generic;

// 譜面のデータ（難易度名とファイル）
[System.Serializable]
public class ChartData
{
	public string difficultyName; // 例："Hard"
	public TextAsset chartFile;   // テキストファイル

	[Tooltip("この譜面固有のズレ調整（秒）。ノーツを早く出したいならマイナス、遅くしたいならプラス")]
	public float defaultOffset = 0.0f;
}

// 1曲分のデータ（曲名、音源、譜面のリスト）
[System.Serializable]
public class SongData
{
	public string songName;       // 例："test1"
	public AudioClip bgm;         // 音源ファイル
	public float bpm = 120.0f;
	public List<ChartData> charts; // この曲の各難易度の譜面リスト

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
}
