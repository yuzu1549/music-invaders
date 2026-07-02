using UnityEngine;
using System.IO;

public class OsuConverter : MonoBehaviour
{
	[Header("osu!のファイルを.txtにしてセット")]
	public TextAsset osuTextFile;

	[Header("曲のBPM")]
	public float bpm = 158.0f;

	[Header("出力するファイル名")]
	public string outputFileName = "Mania2LaneChart.txt";

	[Header("リズムの吸着設定")]
	[Tooltip("4 = 16分音符(0.25拍刻み) / 2 = 8分音符(0.5拍刻み)")]
	public int quantizeDivision = 24;

	// コンテキストメニューから実行
	[ContextMenu("★mania譜面(2レーン)を変換して保存する★")]
	public void ConvertOsuManiaToMyChart()
	{
		if (osuTextFile == null)
		{
			Debug.LogError("osuTextFileがセットされていません！");
			return;
		}

		string[] lines = osuTextFile.text.Split('\n');
		string resultData = "";
		bool isHitObjectSection = false;
		int notesCount = 0;

		foreach (string line in lines)
		{
			string trimLine = line.Trim();

			if (trimLine == "[HitObjects]")
			{
				isHitObjectSection = true;
				continue;
			}

			if (isHitObjectSection && !string.IsNullOrEmpty(trimLine))
			{
				string[] values = trimLine.Split(',');

				if (values.Length >= 3)
				{
					// 1. 時間(ミリ秒)をBeat(拍)に変換
					float timeMs = float.Parse(values[2]);
					float beat = (timeMs / 1000f) * (bpm / 60f);

					// ★追加：キリの良い数字にピタッと吸着させる（クオンタイズ）
					// quantizeDivisionが4なら、1/4拍（0.25刻み）に補正します
					float snappedBeat = Mathf.Round(beat * quantizeDivision) / quantizeDivision;

					// 2. レーンの変換（osu!の画面幅512を2分割）
					int xPos = int.Parse(values[0]);
					int lane = 0;

					if (xPos < 256)
					{
						lane = -1; // 左
					}
					else
					{
						lane = 1;  // 右
					}

					// 3. テキストに追加（補正済みの snappedBeat を書き出す）
					resultData += $"{snappedBeat:F3},{lane}\n";
					notesCount++;
				}
			}
		}

		string path = Path.Combine(Application.dataPath, outputFileName);
		File.WriteAllText(path, resultData);

		Debug.Log($"✨ ズレ補正済みの譜面を保存しました！ {notesCount}個のノーツ:\n{path}");

#if UNITY_EDITOR
		UnityEditor.AssetDatabase.Refresh();
#endif
	}
}
