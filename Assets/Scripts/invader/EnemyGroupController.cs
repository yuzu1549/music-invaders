using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupController : MonoBehaviour
{
    [Header("グループ内の撃破順に対応する得点")]
    [SerializeField] private int[] defeatScores = { 100, 200, 300 };

    private readonly HashSet<EnemyHealth> activeEnemies = new();
    private readonly HashSet<EnemyHealth> defeatedEnemies = new();
    private EnemyScorePopupController scorePopupController;
    private int defeatCount;

    public int StartColumn { get; private set; }
    public int ColumnSpan { get; private set; }

    /// <summary>
    /// 空きグリッド判定に使うグループの配置列を設定する。
    /// </summary>
    /// <param name="startColumn">グループの開始列</param>
    /// <param name="columnSpan">グループが占有する列数</param>
    public void SetGridPlacement(int startColumn, int columnSpan)
    {
        StartColumn = startColumn;
        ColumnSpan = columnSpan;
    }

    /// <summary>
    /// グループ内の撃破得点を設定する。
    /// </summary>
    /// <param name="newDefeatScores">撃破順に対応する得点</param>
    public void SetDefeatScores(int[] newDefeatScores)
    {
        if (newDefeatScores == null || newDefeatScores.Length == 0)
        {
            return;
        }

        defeatScores = (int[])newDefeatScores.Clone();
    }

    /// <summary>
    /// 撃破得点のポップアップを表示する制御クラスを設定する。
    /// </summary>
    /// <param name="popupController">ポップアップを表示する制御クラス</param>
    public void SetScorePopupController(
        EnemyScorePopupController popupController)
    {
        scorePopupController = popupController;
    }

    /// <summary>
    /// 敵をグループへ登録する。
    /// </summary>
    /// <param name="enemy">登録する敵</param>
    public void RegisterEnemy(EnemyHealth enemy)
    {
        if (enemy == null || !activeEnemies.Add(enemy))
        {
            return;
        }

        enemy.transform.SetParent(transform, true);
        enemy.SetEnemyGroup(this);
    }

    /// <summary>
    /// プレイヤーが敵を撃破したことを記録し、撃破順に応じた得点を加算する。
    /// </summary>
    /// <param name="enemy">撃破された敵</param>
    public void RegisterEnemyDefeat(EnemyHealth enemy)
    {
        if (!activeEnemies.Contains(enemy) || !defeatedEnemies.Add(enemy))
        {
            return;
        }

        int defeatIndex = defeatCount;
        defeatCount++;

        if (defeatIndex >= defeatScores.Length || GameManager.Instance == null)
        {
            return;
        }

        int defeatScore = defeatScores[defeatIndex];
        GameManager.Instance.AddEnemyDefeatScore(defeatScore);
        scorePopupController?.ShowScore(defeatScore, enemy.transform.position);
    }

    /// <summary>
    /// グループに残っている敵を得点なしでプールへ戻す。
    /// </summary>
    public void DespawnRemainingEnemies()
    {
        List<EnemyHealth> enemiesToDespawn = new List<EnemyHealth>(
            activeEnemies
        );

        foreach (EnemyHealth enemy in enemiesToDespawn)
        {
            enemy?.DespawnWithoutDefeat();
        }
    }

    /// <summary>
    /// プールへ戻る敵をグループから解除する。
    /// </summary>
    /// <param name="enemy">登録を解除する敵</param>
    public void UnregisterEnemy(EnemyHealth enemy)
    {
        if (!activeEnemies.Remove(enemy))
        {
            return;
        }

        defeatedEnemies.Remove(enemy);

        if (activeEnemies.Count == 0)
        {
            Destroy(gameObject);
        }
    }
}
