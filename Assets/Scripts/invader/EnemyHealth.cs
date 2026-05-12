using UnityEngine;

public class EnemyHealth : MonoBehaviour, IPoolable
{
    public int maxHealth = 1; // 敵の最大体力
    [SerializeField]
    private int currentHealth;   // 敵の現在の体力
    private ObjectPool ownerPool; // 所属するオブジェクトプール
    private bool isDead = false; // 死亡フラグ

    private void Awake()
    {
        currentHealth = maxHealth; // 初期化
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void SetPool(ObjectPool pool)
    {
        ownerPool = pool;
    }

    /// <summary>
    /// 敵にダメージを与えるメソッド
    /// </summary>
    /// <param name="damage">与えるダメージ量</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 敵が死亡したときの処理
    /// </summary>
    private void Die()
    {
        // 死亡エフェクトやスコア加算などの処理をここに追加
        isDead = true; // 死亡フラグを立てる
        if (ownerPool != null)
        {
            OnDespawn(); // プールに返す前に位置や状態をリセット
            ownerPool.Return(gameObject); // プールに返す
        }
        else
        {
            Destroy(gameObject); // プールがない場合はオブジェクトを破壊
        }
    }

    /// <summary>
    /// IPoolableインターフェースの実装
    /// </summary>
    public void OnSpawn()
    {
        isDead = false; // 死亡フラグをリセット
        currentHealth = maxHealth; // スポーン時に体力をリセット
    }

    /// <summary>
    /// IPoolableインターフェースの実装
    /// </summary>
    public void OnDespawn()
    {
        transform.position = ownerPool.container.position; // プールの位置に戻す
        transform.rotation = Quaternion.identity; // 回転をリセット
    }
}
