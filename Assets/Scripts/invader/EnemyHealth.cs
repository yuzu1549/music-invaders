using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IPoolable, IDamageable
{
    public int maxHealth = 1; // 敵の最大体力
    [SerializeField]
    private int currentHealth;   // 敵の現在の体力
    private ObjectPool ownerPool; // 所属するオブジェクトプール
    private bool isDead = false; // 死亡フラグ
    private Animator anim; // アニメーターの参照
    private Rigidbody2D rb; // Rigidbody2Dの参照
    private EnemyMove enemyMove; // EnemyMoveの参照
    
    [Header("死亡音")]
    [SerializeField] private AudioClip deathSE; // 死亡音
    [Header("死亡エフェクトの再生秒数")]
    [SerializeField] private float playSeconds = 1.0f;

    private void Awake()
    {
        currentHealth = maxHealth; // 初期化
        anim = GetComponent<Animator>(); // アニメーターの取得
        enemyMove = GetComponent<EnemyMove>(); // EnemyMoveの取得
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2Dの取得
    }

    void Start()
    {

        anim.SetInteger("HP", currentHealth); // アニメーターに体力を渡す
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
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
        anim.SetInteger("HP", currentHealth); // アニメーターに体力を渡す
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    /// <summary>
    /// 敵が死亡したときの処理
    /// </summary>
    private IEnumerator Die()
    {
        // 死亡エフェクトやスコア加算などの処理をここに追加
        isDead = true; // 死亡フラグを立てる

        if (deathSE != null)
        {
            AudioManager.Instance.PlaySE(deathSE); // 死亡音を再生
        }

        enemyMove.enabled = false; // 敵の移動を停止
        rb.linearVelocity = Vector2.zero; // Rigidbodyの速度をリセット

        yield return new WaitForSeconds(playSeconds);

        if (ownerPool != null)
        {
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
        anim.SetInteger("HP", currentHealth); // アニメーターに体力を渡す
        enemyMove.enabled = true; // 敵の移動を再開
        rb.linearVelocity = Vector2.zero; // Rigidbodyの速度をリセット
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
