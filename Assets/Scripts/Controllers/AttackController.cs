using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemies;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapons;
// ReSharper disable Unity.PreferNonAllocApi

namespace Controllers
{
	[RequireComponent(typeof(MovementController), typeof(CharacterStats))]
	public class AttackController : NetworkBehaviour
	{
		PlayerController playerController;
		MovementController movementController;
		HealthController healthController;
		Coroutine attackCoroutine;
		EnemyController lastEnemyHit;
		PhysicalWeapon lastWeaponHit;
		[SyncVar] GameObject selectedTarget;
		[SyncVar] public bool IsAttackingTarget;
		[SyncVar] float weaponCooldown;

		WeaponScriptable weapon;
		public CharacterStats Stats { get; private set; }
		
		void Start()
		{
			playerController = GetComponent<PlayerController>();
			movementController = GetComponent<MovementController>();
			healthController = GetComponent<HealthController>();
			if (healthController)
			{
				healthController.OnDeath += OnDeath;
			}
			Stats = GetComponent<CharacterStats>();
		}

		void Update()
		{
			weaponCooldown += Time.deltaTime;
		}

		public bool TryAttack(Ray ray)
		{
			if (!Physics.Raycast(ray, out RaycastHit hit)) return false;

			EnemyController enemy = hit.collider.GetComponent<EnemyController>();

			if (enemy)
			{
				if (lastEnemyHit != enemy)
				{
					lastEnemyHit?.RemoveEffect();
					lastEnemyHit = enemy;
					lastEnemyHit.SetHoverEffect();
				}

				if (Mouse.current.leftButton.wasPressedThisFrame)
				{
					SelectTarget(enemy.gameObject);
				}

				return true;
			}

			if (isLocalPlayer && Mouse.current.leftButton.wasPressedThisFrame)
			{
				CmdStopAttacking();
			}

			lastEnemyHit?.RemoveEffect();
			lastEnemyHit = null;
			return false;
		}

		// This is when I click
		void SelectTarget(GameObject enemy)
		{
			if (selectedTarget && selectedTarget != enemy)
			{
				lastEnemyHit.RemoveEffect();
			}

			if (!isLocalPlayer)
			{
				return;
			}

			lastEnemyHit.SetClickEffect();
			
			CmdAttack(enemy);
		}
		
		[Command]
		void CmdAttack(GameObject enemy)
		{
			selectedTarget = enemy;
			IsAttackingTarget = true;

			if (attackCoroutine != null)
			{
				StopCoroutine(attackCoroutine);
				attackCoroutine = null;
			}

			movementController.SrvStop();
			
			attackCoroutine = StartCoroutine(Attack(selectedTarget));
		}

		[Command]
		void CmdStopAttacking()
		{
			if (attackCoroutine != null)
			{
				StopCoroutine(attackCoroutine);
				attackCoroutine = null;
			}

			movementController.SrvStop();
			IsAttackingTarget = false;
		}
		
		[Server]
		public void SrvEnemyAttack(GameObject target)
		{
			selectedTarget = target;
			IsAttackingTarget = true;

			if (attackCoroutine != null)
			{
				return;
			}

			movementController.SrvStop();
			attackCoroutine = StartCoroutine(Attack(selectedTarget));
		}

		[Server]
		public void SrvStopAttacking()
		{
			if (attackCoroutine != null)
			{
				StopCoroutine(attackCoroutine);
				attackCoroutine = null;
			}

			movementController.SrvStop();
			IsAttackingTarget = false;
		}
		
		IEnumerator Attack(GameObject target)
		{
			while (IsAttackingTarget && target)
			{
				if (!target)
				{
					StopCoroutine(attackCoroutine);
					attackCoroutine = null;
				}
				
				float distance = Vector3.Distance(transform.position, target.transform.position);
				bool inRange = distance <= weapon.BaseRange;
				
				if (!inRange)
				{
					Vector3 stopPos = GetPositionAtMaxAttackRange(target.transform, weapon.BaseRange * 0.75f);
					movementController.SrvMove(stopPos);
				}
				else
				{
					movementController.SrvStop();

					if (weaponCooldown >= weapon.BaseCooldown)
					{
						weaponCooldown = 0;
						PerformAttack(target);
					}
				}

				yield return null;
			}
		}

