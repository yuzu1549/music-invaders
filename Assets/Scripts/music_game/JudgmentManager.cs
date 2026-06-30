using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class JudgmentManager : MonoBehaviour
{
	// ★追加：他のスクリプト（ノーツ）から一瞬でアクセスできるようにする
	public static JudgmentManager Instance { get; private set; }

	[Header("判定時間（秒） 60fps基準")]
	public float perfectWindow = 0.0666f; // ±4フレーム (66.6ms)
	public float goodWindow = 0.1000f;    // ±6フレーム (100.0ms)
	public float missWindow = 0.1666f;    // ±10フレーム (166.6ms)

	// ★追加：インスペクターで判定ごとの効果音を設定できるようにする
	[Header("ノーツ・判定の効果音")]
	public AudioClip perfectSE;
	public AudioClip goodSE;
	public AudioClip emptyHitSE; // 空打ち（ノーツが無い時）用の音

	[Header("キー設定（New Input System用）")]
	public Key leftKey = Key.F;
	public Key centerKey = Key.Space;
	public Key rightKey = Key.J;

	[Header("UI設定")]
	public TextMeshProUGUI judgmentText;

	public event Action<string> OnJudgment; // 判定結果を外部に通知するイベント

	public int PerfectCount { get; private set; }
	public int GoodCount { get; private set; }
	public int MissCount { get; private set; }

	[Header("エフェクト")]
	public GameObject perfectEffectPrefab; // ★追加：ここに作ったエフェクトを入れる
	public GameObject goodEffectPrefab;
	public GameObject missEffectPrefab;

	// ★追加：起動時に自分自身をInstanceに登録する
	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void Update()
	{
		if (Keyboard.current == null) return;

		if (Keyboard.current[leftKey].wasPressedThisFrame) CheckHit(-1);
		if (Keyboard.current[centerKey].wasPressedThisFrame) CheckHit(0);
		if (Keyboard.current[rightKey].wasPressedThisFrame) CheckHit(1);
	}

	void CheckHit(int laneIndex)
	{
		GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");
		GameObject targetNote = null;
		float minTimeDiff = float.MaxValue;

		foreach (GameObject noteObj in notes)
		{
			Pseudo3DNote note = noteObj.GetComponent<Pseudo3DNote>();

			if (note != null && note.lane == laneIndex)
			{
				float timeDiff = Mathf.Abs(note.GetTimeDiff());

				if (timeDiff < minTimeDiff)
				{
					minTimeDiff = timeDiff;
					targetNote = noteObj;
				}
			}
		}

		// --- 🌟 ここから下が判定と音を鳴らす処理です 🌟 ---

		if (targetNote != null)
		{
			if (minTimeDiff <= perfectWindow)
			{
				PerfectCount++;
				ShowJudgment("PERFECT!!", Color.yellow);
				OnJudgment?.Invoke("PERFECT");

				// ★追加：PerfectのSEを鳴らす（ミキサーのSEグループを通る）
				AudioManager.Instance.PlaySE(perfectSE);

				Pseudo3DNote script = targetNote.GetComponent<Pseudo3DNote>();
				if (script != null) script.HitAndDespawn();
				SpawnEffect(targetNote, perfectEffectPrefab);
			}
			else if (minTimeDiff <= goodWindow)
			{
				GoodCount++;
				ShowJudgment("GOOD!", Color.green);
				OnJudgment?.Invoke("GOOD");

				// ★追加：GoodのSEを鳴らす
				AudioManager.Instance.PlaySE(goodSE);

				Pseudo3DNote script = targetNote.GetComponent<Pseudo3DNote>();
				if (script != null) script.HitAndDespawn();
				SpawnEffect(targetNote, goodEffectPrefab);
			}
			else if (minTimeDiff <= missWindow)
			{
				MissCount++;
				ShowJudgment("MISS", Color.gray);
				OnJudgment?.Invoke("MISS");

				// ★追加：MISSの時も空打ち用の音を鳴らす！
				AudioManager.Instance.PlaySE(emptyHitSE);
				SpawnEffect(targetNote, missEffectPrefab);

				Pseudo3DNote script = targetNote.GetComponent<Pseudo3DNote>();
				if (script != null) script.HitAndDespawn();
			}
			else
			{
				// ノーツはあるけれど判定範囲（missWindow）よりも大幅に手前、または後ろすぎる場合
				// これも「空打ち」として扱います
				AudioManager.Instance.PlaySE(emptyHitSE);
			}
		}
		else
		{
			// ★追加：そのレーンにノーツが1つも存在しないのにキーを押した（空打ち）の時
			AudioManager.Instance.PlaySE(emptyHitSE);
		}
	}

	// ★追加：ノーツが見逃されて消える時に、外部から呼び出す用のメソッド
	public void DisplayMiss()
	{
		MissCount++;
		ShowJudgment("MISS", Color.gray);
		OnJudgment?.Invoke("MISS");
	}

	void ShowJudgment(string resultMessage, Color textColor)
	{
		if (judgmentText != null)
		{
			judgmentText.text = resultMessage;
			judgmentText.color = textColor;
			judgmentText.gameObject.SetActive(true);

			StopAllCoroutines();
			StartCoroutine(HideJudgmentCoroutine());
		}
	}

	System.Collections.IEnumerator HideJudgmentCoroutine()
	{
		yield return new WaitForSeconds(0.5f);
		judgmentText.gameObject.SetActive(false);
	}

	private void SpawnEffect(GameObject note, GameObject effectPrefab)
	{
		// 指定されたエフェクトがちゃんとインスペクターにセットされているか確認
		if (effectPrefab != null)
		{
			// ノーツの位置に、指定されたエフェクトを生成
			Instantiate(effectPrefab, note.transform.position, Quaternion.identity);
		}
	}
}
