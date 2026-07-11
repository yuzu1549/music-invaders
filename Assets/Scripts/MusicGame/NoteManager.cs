using UnityEngine;
using System.Collections.Generic;

public class NoteManager : MonoBehaviour
{
	[Header("参照")]
	public TextAsset chartText;
	public AudioSource audioSource;
	public ObjectPool pool;
	public SongDatabase songDatabase;

	[Header("設定")]
	public float bpm = 158.0f;
	public float baseDuration = 2.0f;
	public float noteSpeed = 1.0f;
	public float delayTime = 3.0f;    // 全体の開始待ち時間

	[Header("同期調整")]
	public float chartOffset = 0.0f;

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

	// 途中から始めた分の時間を記憶する変数
	private float debugStartTimeOffset = 0f;

	struct NoteData
	{
		public float beat;
		public int lane;
	}

	void Start()
	{
		// 🔥【追加】前のシーンから曲データと難易度が渡されていればセットアップする
		SetupFromArgs();

		// 譜面データを読み込む
		LoadChart();

		float secondsPerBeat = 60.0f / bpm;
		float actualDuration = baseDuration / noteSpeed;

		// デバッグモード：テスト用の途中再生処理
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

			// 既に通り過ぎているべき過去のノーツをリストから削除しておく
			notes.RemoveAll(n => (n.beat * secondsPerBeat) + chartOffset < debugStartTimeOffset - actualDuration);

			// コンソールに黄色い警告を出して、テスト中であることを知らせる
			Debug.LogWarning($"【テストモード作動中】第{startMeasure}小節（{debugStartTimeOffset:F2}秒）から再生します！");
		}

		dspStartTime = AudioSettings.dspTime;

		if (audioSource != null && audioSource.clip != null)
		{
			// 精密な時間で再生を予約する
			audioSource.PlayScheduled(dspStartTime + delayTime);
			isMusicScheduled = true;
		}
		else
		{
			Debug.LogError("AudioSourceまたはClipが設定されていません！");
		}
	}

	// 🔥【追加】シーン間のデータ引き継ぎ処理
	// 🔥修正：Resourcesではなく、SongDatabaseから検索してロードする
	// 🔥修正：曲ごとではなく「譜面（難易度）ごと」のオフセットを取得して適用する
	void SetupFromArgs()
	{
		string targetSongName = GameSceneArgs.SelectedMusic;
		string targetDifficulty = GameSceneArgs.SelectedDifficulty;

		if (string.IsNullOrEmpty(targetSongName) || string.IsNullOrEmpty(targetDifficulty))
		{
			Debug.LogWarning("データが渡されていません。インスペクターの初期設定を使用します。");
			return;
		}

		if (songDatabase == null)
		{
			Debug.LogError("❌ SongDatabase がインスペクターにセットされていません！");
			return;
		}

		// 1. データベースから曲名で検索
		SongData foundSong = songDatabase.FindSong(targetSongName);

		if (foundSong != null)
		{
			// 曲をセット
			audioSource.clip = foundSong.bgm;
			Debug.Log($"🎵 BGMをセットしました: {targetSongName}");

			// 2. その曲の中から、難易度が一致する譜面を検索
			ChartData foundChart = foundSong.charts.Find(c => c.difficultyName == targetDifficulty);

			if (foundChart != null)
			{
				// 譜面テキストをセット
				chartText = foundChart.chartFile;
				Debug.Log($"✨ 譜面をセットしました: {targetSongName} - {targetDifficulty}");

				// 🔥【変更】見つかった「譜面」固有のデフォルトオフセットを、設定用の全体オフセットに加算する
				chartOffset += foundChart.defaultOffset;
				Debug.Log($"🔧 譜面固有({targetDifficulty})のオフセットを適用しました: {foundChart.defaultOffset:F3}秒 (合計オフセット: {chartOffset:F3}秒)");
			}
			else
			{
				Debug.LogError($"❌ 難易度【{targetDifficulty}】の譜面が登録されていません！");
			}
		}
		else
		{
			Debug.LogError($"❌ 曲名【{targetSongName}】がデータベースに登録されていません！");
		}
	}

	void LoadChart()
	{
		// chartTextが空ならエラーを防ぐために処理しない
		if (chartText == null) return;

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

		// 待ち時間を過ぎて曲が鳴り始めているはずの時はオーディオの再生位置（秒）を基準にする
		if (currentDspTime >= delayTime)
		{
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
