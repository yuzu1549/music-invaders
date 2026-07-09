using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnDatabase", menuName = "Data/Enemy Spawn Database")]
public class EnemySpawnDatabase : ScriptableObject
{
    public EnemySpawnData[] spawnDatas;

    // 名前で敵スポーンデータを取得するメソッド
    public EnemySpawnData GetByName(string name)
    {
        foreach (var spawnData in spawnDatas)
        {
            if (spawnData.name == name)
                return spawnData;
        }
        Debug.LogWarning($"指定された名前の敵スポーンデータが見つかりませんでした: {name}");
        return spawnDatas[0]; // デフォルトで最初のデータを返す
    }
}