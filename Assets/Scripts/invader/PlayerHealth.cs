using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("プレイヤーの最大体力")]
    public int maxHealth = 3; // プレイヤーの最大体力
    [Header("プレイヤーの現在の体力")]
    public int currentHealth;   // プレイヤーの現在の体力
    [Header("プレイヤーの無敵時間")]
    public int invincibleTime = 1; // 無敵時間（秒）
    [Header("点滅間隔")]
    [SerializeField] private float blinkInterval = 0.1f;
    [Header("ダメージを受けたときの音")]
    [SerializeField] private AudioClip damageSE; // ダメージを受けたときの音
    private bool isInvincible = false; // 無敵状態かどうか
    private ObjectPool ownerPool; // 所属するオブジェクトプール
    private SpriteRenderer spriteRenderer; // スプライトレンダラーの参照
    private bool isDead = false; // 死亡フラグ

    private void Awake()
    {
        currentHealth = maxHealth; // 初期化
        spriteRenderer = GetComponent<SpriteRenderer>(); // スプライトレンダラーの取得
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
        if (!isInvincible)
        {
            currentHealth -= damage;
            AudioManager.Instance.PlaySE(damageSE, 4f); // ダメージ音を再生
            StartCoroutine(InvincibleCoroutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 無敵状態のコルーチン
    /// </summary>
    /// <returns></returns>
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        float elapsedTime = 0f; // 経過時間の初期化

        while (elapsedTime < invincibleTime)
        {
            spriteRenderer.enabled = false; // スプライトを非表示にする
            yield return new WaitForSeconds(blinkInterval);

            spriteRenderer.enabled = true; // スプライトを表示する
            yield return new WaitForSeconds(blinkInterval);

            elapsedTime += blinkInterval * 2f;
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    /// <summary>
    /// 敵が死亡したときの処理
    /// </summary>
    private void Die()
    {
        // 死亡エフェクトやスコア加算などの処理をここに追加
        isDead = true; // 死亡フラグを立てる

        // ゲームオーバー処理
        GameManager.Instance.GameOver();
    }
}
