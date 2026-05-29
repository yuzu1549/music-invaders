using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("敵")]
    [SerializeField] private EnemyMove enemyPrefab; // スポーンする敵のプレハブ

    [Space(15)] // 見出しを隔てるスペース
    [Header("スポーン設定")]
    [Header("スポーンする間隔")]
    [SerializeField] private float spawnInterval = 2f; // 敵をスポーンさせる間隔
    [Header("最初にスポーンさせる敵の数")]
    [SerializeField] private int initialEnemyCount = 5; // 最初にスポーンさせる敵の数
    [Header("敵敵同士の間隔")]
    [SerializeField] private float enemySpacing = 1f; // 敵同士の間隔
    private Transform spawnPoint1; // スポーンする位置
    private Transform spawnPoint2; // スポーンする位置

    [Space(15)] // 見出しを隔てるスペース
    [Header("敵の移動設定")]
    [Header("敵の移動速度")]
    [SerializeField] private float enemySpeed = 3f;
    [Header("敵が壁に当たったときの垂直移動の距離")]
    [SerializeField] private float enemyDownDistance = 1f;

    private bool isFirstSpawn = true; // 最初のスポーンかどうかを判定するフラグ

    private void Awake()
    {
        // 最初のスポーン位置をランダムに選択
        spawnPoint1 = this.transform.Find("SpawnPoint1");
        spawnPoint2 = this.transform.Find("SpawnPoint2");
    }

    private void Start() 
    {
        StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// 敵をスポーンさせるループ処理
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnEnemies();

            yield return new WaitForSeconds(spawnInterval); // 次のスポーンまで待機
        }
    }

    /// <summary>
    /// 敵をスポーンさせる処理
    /// </summary>
    private void SpawnEnemies()
    {
        if (isFirstSpawn)
        {
            SpawnRow(spawnPoint1.position, Vector3.right, EnemyMove.MoveDirection.Right); // 1行目をスポーン
            SpawnRow(spawnPoint2.position, Vector3.left, EnemyMove.MoveDirection.Left); // 2行目をスポーン
            isFirstSpawn = false;
        }
        else
        {
            SpawnRow(spawnPoint1.position, Vector3.right, EnemyMove.MoveDirection.Right); // 1行目をスポーン
        }
    }

    /// <summary>
    /// 敵をスポーンさせる行を生成する処理
    /// </summary> 
    /// <param name="basePosition">スポーンの基準位置</param>
    /// <param name="direction">敵をスポーンさせる方向</param>
    /// <param name="moveDirection">敵の移動方向</param>
    private void SpawnRow(Vector3 basePosition, Vector3 direction, EnemyMove.MoveDirection moveDirection)
    {
        // 敵をスポーンさせる位置を計算
        for (int i = 0; i < initialEnemyCount; i++)
        {
            Vector3 spawnPosition = basePosition + direction * enemySpacing * i; // 敵同士の間隔を考慮してスポーン位置を計算

            // 敵をスポーンさせる
            EnemyMove enemy = Instantiate(
                enemyPrefab,
                spawnPosition,
                Quaternion.identity
            );

            enemy.SetMoveSettings(
                enemySpeed,
                enemyDownDistance,
                moveDirection
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
