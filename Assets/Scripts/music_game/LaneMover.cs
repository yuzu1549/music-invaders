using UnityEngine;

public class LaneMover : MonoBehaviour
{
	[Header("レーン追従設定(0or1)")]
	[Tooltip("1ならPlayerの子になる、0なら切り離す")]
	public int move_lane = 0;

	[Header("対象のオブジェクト")]
	public Transform notesContainer; // ノーツをまとめている空オブジェクト
	public Transform player;         // プレイヤーのオブジェクト

	private int previousMoveLane = -1; // 値の変化を検知するための変数
	private Transform defaultParent;   // 最初（0の時）の親オブジェクトを記憶する用

	void Start()
	{
		// ゲーム開始時の NotesContainer の親を記憶しておく（基本はnull＝一番上）
		if (notesContainer != null)
		{
			defaultParent = notesContainer.parent;
		}
	}

	void Update()
	{
		if (notesContainer == null || player == null) return;

		// move_lane の数値が切り替わった「瞬間」だけ処理を行う
		if (move_lane != previousMoveLane)
		{
			if (move_lane == 1)
			{
				// ★ move_laneが1の時：NotesContainerをPlayerの子オブジェクトにする
				// 第2引数の true は「親子関係が変わっても、今の画面上の位置をキープする」設定です
				notesContainer.SetParent(player, true);

				Debug.Log("レーンがプレイヤーに追従を開始しました！");
			}
			else if (move_lane == 0)
			{
				// ★ move_laneが0の時：NotesContainerを元の状態（Playerから切り離す）に戻す
				notesContainer.SetParent(defaultParent, true);

				Debug.Log("レーンの追従を解除しました。");
			}

			// 現在の値を記憶して、次に切り替わるまで待機
			previousMoveLane = move_lane;
		}
	}
}
