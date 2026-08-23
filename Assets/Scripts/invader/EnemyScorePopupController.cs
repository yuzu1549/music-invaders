using UnityEngine;

public class EnemyScorePopupController : MonoBehaviour
{
    [Header("撃破得点の表示に使用するポップアップ")]
    [SerializeField] private EnemyScorePopup scorePopupPrefab;

    [Header("生成したポップアップを格納する親（未設定でも可）")]
    [SerializeField] private Transform popupParent;

    [Header("敵の位置からずらすワールド座標")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.5f, 0f);

    private bool hasWarnedMissingPrefab;

    /// <summary>
    /// 指定した敵の位置に獲得スコアを表示する。
    /// </summary>
    /// <param name="score">表示する獲得スコア</param>
    /// <param name="enemyPosition">撃破された敵のワールド座標</param>
    public void ShowScore(int score, Vector3 enemyPosition)
    {
        if (score <= 0)
        {
            return;
        }

        if (scorePopupPrefab == null)
        {
            if (!hasWarnedMissingPrefab)
            {
                Debug.LogWarning(
                    $"{name}: 撃破得点ポップアップが設定されていません。"
                );
                hasWarnedMissingPrefab = true;
            }

            return;
        }

        EnemyScorePopup scorePopup = Instantiate(
            scorePopupPrefab,
            enemyPosition + worldOffset,
            scorePopupPrefab.transform.rotation,
            popupParent
        );
        scorePopup.Show(score);
    }
}
