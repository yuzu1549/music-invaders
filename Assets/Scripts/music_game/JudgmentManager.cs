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

	[Header("キー設定（New Input System用）")]
	public Key leftKey = Key.F;
	public Key centerKey = Key.Space;
	public Key rightKey = Key.J;

	[Header("UI設定")]
	public TextMeshProUGUI judgmentText;

	public event Action<string> OnJudgment; // 判定結果を外部に通知するイベント

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

		if (targetNote != null)
		{
			if (minTimeDiff <= perfectWindow)
			{
				ShowJudgment("PERFECT!!", Color.yellow);
				OnJudgment?.Invoke("PERFECT");

				Pseudo3DNote script = targetNote.GetComponent<Pseudo3DNote>();
				if (script != null) script.HitAndDespawn();
			}
			else if (minTimeDiff <= goodWindow)
			{
				ShowJudgment("GOOD!", Color.green);
				OnJudgment?.Invoke("GOOD");

				Pseudo3DNote script = targetNote.GetComponent<Pseudo3DNote>();
				if (script != null) script.HitAndDespawn();
			}
			else if (minTimeDiff <= missWindow)
			{
				ShowJudgment("MISS", Color.gray);

				Pseudo3DNote script = targetNote.GetComponent<Pseudo3DNote>();
				if (script != null) script.HitAndDespawn();
			}
		}
	}

	// ★追加：ノーツが見逃されて消える時に、外部から呼び出す用のメソッド
	public void DisplayMiss()
	{
		ShowJudgment("MISS", Color.gray);
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
}
