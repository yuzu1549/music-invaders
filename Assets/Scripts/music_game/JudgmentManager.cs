using UnityEngine;
using UnityEngine.InputSystem;

public class JudgmentManager : MonoBehaviour
{
	[Header("判定時間（秒） 60fps基準")]
	public float perfectWindow = 0.0666f; // ±4フレーム (66.6ms)
	public float goodWindow = 0.1000f;    // ±6フレーム (100.0ms)
	public float missWindow = 0.1666f;    // ±10フレーム (166.6ms)

	[Header("キー設定（New Input System用）")]
	public Key leftKey = Key.F;
	public Key centerKey = Key.Space;
	public Key rightKey = Key.J;

	void Update()
	{
		if (Keyboard.current == null) return;

		if (Keyboard.current[leftKey].wasPressedThisFrame) CheckHit(-1);
		if (Keyboard.current[centerKey].wasPressedThisFrame) CheckHit(0);
		if (Keyboard.current[rightKey].wasPressedThisFrame) CheckHit(1);
	}

	void CheckHit(int laneIndex)
    {
        // 【ログ1】そもそもキー入力が届いているか？
        Debug.Log($"★キーが押されました！ チェックするレーン番号: {laneIndex}");

        GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");
        
        // 【ログ2】「Note」タグのついたオブジェクトが見つかっているか？
        Debug.Log($"★画面内にある 'Note' タグのオブジェクト数: {notes.Length}");

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
            // 【ログ3】見つかったノーツとの「時間のズレ」は何秒か？
            Debug.Log($"★ターゲット発見！ 判定ラインとのズレ: {minTimeDiff} 秒 (MISS判定は {missWindow} 秒以内)");

            if (minTimeDiff <= perfectWindow)
            {
                Debug.Log("PERFECT!!");
				// targetNote（ゲームオブジェクト）から直接スクリプトを取得して即時消去
				Pseudo3DNote script = targetNote.GetComponent<Pseudo3DNote>();
				if (script != null) script.HitAndDespawn();
			}
            else if (minTimeDiff <= goodWindow)
            {
                Debug.Log("GOOD!");
				// targetNote（ゲームオブジェクト）から直接スクリプトを取得して即時消去
				Pseudo3DNote script = targetNote.GetComponent<Pseudo3DNote>();
				if (script != null) script.HitAndDespawn();
			}
            else if (minTimeDiff <= missWindow)
            {
                Debug.Log("MISS (判定枠内だけど早すぎ/遅すぎ)");
				// targetNote（ゲームオブジェクト）から直接スクリプトを取得して即時消去
				Pseudo3DNote script = targetNote.GetComponent<Pseudo3DNote>();
				if (script != null) script.HitAndDespawn();
			}
            else
            {
                // 【ログ4】ズレが大きすぎる場合
                Debug.Log("★スルー：ノーツがまだ遠すぎます！");
            }
        }
        else
        {
            // 【ログ5】レーンが一致するノーツがない場合
            Debug.Log($"★警告：レーン {laneIndex} に対応するノーツが画面内にありません。");
        }
    }
}
