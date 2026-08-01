using UnityEngine;

public class EnemyReachLine : MonoBehaviour
{
	[Header("プレイヤーのゲームオブジェクト")]
	[SerializeField] private GameObject player;

	private PlayerHealth playerHealth;

	private void Start()
	{
		if (player == null)
		{
			Debug.LogError(
				"EnemyReachLine: プレイヤーが設定されていません。");
			return;
		}

		playerHealth = player.GetComponent<PlayerHealth>();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!collision.CompareTag("Enemy"))
		{
			return;
		}

		EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
		if (enemyHealth == null)
		{
			return;
		}

		enemyHealth.TakeDamage(enemyHealth.maxHealth);
		playerHealth?.TakeDamage(1);
	}
}
