using UnityEngine;
using System.Collections.Generic;

public class NoteManager : MonoBehaviour
{
	[Header("参照")]
	public TextAsset chartText;
	public AudioSource audioSource;
	public ObjectPool pool;
	public SongDatabase songDatabase;

	[Header("表示上の判定線")]
	public Transform judgementLineTransform;

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
	private float timingOffsetSeconds = 0f;

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
		ApplySettings();

		// 譜面データを読み込む
		LoadChart();

		float secondsPerBeat = 60.0f / bpm;
		float actualDuration = GetNoteVisibleDurationSeconds();

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
			notes.RemoveAll(n => GetTargetTime(n.beat, secondsPerBeat) < debugStartTimeOffset - actualDuration);

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

	/// <summary>
	/// GameSettings に保存されているリズム設定を反映する。
	/// </summary>
	private void ApplySettings()
	{
		noteSpeed = GameSettings.NoteSpeed;
		timingOffsetSeconds = GameSettings.TimingOffsetMs / 1000f;
	}

	/// <summary>
	/// ノーツが出現してから判定ラインに到達するまでの時間を返す。
	/// </summary>
	/// <returns>ノーツ表示時間（秒）</returns>
	private float GetNoteVisibleDurationSeconds()
	{
		return 5.0f / noteSpeed;
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

		float actualDuration = GetNoteVisibleDurationSeconds();
		float secondsPerBeat = 60.0f / bpm;

		for (int i = 0; i < notes.Count; i++)
		{
			float targetTime = GetTargetTime(notes[i].beat, secondsPerBeat);

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
				float judgementLineY = GetJudgementLineLocalY(obj.transform, noteScript.endY);
				noteScript.Init(lane, actualDuration, judgementLineY);
			}
		}
	}

	/// <summary>
	/// ノーツが判定位置に到達する曲中時間を返す。
	/// </summary>
	/// <param name="beat">譜面上の拍</param>
	/// <param name="secondsPerBeat">1拍あたりの秒数</param>
	/// <returns>判定位置に到達する曲中時間</returns>
	private float GetTargetTime(float beat, float secondsPerBeat)
	{
		return (beat * secondsPerBeat) + chartOffset - timingOffsetSeconds;
	}

	/// <summary>
	/// 表示上の判定線を、ノーツの親を基準にした Y 座標へ変換する。
	/// </summary>
	/// <param name="noteTransform">生成されたノーツの Transform</param>
	/// <param name="fallbackY">判定線が未設定の場合に使う Y 座標</param>
	/// <returns>ノーツのローカル座標系における判定線の Y 座標</returns>
	private float GetJudgementLineLocalY(Transform noteTransform, float fallbackY)
	{
		if (judgementLineTransform == null)
		{
			return fallbackY;
		}

		if (noteTransform.parent == null)
		{
			return judgementLineTransform.position.y;
		}

		Vector3 localPosition = noteTransform.parent.InverseTransformPoint(judgementLineTransform.position);
		return localPosition.y;
	}

	/// <summary>
	/// 音楽の再生を停止します（外部から呼び出す用）。
	/// </summary>
	public void StopMusic()
	{
		if (audioSource == null) return;
		// 再生予約・再生を停止し、フラグをリセット
		audioSource.Stop();
		isMusicScheduled = false;
	}

	/// <summary>
	/// 音楽を一時停止します（必要なら）。
	/// </summary>
	public void PauseMusic()
	{
		if (audioSource == null) return;
		audioSource.Pause();
	}

	/// <summary>
	/// 一時停止を解除します（必要なら）。
	/// </summary>
	public void UnpauseMusic()
	{
		if (audioSource == null) return;
		audioSource.UnPause();
		isMusicScheduled = audioSource.clip != null;
	}
}
