using Mirror;
using UI;
using UnityEngine;

namespace Controllers
{
	public class HealthController : NetworkBehaviour
	{
		[SyncVar] public int maxHealth = 100;
		[SyncVar(hook = nameof(OnHealthChanged))] public int currentHealth;

		public System.Action OnDeath;
		public System.Action<int> OnDamaged;

		void Start()
		{
			if (isServer)
			{
				currentHealth = maxHealth;
				
				OnDeath += Die;
			}
		}
		void OnHealthChanged(int oldValue, int newValue)
		{
			int damage = oldValue - newValue;
			if (damage > 0)
			{
				DamageTextSpawner spawner = FindAnyObjectByType<DamageTextSpawner>();
				spawner.SpawnDamageText(damage, transform.position + Vector3.up * Random.Range(0.5f, 1f), transform);
			}

			OnDamaged?.Invoke(newValue);

			if (newValue <= 0)
			{
				OnDeath?.Invoke();
			}
		}

		[Server]
		public void TakeDamage(int amount)
		{
			if (currentHealth <= 0)
			{
				return;
			}
			currentHealth = Mathf.Max(0, currentHealth - amount);
		}
		
		[Server]
		void Die()
		{
			NetworkServer.Destroy(gameObject);
		}
	}
}
