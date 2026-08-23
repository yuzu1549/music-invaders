using System.Collections.Generic;
using UnityEngine;

public class RandomEnemyAttacker : MonoBehaviour
{
    [Header("攻撃開始に使用する拍クロック")]
    [SerializeField] private MusicBeatClock musicBeatClock;

    [Header("攻撃間隔")]
    [Min(0.01f)]
    [SerializeField] private float attackInterval = 2f;
    [Header("攻撃数")]
    [SerializeField] private int attackCount = 1;

    private bool hasStartedAttackLoop;
    private bool isSubscribed;

    private void OnEnable()
    {
        TryStartOrSubscribe();
    }

    private void Start()
    {
        if (musicBeatClock == null)
        {
            Debug.LogError($"{name}: 攻撃開始に使用する拍クロックが設定されていません。");
            return;
        }

        TryStartOrSubscribe();
    }

    private void OnDisable()
    {
        UnsubscribeFromBeatClock();
        CancelInvoke(nameof(RandomEnemyAttack));
        hasStartedAttackLoop = false;
    }

    /// <summary>
    /// 楽曲開始済みなら攻撃を開始し、開始前なら最初の拍を待つ。
    /// </summary>
    private void TryStartOrSubscribe()
    {
        if (musicBeatClock == null || hasStartedAttackLoop)
        {
            return;
        }

        if (musicBeatClock.CurrentBeatIndex >= 0)
        {
            StartAttackLoop();
            return;
        }

        if (!isSubscribed)
        {
            musicBeatClock.OnBeat += HandleFirstBeat;
            isSubscribed = true;
        }
    }

    /// <summary>
    /// 最初の拍で攻撃間隔の計測を開始する。
    /// </summary>
    /// <param name="beatIndex">曲全体での拍番号</param>
    private void HandleFirstBeat(int beatIndex)
    {
        StartAttackLoop();
    }

    /// <summary>
    /// 設定した間隔で敵の攻撃を繰り返す。
    /// </summary>
    private void StartAttackLoop()
    {
        if (hasStartedAttackLoop)
        {
            return;
        }

        hasStartedAttackLoop = true;
        UnsubscribeFromBeatClock();

        float safeAttackInterval = Mathf.Max(0.01f, attackInterval);
        InvokeRepeating(
            nameof(RandomEnemyAttack),
            safeAttackInterval,
            safeAttackInterval
        );
    }

    /// <summary>
    /// 拍クロックの購読を解除する。
    /// </summary>
    private void UnsubscribeFromBeatClock()
    {
        if (!isSubscribed || musicBeatClock == null)
        {
            return;
        }

        musicBeatClock.OnBeat -= HandleFirstBeat;
        isSubscribed = false;
    }

    /// <summary>
    /// ランダムな敵を選んで攻撃させる処理
    /// </summary>
    private void RandomEnemyAttack()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<GameObject> availableEnemies = new();

        foreach (GameObject enemy in enemies)
        {
            EnemyGroupMovement groupMovement =
                enemy.GetComponentInParent<EnemyGroupMovement>();
            if (groupMovement != null && groupMovement.IsInNormalPhase)
            {
                availableEnemies.Add(enemy);
            }
        }

        if (availableEnemies.Count == 0)
        {
            return;
        }

        int attacks = Mathf.Min(attackCount, availableEnemies.Count);

        // ランダムに攻撃する敵を選ぶ
        // 一度選ばれた敵はリストから削除して、同じ敵が複数回攻撃するのを防ぐ
        for (int i = 0; i < attacks; i++)
        {
            int index = Random.Range(0, availableEnemies.Count);
            GameObject selectedEnemy = availableEnemies[index];
            availableEnemies.RemoveAt(index);

            EnemyAttack enemyAttack = selectedEnemy.GetComponent<EnemyAttack>();
            if (enemyAttack != null)
            {
                enemyAttack.Attack();
            }
        }
    }
}
