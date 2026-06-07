using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("スポーンする弾の名前")]
    [SerializeField] private string bulletName = "Bullet";

    [Header("スポーンする間隔")]
    [SerializeField] private float spawnInterval = 2f;

    private float timer = 0f;
    private GameObject bullet;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnBullet();
            timer = 0f;
        }
    }

    /// <summary>
    /// 弾をスポーンさせる処理
    /// </summary>
    private void SpawnBullet()
    {
        if (bulletName != null)
        {
            // プールから弾を取得
            bullet = PoolManager.Instance.Get(bulletName);
            if (bullet != null)
            {
                bullet.GetComponent<Bullet>().Attack(); // 弾を発射して攻撃する
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
