using UnityEngine;
using System.Collections.Generic;

public class NoteManager : MonoBehaviour
{
	[Header("参照")]
	public TextAsset chartText;
	public AudioSource audioSource;
	public ObjectPool pool;

	[Header("設定")]
	public float bpm = 158.0f;
	public float baseDuration = 2.0f;
	public float noteSpeed = 1.0f;
	public float delayTime = 3.0f;    // 全体の開始待ち時間

	[Header("同期調整")]
	// ★追加：曲に対する譜面のズレを秒単位で調整するオフセット
	public float chartOffset = 0.0f;

	private List<NoteData> notes = new List<NoteData>();
	private double dspStartTime;
	private bool isMusicScheduled = false;

	struct NoteData
	{
		public float beat;
		public int lane;
	}

	void Start()
	{
		LoadChart();

		dspStartTime = AudioSettings.dspTime;

		if (audioSource != null && audioSource.clip != null)
		{
			audioSource.PlayScheduled(dspStartTime + delayTime);
			isMusicScheduled = true;
		}
		else
		{
			Debug.LogError("AudioSourceまたはClipが設定されていません！");
		}
	}

	void LoadChart()
	{
		string[] lines = chartText.text.Split('\n');
		foreach (string line in lines)
		{
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] values = line.Split(',');
			if (values.Length == 2)
			{
				notes.Add(new NoteData
				{
					beat = float.Parse(values[0]),
					lane = int.Parse(values[1])
				});
			}
		}
	}

	void Update()
	{
		if (audioSource == null || !isMusicScheduled) return;

		double currentDspTime = AudioSettings.dspTime - dspStartTime;
		float currentMusicTime = (float)(currentDspTime - delayTime);

		float actualDuration = baseDuration / noteSpeed;
		float secondsPerBeat = 60.0f / bpm;

		for (int i = 0; i < notes.Count; i++)
		{
			// ★修正：本来のターゲット時間に、ズレ調整用のオフセット（秒）を足す
			float targetTime = (notes[i].beat * secondsPerBeat) + chartOffset;

			if (currentMusicTime >= targetTime - actualDuration)
			{
				SpawnNote(notes[i].lane, actualDuration);
				notes.RemoveAt(i);
				i--;
			}
		}
	}

	void SpawnNote(int lane, float actualDuration)
	{
		GameObject obj = pool.Get();
		if (obj != null)
		{
			Pseudo3DNote noteScript = obj.GetComponent<Pseudo3DNote>();
			if (noteScript != null)
			{
				noteScript.Init(lane, actualDuration);
			}
		}
	}
}
