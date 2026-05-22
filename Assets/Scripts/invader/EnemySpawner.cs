using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("スポーンする敵の名前")]
    [SerializeField] private string enemyName;

    [Header("スポーンする間隔")]
    [SerializeField] private float spawnInterval = 10f;

    private float timer = 0f;
    private GameObject enemy;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    /// <summary>
    /// 敵をスポーンさせる処理
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyName != null)
        {
            // プールから敵を取得
            enemy = PoolManager.Instance.Get(enemyName);
            if (enemy != null)
            {
                enemy.transform.position = Vector3.zero; // 原点にスポーン (必要に応じて変更)
                enemy.transform.rotation = Quaternion.identity;
            }
            else
            {
                Debug.LogError($"{name}: プールから {enemyName} を取得できませんでした。");
            }
        }
        else
        {
            Debug.LogError($"{name}: enemyName が設定されていません。");
        }
    }
}
