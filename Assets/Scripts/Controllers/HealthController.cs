using Enemies;
using Enemies.EnemyStates;
using Mirror;
using UI;
using UnityEngine;

namespace Controllers
{
	public class HealthController : NetworkBehaviour
	{
		[SyncVar] public int maxHealth = 100;
		[SyncVar(hook = nameof(OnHealthChanged))] public float currentHealth;

		public float healthPercentage = 1;
		public System.Action OnDeath;
		public System.Action<float> OnDamaged;
		EnemyController enemyController;

		void Start()
		{
			if (!isServer)
			{
				return;
			}
			
			currentHealth = maxHealth;
				
			OnDeath += Die;
			
			enemyController = GetComponent<EnemyController>();
		}
		void OnHealthChanged(float oldValue, float newValue)
		{
			float damage = oldValue - newValue;
			healthPercentage = newValue / maxHealth;
			if (damage > 0)
			{
				DamageTextSpawner spawner = FindAnyObjectByType<DamageTextSpawner>();
				spawner.SpawnDamageText(damage, transform.position + Vector3.up * Random.Range(0.5f, 1f));
			}

			OnDamaged?.Invoke(newValue);

			if (newValue <= 0)
			{
				OnDeath?.Invoke();
			}
		}

		[Server]
		public void TakeDamage(GameObject attacker, float amount)
		{
			if (currentHealth <= 0)
			{
				return;
			}
			currentHealth = Mathf.Max(0, currentHealth - amount);
			if (enemyController)
			{
				if (enemyController.state.GetType() == typeof(AttackState))
				{
					return;
				}
				
				enemyController.state = new AttackState(enemyController, attacker);
			}
		}
		
		[Server]
		void Die()
		{
			NetworkServer.Destroy(gameObject);
		}
	}
}
