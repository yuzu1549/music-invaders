using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    private ObjectPool ownerPool; // このオブジェクトが入ったプール
    [Header("弾の速度")]
    public float speed = 10f; // 弾の速度
    private Vector2 direction = Vector2.up; // 弾の進行方向
    private float Distance = 0f; // 進んだ距離
    [Header("弾が消える距離")]
    public float maxDistance = 20f; // 弾が消える距離
    private Vector2 startPosition; // スタート位置
    private bool isAttacking = false; // 攻撃中かどうか
    private Transform playerTransform; // プレイヤーのTransform
    
    [Header("弾のダメージ量")]
    public int damage = 1; // ダメージ量
    [Header("弾の貫通力")]
    public int penetration = 1; // 貫通力

    private void Awake()
    {
        // "Player"タグを持つオブジェクトを取得
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

	/// <summary>
    /// 自分がどのプールに所属しているか覚えておくためのもの
    /// </summary>
    /// <param name="pool"></param>
    public void SetPool(ObjectPool pool)
    {
        ownerPool = pool;
    }

    /// <summary>
    /// オブジェクトの初期化
    /// </summary>
    public void OnSpawn()
    {
        // 位置をプレイヤーの位置に設定
        transform.position = playerTransform.position;
        // 回転をリセット
        transform.rotation = Quaternion.identity;
        // 状態をリセット
        Distance = 0f;
        startPosition = transform.position;
        isAttacking = false;
    }

    /// <summary>
    /// オブジェクトの後処理
    /// </summary>
    public void OnDespawn()
    {
        Distance = 0f;
        isAttacking = false;
        transform.rotation = Quaternion.identity; // 回転をリセット
        transform.position = ownerPool.container.position; // プールの位置に戻す
    }

        /// <summary>
    /// プールがあればオブジェクトを返す
    /// </summary>
    private void ReturnToPool()
    {
        if (ownerPool != null)
        {
            ownerPool.Return(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 弾を発射して攻撃する
    /// </summary>
    public void Attack()
    {
        isAttacking = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttacking) return;

        // プレイヤー以外のオブジェクトにダメージを与える
        if (! other.CompareTag("Player") && other.gameObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
            penetration--; // 貫通力を減らす
            if (penetration <= 0)
            {
                ReturnToPool(); // 貫通力がなくなったら弾を消す
            }
            
        }
    }

    private void Update()
    {
        if (isAttacking)
        {
            // 弾を進行方向に移動させる
            transform.Translate(direction * speed * Time.deltaTime);

            // スタート位置からの距離を計算
            Distance = Vector2.Distance(startPosition, transform.position);

            // 攻撃時間が経過したか、プレイヤーから一定距離以上離れたら弾を消す
            if (Distance >= maxDistance)
            {
                ReturnToPool();
            }
        }
    }
}
