using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("スポーンする弾の名前")]
    [SerializeField] private string bulletName = "EnemyBullet";

    private GameObject bullet;

    private void Update()
    {
    }

    /// <summary>
    /// 弾をスポーンさせる処理
    /// </summary>
    public void Attack()
    {
        if (bulletName != null)
        {
            // プールから弾を取得
            bullet = PoolManager.Instance.Get(bulletName);
            if (bullet != null)
            {
                bullet.transform.position = transform.position; // 弾の位置を敵の位置に設定
                bullet.GetComponent<EnemyBullet>().Attack(); // 弾を発射して攻撃する
            }
            else
            {
                Debug.LogError($"{name}: プールから {bulletName} を取得できませんでした。");
            }
        }
        else
        {
            Debug.LogError($"{name}: bulletName が設定されていません。");
        }
    }

}
