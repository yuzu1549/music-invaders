using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnData", menuName = "Data/Enemy Spawn")]
public class EnemySpawnData : ScriptableObject
{
    [Header("敵のプールのキー")]
    public string enemyPoolKey = "Invader";

    [Space(15)]
    [Header("スポーン設定")]
    [Header("スポーンする間隔")]
    public float spawnInterval = 3f;
    [Header("最初のスポーンで生成する敵の数")]
    public int initialEnemyCount = 3;
    [Header("2回目以降で生成する敵の数")]
    public int subsequentEnemyCount = 3;
    [Header("敵同士の間隔")]
    public float enemySpacing = 0.75f;

    [Space(15)]
    [Header("敵の移動設定")]
    [Header("敵の移動速度")]
    public float enemySpeed = 1.25f;
    [Header("敵が壁に当たったときの垂直移動距離")]
    public float enemyDownDistance = 0.75f;
}
