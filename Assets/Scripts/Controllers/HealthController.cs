using Enemies;
using Enemies.EnemyStates;
using Mirror;
using UI;
using UnityEngine;

namespace Controllers
{
	public class HealthController : NetworkBehaviour
	{
		[SyncVar] public int MaxHealth = 100;
		[SyncVar(hook = nameof(OnHealthChanged))] public float CurrentHealth;

		public float HealthPercentage = 1;
		public System.Action OnDeath;
		// public System.Action<float> OnDamaged;
		EnemyController enemyController;

		void Start()
		{
			if (!isServer)
			{
				return;
			}
			
			CurrentHealth = MaxHealth;
				
			OnDeath += Die;
			
			enemyController = GetComponent<EnemyController>();
		}
		void OnHealthChanged(float oldValue, float newValue)
		{
			float damage = oldValue - newValue;
			HealthPercentage = newValue / MaxHealth;
			if (damage > 0)
			{
				DamageTextSpawner spawner = FindAnyObjectByType<DamageTextSpawner>();
				spawner.SpawnDamageText(damage, transform.position + Vector3.up * Random.Range(0.5f, 1f));
			}

			// OnDamaged?.Invoke(NewValue);

			if (newValue <= 0)
			{
				OnDeath?.Invoke();
			}
		}

		[Server]
		public void TakeDamage(GameObject attacker, float amount)
		{
			if (CurrentHealth <= 0)
			{
				return;
			}
			CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
			if (enemyController)
			{
				if (enemyController.State.GetType() == typeof(AttackState))
				{
					return;
				}
				
				enemyController.State = new AttackState(enemyController, attacker);
			}
		}
		
		[Server]
		void Die()
		{
			NetworkServer.Destroy(gameObject);
		}
	}
}
