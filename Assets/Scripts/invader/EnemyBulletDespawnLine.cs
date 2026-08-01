using UnityEngine;

public class EnemyBulletDespawnLine : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		EnemyBullet enemyBullet = collision.GetComponent<EnemyBullet>();
		if (enemyBullet == null)
		{
			return;
		}

		enemyBullet.ReturnToPool();
	}
}
