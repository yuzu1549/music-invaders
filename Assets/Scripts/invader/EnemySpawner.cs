using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("敵")]
    [SerializeField] private EnemySnakeMover enemyPrefab;

    [Header("スポーン設定")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int initialEnemyCount = 5;
    [SerializeField] private float enemySpacing = 1f;
    [SerializeField] private float rowSpacing = 1f;
    [SerializeField] private Transform spawnPoint;

    [Header("敵の移動設定")]
    [SerializeField] private float enemySpeed = 3f;
    [SerializeField] private float enemyDownDistance = 1f;
    [SerializeField] private EnemySnakeMover.MoveDirection firstDirection =
        EnemySnakeMover.MoveDirection.Right;

    private bool isFirstSpawn = true;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnEnemies();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemies()
    {
        if (isFirstSpawn)
        {
            SpawnRow(spawnPoint.position);
            SpawnRow(spawnPoint.position + Vector3.down * rowSpacing);
            isFirstSpawn = false;
        }
        else
        {
            SpawnRow(spawnPoint.position);
        }
    }

    private void SpawnRow(Vector3 basePosition)
    {
        for (int i = 0; i < initialEnemyCount; i++)
        {
            Vector3 spawnPosition = basePosition + Vector3.left * enemySpacing * i;

            EnemySnakeMover enemy = Instantiate(
                enemyPrefab,
                spawnPosition,
                Quaternion.identity
            );

            enemy.SetMoveSettings(
                enemySpeed,
                enemyDownDistance,
                firstDirection
            );
        }
    }























    // [Header("スポーンする敵の名前")]
    // [SerializeField] private string enemyName;

    // [Header("スポーンする間隔")]
    // [SerializeField] private float spawnInterval = 10f;

    // private float timer = 0f;
    // private GameObject enemy;

    // private void Update()
    // {
    //     timer += Time.deltaTime;
    //     if (timer >= spawnInterval)
    //     {
    //         SpawnEnemy();
    //         timer = 0f;
    //     }
    // }

    // /// <summary>
    // /// 敵をスポーンさせる処理
    // /// </summary>
    // private void SpawnEnemy()
    // {
    //     if (enemyName != null)
    //     {
    //         // プールから敵を取得
    //         enemy = PoolManager.Instance.Get(enemyName);
    //         if (enemy != null)
    //         {
    //             enemy.transform.position = Vector3.zero; // 原点にスポーン (必要に応じて変更)
    //             enemy.transform.rotation = Quaternion.identity;
    //         }
    //         else
    //         {
    //             Debug.LogError($"{name}: プールから {enemyName} を取得できませんでした。");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError($"{name}: enemyName が設定されていません。");
    //     }
    // }
}
