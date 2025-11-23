using System.Collections;
using Enemies;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapons;

namespace Controllers
{
	public class AttackController : NetworkBehaviour
	{
		MovementController movementController;

		[SyncVar] CharacterStats stats;
		public WeaponScriptable weapon;

		EnemyController lastEnemyHit;
		[SyncVar] EnemyController selectedEnemy;

		[SyncVar] public bool isAttackingTarget;

		[SyncVar] float weaponCooldown;
		Coroutine attackCoroutine;

		void Start()
		{
			movementController = GetComponent<MovementController>();
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
					lastEnemyHit?.RemoveHoverEffect();
					lastEnemyHit = enemy;
					lastEnemyHit.SetHoverEffect();
				}

				if (Mouse.current.leftButton.wasPressedThisFrame)
				{
					SelectTarget(enemy);
				}

				return true;
			}

			if (isLocalPlayer && Mouse.current.leftButton.wasPressedThisFrame)
			{
				CmdStopAttacking();
			}

			lastEnemyHit?.RemoveHoverEffect();
			lastEnemyHit = null;
			return false;
		}

		void SelectTarget(EnemyController enemy)
		{
			if (selectedEnemy && selectedEnemy != enemy)
			{
				lastEnemyHit.RemoveHoverEffect();
			}

			if (!isLocalPlayer)
			{
				return;
			}

			lastEnemyHit.SetClickEffect();
			
			CmdAttack(enemy);
		}
		
		[Command]
		void CmdAttack(EnemyController enemy)
		{
			selectedEnemy = enemy;
			isAttackingTarget = true;

			if (attackCoroutine != null)
			{
				StopCoroutine(attackCoroutine);
			}

			movementController.SrvStop();
			attackCoroutine = StartCoroutine(Attack(selectedEnemy.gameObject));
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
			isAttackingTarget = false;
		}

		IEnumerator Attack(GameObject target)
		{
			while (isAttackingTarget && target != null)
			{
				while (weaponCooldown < weapon.baseCooldown) yield return null;

				float distance = Vector3.Distance(transform.position, target.transform.position);

				if (distance > weapon.baseRange)
				{
					Vector3 stopPos = GetPositionAtMaxAttackRange(target.transform, weapon.baseRange * 0.9f);
					movementController.SrvMove(stopPos);
					print("Out Of Range");
				}
				else
				{
					movementController.SrvStop();
					weaponCooldown = 0;
					PerformAttack(target);
				}

				yield return null;
			}
		}

		void PerformAttack(GameObject target)
		{
			if (!isServer) return;

			float damage = weapon.baseDamage * (1f + stats.Strength);

			switch (weapon.attackType)
			{
				case AttackType.Melee:
					AttackMelee(target, damage);
					break;

				case AttackType.Ranged:
					AttackRanged(target, damage);
					break;

				case AttackType.Area:
					AttackArea(target, damage);
					break;
			}
		}

		void AttackMelee(GameObject target, float damage)
		{
			if (target.GetComponent<HealthController>() is {} healthController)
			{
				healthController.TakeDamage((int)damage);
			}
		}

		void AttackRanged(GameObject target, float damage)
		{
			if (target.GetComponent<HealthController>() is {} healthController)
			{
				healthController.TakeDamage((int)damage);
			}
		}

		void AttackArea(GameObject target, float damage)
		{
			Collider[] hits = Physics.OverlapSphere(target.transform.position, weapon.areaDiameter / 2.0f);
			foreach (Collider c in hits)
			{
				if (c.GetComponent<HealthController>() is {} healthController)
				{
					healthController.TakeDamage((int)damage);
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
			if (!Physics.Raycast(ray, out RaycastHit hit)) return;

			PhysicalWeapon physicalWeapon = hit.collider.GetComponent<PhysicalWeapon>();

			if (physicalWeapon)
			{
				if (Mouse.current.leftButton.wasPressedThisFrame)
				{
					weapon = physicalWeapon.Swap(weapon);
				}
			}
		}
	}
}
