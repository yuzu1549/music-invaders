using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("敵のプールのキー")]
    [SerializeField] private string enemyPoolKey = "Invader";

    [Space(15)]
    [Header("スポーン設定")]
    [Header("スポーンする間隔")]
    [SerializeField] private float spawnInterval = 2f;
    [Header("最初のスポーンで生成する敵の数")]
    [SerializeField] private int initialEnemyCount = 5;
    [Header("2回目以降で生成する敵の数")]
    [SerializeField] private int subsequentEnemyCount = 3;
    [Header("敵同士の間隔")]
    [SerializeField] private float enemySpacing = 1f;

    private Transform spawnPoint1; // スポーンポイント(1行目)
    private Transform spawnPoint2; // スポーンポイント(2行目)

    [Space(15)]
    [Header("敵の移動設定")]
    [Header("敵の移動速度")]
    [SerializeField] private float enemySpeed = 3f;
    [Header("敵が壁に当たったときの垂直移動距離")]
    [SerializeField] private float enemyDownDistance = 1f;
    [Space(15)]
    [Header("敵スポーンデータベース")]
    [SerializeField] private EnemySpawnDatabase enemySpawnDatabase; // 敵スポーンデータベースの参照

    private bool isFirstSpawn = true; // 最初のスポーンかどうかを判定するフラグ

    private void Awake()
    {
        spawnPoint1 = transform.Find("SpawnPoint1");
        spawnPoint2 = transform.Find("SpawnPoint2");
    }

    private void Start()
    {
        StartCoroutine(SpawnLoop()); // 敵のスポーンループを開始
        EnemySpawnData spawnData = enemySpawnDatabase.GetByName(GameManager.Instance.musicTitle + GameManager.Instance.difficulty); // 敵スポーンデータを取得
        if (spawnData != null)
        {
            spawnInterval = spawnData.spawnInterval;
            initialEnemyCount = spawnData.initialEnemyCount;
            subsequentEnemyCount = spawnData.subsequentEnemyCount;
            enemySpacing = spawnData.enemySpacing;
            enemySpeed = spawnData.enemySpeed;
            enemyDownDistance = spawnData.enemyDownDistance;
        }
    }

    /// <summary>
    /// 敵を定期的にスポーンさせるループ
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnEnemies();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// 敵をスポーンさせる処理
    /// </summary>
    private void SpawnEnemies()
    {
        if (isFirstSpawn)
        {
            InitializeSpawnRow(spawnPoint1.position, Vector3.right, EnemyMove.MoveDirection.Right);
            InitializeSpawnRow(spawnPoint2.position, Vector3.left, EnemyMove.MoveDirection.Left);
            isFirstSpawn = false;
        }
        else
        {
            SpawnRow(spawnPoint1.position, Vector3.right, EnemyMove.MoveDirection.Right);
        }
    }

    /// <summary>
    /// 1行分の敵をスポーンさせる処理
    /// </summary>
    /// <param name="basePosition"></param>
    /// <param name="direction"></param>
    /// <param name="moveDirection"></param>
    private void InitializeSpawnRow(Vector3 basePosition, Vector3 direction, EnemyMove.MoveDirection moveDirection)
    {
        // 敵を等間隔でスポーンさせる
        for (int i = 0; i < initialEnemyCount; i++)
        {
            Vector3 spawnPosition = basePosition + direction * enemySpacing * i; // 敵同士の間隔を考慮してスポーン位置を計算
            EnemyMove enemy = GetEnemy(spawnPosition); // 敵をスポーンさせる
            if (enemy == null)
            {
                continue;
            }

            enemy.SetMoveSettings(
                enemySpeed,
                enemyDownDistance,
                moveDirection
            );
        }
    }

    private void SpawnRow(Vector3 basePosition, Vector3 direction, EnemyMove.MoveDirection moveDirection)
    {
        // 敵を等間隔でスポーンさせる
        for (int i = 0; i < subsequentEnemyCount; i++)
        {
            Vector3 spawnPosition = basePosition + direction * enemySpacing * i; // 敵同士の間隔を考慮してスポーン位置を計算
            EnemyMove enemy = GetEnemy(spawnPosition); // 敵をスポーンさせる
            if (enemy == null)
            {
                continue;
            }

            enemy.SetMoveSettings(
                enemySpeed,
                enemyDownDistance,
                moveDirection
            );
        }
    }

    /// <summary>
    /// 敵をスポーンさせる処理
    /// </summary>
    /// <param name="spawnPosition"></param>
    /// <returns></returns>
    private EnemyMove GetEnemy(Vector3 spawnPosition)
    {
        EnemyMove enemy = null;

        // プールから敵を取得
        if (PoolManager.Instance != null && !string.IsNullOrEmpty(enemyPoolKey))
        {
            GameObject enemyObject = PoolManager.Instance.Get(enemyPoolKey); // プールから敵のオブジェクトを取得
            if (enemyObject == null) // プールから敵のオブジェクトを取得できなかった場合のエラーハンドリング
            {
                Debug.LogError($"{name}: Failed to get {enemyPoolKey} from pool.");
                return null;
            }

            // 取得したオブジェクトからEnemyMoveコンポーネントを取得
            if (!enemyObject.TryGetComponent(out enemy))
            {
                Debug.LogError($"{name}: {enemyPoolKey} does not have EnemyMove.");
                PoolManager.Instance.Return(enemyPoolKey, enemyObject);
                return null;
            }
        }
        else
        {
            Debug.LogError($"{name}: PoolManager or enemyPoolKey is not set.");
            return null;
        }

        enemy.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity); // 敵の位置と回転を設定
        return enemy;
    }
}
