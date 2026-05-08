using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class NoteManager : MonoBehaviour
{
	public TextAsset chartText;     // 譜面テキストファイル
	public GameObject notePrefab;   // ノーツのプレハブ
									
	public ObjectPool pool;

	[Header("設定")]
	public float noteSpeed = 1.0f;
	public float baseDuration = 2.0f;

	private float startTime;
	private List<NoteData> notes = new List<NoteData>();

	// 譜面データの構造体
	struct NoteData
	{
		public float time;
		public int lane;
	}

	void Start()
	{
		LoadChart();
		startTime = Time.time;
	}

	void LoadChart()
	{
		// テキストを1行ずつ読み込む
		string[] lines = chartText.text.Split('\n');
		foreach (string line in lines)
		{
			if (string.IsNullOrEmpty(line)) continue;
			string[] values = line.Split(',');
			if (values.Length == 2)
			{
				notes.Add(new NoteData
				{
					time = float.Parse(values[0]),
					lane = int.Parse(values[1])
				});
			}
		}
	}

	void Update()
	{
		float elapsedTime = Time.time - startTime;
		float actualDuration = baseDuration / noteSpeed;

		// 次に流れてくるノーツをチェック
		for (int i = 0; i < notes.Count; i++)
		{
			// 重要：判定時間にちょうど届くように「落下時間」を逆算して生成する
			if (elapsedTime >= notes[i].time - actualDuration)
			{
				SpawnNote(notes[i].lane);
				notes.RemoveAt(i);
				i--;
			}
		}
	}

	void SpawnNote(int lane)
	{
		// プールのメソッド名は Get()
		GameObject obj = pool.Get();

		if (obj != null)
		{
			Pseudo3DNote noteScript = obj.GetComponent<Pseudo3DNote>();
			if (noteScript != null)
			{
				// レーンを渡して、座標計算を開始させる
				noteScript.Init(lane);
			}
		}
	}
}
