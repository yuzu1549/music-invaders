using UnityEngine;

public class RandomEnemyAttacker : MonoBehaviour
{
    [Header("攻撃間隔")]
    [SerializeField] private float attackInterval = 2f;
    [Header("攻撃数")]
    [SerializeField] private int attackCount = 1;

    private void Start()
    {
        InvokeRepeating(nameof(RandomEnemyAttack), attackInterval, attackInterval);
    }

    /// <summary>
    /// ランダムな敵を選んで攻撃させる処理
    /// </summary>
    private void RandomEnemyAttack()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0) return;

        var availableEnemies = new System.Collections.Generic.List<GameObject>(enemies);
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