		void PerformAttack(GameObject target)
		{
			if (!isServer) return;

			float damage = weapon.BaseDamage + Stats.Strength;

			switch (weapon.AttackType)
			{
				case AttackType.Melee:
					StartCoroutine(AttackMelee(target.transform, damage));
					break;

				case AttackType.Ranged:
					StartCoroutine(AttackRanged(target.transform, damage));
					break;

				case AttackType.Area:
					StartCoroutine(AttackArea(target.transform, damage));
					break;
			}
		}

		IEnumerator AttackMelee(Transform target, float damage)
		{
			Vector3 position = (target.position + transform.position) / 2f;
			IEnumerable<Collider> hits = Physics.OverlapSphere(position, weapon.BaseRange / 2.0f).Except(GetComponents<Collider>());

			Attack(hits, damage);
			yield return null;
		}

		IEnumerator AttackRanged(Transform target, float damage)
		{
			Vector3 startPos = transform.position;
			Vector3 targetPos = target.position;
    
			float distance = Vector3.Distance(startPos, targetPos);
			const float resolutionPerMetre = 5f;
    
			int steps = Mathf.CeilToInt(resolutionPerMetre * distance); 

			for (int i = 0; i <= steps; i++)
			{
				float t = (float)i / steps;
        
				Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

				List<Collider> hits = Physics.OverlapSphere(currentPos, weapon.AreaDiameter / 2.0f)
					.Except(GetComponents<Collider>())
					.ToList();

				if (hits.Count > 0)
				{
					Attack(hits, damage);
					break;
				}

				yield return new WaitForSeconds(1.0f / steps);
			}
    
			yield return null;
		}

		IEnumerator AttackArea(Transform target, float damage)
		{
			yield return new WaitForSeconds(weapon.CastTime);
			
			IEnumerable<Collider> hits = Physics.OverlapSphere(target.position, weapon.AreaDiameter / 2.0f).Except(GetComponents<Collider>());
			
			Attack(hits, damage);
			yield return null;
		}

		void Attack(IEnumerable<Collider> hits, float damage)
		{
			foreach (Collider c in hits)
			{
				if (c.GetComponent<HealthController>() is {} hC)
				{
					hC.TakeDamage(gameObject, damage);
				}
			}
		}

		Vector3 GetPositionAtMaxAttackRange(Transform target, float range)
		{
			Vector3 dir = target.position - transform.position;
			dir.y = 0;

			if (dir.sqrMagnitude < 0.01f)
			{
				return transform.position;
			}

			dir.Normalize();
			return target.position - dir * range;
		}
		
		public void SwapWeapons(Ray ray)
		{
			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				PhysicalWeapon physicalWeapon = hit.collider.GetComponent<PhysicalWeapon>();

				if (physicalWeapon)
				{
					if (lastWeaponHit != physicalWeapon)
					{
						lastWeaponHit?.RemoveEffect();
						lastWeaponHit = physicalWeapon;
						lastWeaponHit.SetHoverEffect();
					}
					if (Mouse.current.leftButton.wasPressedThisFrame)
					{
						weapon = physicalWeapon.Swap(weapon);
					}
				}
			}
			else
			{
				lastWeaponHit?.RemoveEffect();
			}
		}
		
		public void SwapWeapons(WeaponScriptable newWeapon)
		{
			weapon = newWeapon;
		}
		
		[Server]
		void OnDeath(GameObject lastAttacker)
		{
			if (playerController)
			{
				return;
			}
			GameObject droppedWeapon = Instantiate(Prefabs.GetInstance().PhysicalWeapon, transform.position, Quaternion.identity);
			droppedWeapon.GetComponent<PhysicalWeapon>().SetWeapon(weapon);
			NetworkServer.Spawn(droppedWeapon);
		}
	}
}
