using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyPlayerCollision : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    private bool hasCollidedWithPlayer;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        hasCollidedWithPlayer = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerCollision(other.gameObject);
    }

    /// <summary>
    /// 突入中に自機と接触した敵を消し、自機へダメージを与える。
    /// </summary>
    /// <param name="collisionObject">接触したオブジェクト</param>
    private void HandlePlayerCollision(GameObject collisionObject)
    {
        if (hasCollidedWithPlayer || collisionObject == null)
        {
            return;
        }

        PlayerHealth playerHealth =
            collisionObject.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        EnemyGroupMovement groupMovement =
            GetComponentInParent<EnemyGroupMovement>();
        if (groupMovement == null || !groupMovement.IsInDivePhase)
        {
            return;
        }

        hasCollidedWithPlayer = true;
        playerHealth.TakeDamage(1);
        enemyHealth.DespawnWithoutDefeat();
    }
}
