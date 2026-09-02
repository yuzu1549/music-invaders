using UnityEngine;

public class EnemyGroupScoreSettings : MonoBehaviour
{
    [Header("グループ内の撃破順に対応する得点")]
    [SerializeField] private int[] groupDefeatScores = { 100, 200, 300 };

    [Header("撃破得点のポップアップを表示する制御クラス")]
    [SerializeField] private EnemyScorePopupController scorePopupController;

    /// <summary>
    /// 敵グループへ撃破得点とポップアップ表示先を設定する。
    /// </summary>
    /// <param name="enemyGroup">設定対象の敵グループ</param>
    public void ApplyTo(EnemyGroupController enemyGroup)
    {
        if (enemyGroup == null)
        {
            return;
        }

        enemyGroup.SetDefeatScores(groupDefeatScores);
        enemyGroup.SetScorePopupController(scorePopupController);
    }

    /// <summary>
    /// 撃破得点の設定が利用可能か確認する。
    /// </summary>
    /// <param name="errorMessage">利用できない場合の理由</param>
    /// <returns>設定を利用できる場合は true</returns>
    public bool TryValidate(out string errorMessage)
    {
        if (groupDefeatScores == null || groupDefeatScores.Length == 0)
        {
            errorMessage = "グループ内の撃破得点が設定されていません。";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
