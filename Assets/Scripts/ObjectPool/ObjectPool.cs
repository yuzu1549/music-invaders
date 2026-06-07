using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [Header("生成するプレハブ")]
    [SerializeField] private GameObject prefab;

    [Header("初期生成する数")]
    [SerializeField] private int initialSize = 10;

    [Header("最大生成する数")]
    [SerializeField] private int maxSize = 10;

    [Header("プールしたオブジェクトの位置")]
    [Tooltip("基本的にこのスクリプトがついているオブジェクトのTransformを指定してください")]
    public Transform container;

    // 未使用オブジェクト
    private readonly Queue<GameObject> pool = new();

    // 使用中オブジェクト（古い順）
    private readonly LinkedList<GameObject> activeList = new();
    private readonly Dictionary<GameObject, LinkedListNode<GameObject>> activeMap = new();

    // 総生成数
    private int totalCreated = 0;

    private void Awake()
    {
        if (container == null)
        {
            container = transform;
        }

        if (prefab == null)
        {
            Debug.LogError($"{name}: prefab が設定されていません。");
            enabled = false;
            return;
        }

        if (maxSize <= 0)
        {
            Debug.LogError($"{name}: maxSize は 1 以上にしてください。");
            enabled = false;
            return;
        }

        // 初期生成
        int spawnCount = Mathf.Clamp(initialSize, 0, maxSize);
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject obj = CreateNewInstance();
            pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// 新しいインスタンスを生成してプールに追加する
    /// </summary> <returns>生成したオブジェクト</returns>
    private GameObject CreateNewInstance()
    {
        GameObject obj = Instantiate(prefab, container);
        obj.SetActive(false);

        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.SetPool(this);

        totalCreated++;
        return obj;
    }

    /// <summary>
    /// オブジェクトを取得する
    /// </summary> <returns>取得したオブジェクト</returns>
    public GameObject Get()
    {
        GameObject obj = null;

        // 1. 未使用があれば使う
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        // 2. まだ生成可能なら新規生成
        else if (totalCreated < maxSize)
        {
            obj = CreateNewInstance();
        }
        // 3. 上限到達なら最古の使用中を再利用
        else if (activeList.First != null)
        {
            obj = activeList.First.Value; // 最古の使用中オブジェクト
            RemoveFromActive(obj);

            IPoolable poolable = obj.GetComponent<IPoolable>();
            poolable?.OnDespawn();

            obj.SetActive(false);
            obj.transform.SetParent(container, false); // プールの位置に戻す
        }
        else
        {
            Debug.LogWarning($"{name}: 取得できるオブジェクトがありません。");
            return null;
        }

        obj.SetActive(true);

        IPoolable spawnable = obj.GetComponent<IPoolable>();
        spawnable?.OnSpawn();

        AddToActive(obj); // 使用中リストに追加
        return obj;
    }

    /// <summary>
    /// オブジェクトを返す
    /// </summary>
    /// <param name="obj"></param>
    public void Return(GameObject obj)
    {
        if (obj == null) return;

        // すでに未使用なら二重返却なので無視
        if (!activeMap.ContainsKey(obj))
        {
            return;
        }

        RemoveFromActive(obj);

        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.OnDespawn();

        obj.SetActive(false);
        obj.transform.SetParent(container, false);

        pool.Enqueue(obj);
    }

    /// <summary>
    /// 使用中リストに追加する
    /// </summary>
    /// <param name="obj"></param>
    private void AddToActive(GameObject obj)
    {
        if (activeMap.ContainsKey(obj)) return;

        LinkedListNode<GameObject> node = activeList.AddLast(obj);
        activeMap[obj] = node;
    }

    /// <summary> 
    /// 使用中リストから削除する
    /// </summary> <param name="obj"></param>
    private void RemoveFromActive(GameObject obj)
    {
        if (activeMap.TryGetValue(obj, out var node))
        {
            activeList.Remove(node);
            activeMap.Remove(obj);
        }
    }
}