using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
	// シングルトン
    public static PoolManager Instance { get; private set; }

    [System.Serializable]
    public class PoolEntry
    {
        public string key; // オブジェクトの名前
        public ObjectPool pool; // 対応するオブジェクトプール
    }

    [Header("オブジェクトの名前と対応するオブジェクトプール")]
    [SerializeField] private List<PoolEntry> poolEntries = new();

    private Dictionary<string, ObjectPool> poolDictionary; // オブジェクトとプールを対応づけた辞書

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

		// インスペクターに設定した一覧を辞書に変換
        poolDictionary = new Dictionary<string, ObjectPool>();
        foreach (PoolEntry entry in poolEntries)
        {
            if (!poolDictionary.ContainsKey(entry.key))
            {
                poolDictionary.Add(entry.key, entry.pool);
            }
        }
    }

    /// <summary>
    /// 取り出し窓口
    /// </summary>
    /// <param name="key">オブジェクト名</param>
    /// <returns>借りたオブジェクト</returns>
    public GameObject Get(string key)
    {
        if (poolDictionary.TryGetValue(key, out ObjectPool pool))
        {
            return pool.Get();
        }

        Debug.LogWarning($"Pool not found: {key}");
        return null;
    }

	/// <summary>
    /// 返却窓口
    /// </summary>
    /// <param name="key">オブジェクト名</param>
    /// <param name="obj">返却するオブジェクト</param>
    public void Return(string key, GameObject obj)
    {
        if (poolDictionary.TryGetValue(key, out ObjectPool pool))
        {
            pool.Return(obj);
            return;
        }

        Debug.LogWarning($"Pool not found: {key}");
        obj.SetActive(false);
    }
}
