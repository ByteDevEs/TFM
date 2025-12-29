using System;
using Enemies;
using Enemies.EnemyStates;
using Mirror;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Controllers
{
	public class HealthController : NetworkBehaviour
	{
		[SyncVar] public int MaxHealth = 100;
		public float TimeToRegenPotion;
		public int MaxPotionCount = 3;
		public int PotionHeal = 50;
		[SyncVar(hook = nameof(OnHealthChanged))] [HideInInspector] public float CurrentHealth;
		[HideInInspector] public float HealthPercentage = 1;
		[SyncVar] [HideInInspector] public float PotionCooldownPercentage;
		[SyncVar] [HideInInspector] public int PotionCount;
		public Action<GameObject> OnDeath;
		
		// public System.Action<float> OnDamaged;
		PlayerController playerController;
		EnemyController enemyController;
		int initialMaxHealth;
		float potionCooldown;
		GameObject lastAttacker;
		bool started;

		void Start()
		{
			initialMaxHealth = MaxHealth;
			if (isServer)
			{
				CurrentHealth = MaxHealth;
				PotionCount = MaxPotionCount;
			
				OnDeath += Die;
			
				playerController = GetComponent<PlayerController>();
				enemyController = GetComponent<EnemyController>();
			}
			
			started = true;
		}

		void Update()
		{
			HealthPercentage = CurrentHealth / MaxHealth;
			
			if (!isServer)
			{
				return;
			}

			MaxHealth = playerController is not null
				? initialMaxHealth + (playerController.AttackController.Stats.Health - 1) * 10
				: initialMaxHealth;
			
			PotionCooldownPercentage = potionCooldown / TimeToRegenPotion;

			if (PotionCount == MaxPotionCount)
			{
				return;
			}
			
			potionCooldown -= Time.deltaTime;

			if (potionCooldown > 0)
			{
				return;
			}

			PotionCount += 1;
			
			if (PotionCount != MaxPotionCount)
			{
				potionCooldown = TimeToRegenPotion;
			}
		}

		void OnHealthChanged(float oldValue, float newValue)
		{
			if (!started)
			{
				return;
			}
			
			float damage = oldValue - newValue;
			
			if (damage != 0)
			{
				DamageTextSpawner spawner = FindAnyObjectByType<DamageTextSpawner>();
				spawner.SpawnDamageText(damage, transform.position + Vector3.up * Random.Range(1f, 1.5f));
			}

			// OnDamaged?.Invoke(NewValue);
			if (newValue <= 0)
			{
				OnDeath?.Invoke(lastAttacker);
			}
		}

		public void TakeDamage(GameObject attacker, int amount)
		{
			if (isServer)
			{
				SrvTakeDamage(attacker, amount);
			}
			else
			{
				CmdTakeDamage(attacker, amount);
			}
		}

		[Command]
		public void CmdTakeDamage(GameObject attacker, float amount) => SrvTakeDamage(attacker, amount);

		[Server]
		public void SrvTakeDamage(GameObject attacker, float amount)
		{
			if (CurrentHealth <= 0)
			{
				return;
			}
			lastAttacker = attacker;
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

		[Command]
		public void Die()
		{
			SrvTakeDamage(null, 999999);
		}
		
		[Server]
		void Die(GameObject attacker)
		{
			if (attacker)
			{
				attacker.GetComponent<AttackController>().Stats.AddXp();
			}
			CurrentHealth = 0;
			if (playerController)
			{
				playerController.IsDead = true;
			}
			else if (enemyController)
			{
				NetworkServer.Destroy(gameObject);
			}
		}
		
		public void TakePotion()
		{
			if(isServer)
			{
				SrvTakePotion();
			}
			else 
			{
				CmdTakePotion();
			}
		}

		[Command]
		void CmdTakePotion() => SrvTakePotion();

		[Server]
		void SrvTakePotion()
		{
			if (PotionCount <= 0 || Mathf.Approximately(CurrentHealth, MaxHealth))
			{
				return;
			}
			
			if (PotionCount == MaxPotionCount)
			{
				potionCooldown = TimeToRegenPotion;
			}
			
			PotionCount--;
			RpcPlaySound("Heal");
			CurrentHealth = Math.Min(CurrentHealth + PotionHeal, MaxHealth);
			if (CurrentHealth > MaxHealth)
			{
				CurrentHealth = MaxHealth;
			}
		}
		
		[ClientRpc]
		void RpcPlaySound(string soundName)
		{
			Prefabs.GetInstance().PlaySound(soundName, transform);
		}
		
		[Server]
		public void SrvRevive()
		{
			CurrentHealth = Math.Min(CurrentHealth + PotionHeal, MaxHealth);
			if (CurrentHealth > MaxHealth)
			{
				CurrentHealth = MaxHealth;
			}
		}
	}
}
