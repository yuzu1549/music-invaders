using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("プレイヤーの最大体力")]
    public int maxHealth = 3; // プレイヤーの最大体力
    [Header("プレイヤーの現在の体力")]
    public int currentHealth;   // プレイヤーの現在の体力
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

    /// <summary>
    /// ダメージを受けるメソッド
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
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

        // ゲームオーバー処理

    }
}
