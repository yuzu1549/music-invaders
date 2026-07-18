using UnityEngine;

public class EnemyDeathLine : MonoBehaviour
{
    [Header("プレイヤーのゲームオブジェクト")]
    public GameObject player; // プレイヤーのゲームオブジェクトを参照するための変数
    private PlayerHealth playerHealth; // プレイヤーの体力を参照するための変数

    private void Start()
    {
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>(); // プレイヤーの体力を取得
        }
        else
        {
            Debug.LogError("EnemyDeathLine: プレイヤーのゲームオブジェクトが設定されていません。");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth?.TakeDamage(enemyHealth.maxHealth); // 敵の体力を0にする
            }

            playerHealth?.TakeDamage(1); // プレイヤーの体力を1減らす
        }

        if (collision.CompareTag("Bullet"))
        {
            EnemyBullet enemyBullet = collision.GetComponent<EnemyBullet>();
            if (enemyBullet != null)
            {
                enemyBullet.penetration--; // 貫通力を減らす
                if (enemyBullet.penetration <= 0)
                {
                    enemyBullet.ReturnToPool(); // 貫通力がなくなったら弾を消す
                }
            }
        }
    }

}
