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
	public float chartOffset = 0.0f;

	// ★追加：インスペクターで一目でテスト用とわかるように目立たせる
	[Header("■■■ デバッグ・テスト専用 ■■■")]
	[Tooltip("ONにすると下の小節から曲と譜面を途中再生します")]
	public bool isDebugMode = false;

	[Tooltip("何小節目からスタートするか（1で曲の最初から）")]
	public int startMeasure = 1;

	[Tooltip("1小節を何拍とするか（一般的な曲は4拍子なので4）")]
	public int beatsPerMeasure = 4;

	private List<NoteData> notes = new List<NoteData>();
	private double dspStartTime;
	private bool isMusicScheduled = false;

	// ★追加：途中から始めた分の時間を記憶する変数
	private float debugStartTimeOffset = 0f;

	struct NoteData
	{
		public float beat;
		public int lane;
	}

	void Start()
	{
		LoadChart();

		float secondsPerBeat = 60.0f / bpm;
		float actualDuration = baseDuration / noteSpeed;

		// ★追加：テスト用の途中再生処理
		if (isDebugMode && startMeasure > 1)
		{
			// 開始する拍数 = (指定した小節 - 1) × 1小節の拍数
			float startBeat = (startMeasure - 1) * beatsPerMeasure;
			debugStartTimeOffset = startBeat * secondsPerBeat;

			if (audioSource != null && audioSource.clip != null)
			{
				// 曲の再生位置（秒）を、指定した小節まで一気に進める
				audioSource.time = Mathf.Min(debugStartTimeOffset, audioSource.clip.length);
			}

			// ★重要：途中から始めたことによって、「既に通り過ぎているべき過去のノーツ」をリストから削除しておく
			// （これをしないと、開始した瞬間に過去のノーツが100個くらい同時に降ってきてバグります）
			notes.RemoveAll(n => (n.beat * secondsPerBeat) + chartOffset < debugStartTimeOffset - actualDuration);

			// コンソールにも黄色い警告を出して、テスト中であることを知らせる
			Debug.LogWarning($"【テストモード作動中】第{startMeasure}小節（{debugStartTimeOffset:F2}秒）から再生します！");
		}

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
		float currentMusicTime;

		// ★超重要：ここが同期ズレを防ぐ心臓部です！
		if (currentDspTime >= delayTime)
		{
			// 待ち時間を過ぎて曲が鳴り始めているはずの時は、
			// 絶対にズレない「実際のオーディオの再生位置（秒）」を基準にする！
			currentMusicTime = audioSource.time;
		}
		else
		{
			// まだ曲が鳴る前の待ち時間（delayTime中）は、時計の計算でノーツを先出しする
			currentMusicTime = (float)(currentDspTime - delayTime) + debugStartTimeOffset;
		}

		float actualDuration = baseDuration / noteSpeed;
		float secondsPerBeat = 60.0f / bpm;

		for (int i = 0; i < notes.Count; i++)
		{
			float targetTime = (notes[i].beat * secondsPerBeat) + chartOffset;

			// 判定ラインに到達する時間 - ノーツが移動する時間
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
