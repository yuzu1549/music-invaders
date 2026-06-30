using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("スポーンする弾の名前")]
    [SerializeField] private string bulletName = "Bullet";
    private GameObject bullet;

    void Start()
    {
        JudgmentManager.Instance.OnJudgment += SpawnBullet; // 判定結果を受ける
    }

    /// <summary>
    /// 弾をスポーンさせる処理
    /// </summary>
    private void SpawnBullet(string judgement)
    {
        if (judgement == "MISS")
        {
            // MISSの場合は弾をスポーンしない
            return;
        }
        if (bulletName != null)
        {
            // プールから弾を取得
            bullet = PoolManager.Instance.Get(bulletName);
            if (bullet != null)
            {
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.damage = GetDamageByJudgement(judgement);
                    bulletScript.Attack(); // 弾を発射して攻撃する
                }
                else
                {
                    Debug.LogError($"{name}: 取得したオブジェクトに Bullet コンポーネントが見つかりませんでした。");
                }
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

    /// <summary>
    /// 判定結果に応じたダメージ量を返すメソッド
    /// </summary>
    /// <param name="judgement"></param>
    /// <returns>ダメージ量</returns>
    private int GetDamageByJudgement(string judgement)
    {
        switch (judgement)
        {
            case "PERFECT":
                return 2;
            case "GOOD":
                return 1;
        }
        return 0;
    }
}
