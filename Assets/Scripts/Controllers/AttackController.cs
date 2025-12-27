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
        public CharacterStats Stats { get; private set; }
        public GameObject Hand;
        public Vector3 HandOffset;
        public Vector3 HandRotation;
        public LayerMask AttackableLayer;
        public float ProjectileSpeed = 20f;
        
		PlayerController playerController;
		MovementController movementController;
		HealthController healthController;
		Coroutine attackCoroutine;
		EnemyController lastEnemyHit;
		PhysicalWeapon lastWeaponHit;
        GameObject currentObj;
        Animator animator;
        
		[SyncVar] GameObject selectedTarget;
		[SyncVar] public bool IsAttackingTarget;
		[SyncVar] float weaponCooldown;
        [SyncVar(hook = nameof(OnWeaponIDChanged))] 
		int weaponID = -1;
        
		void Start()
		{
			playerController = GetComponent<PlayerController>();
			movementController = GetComponent<MovementController>();
			healthController = GetComponent<HealthController>();
			animator = GetComponent<Animator>();
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
				bool inRange = distance <= WeaponLibrary.GetWeapon(weaponID).BaseRange;
				
				if (!inRange)
				{
					Vector3 stopPos = GetPositionAtMaxAttackRange(target.transform, WeaponLibrary.GetWeapon(weaponID).BaseRange * 0.75f);
					movementController.SrvMove(stopPos);
				}
				else
				{
					movementController.SrvStop();

					if (weaponCooldown >= WeaponLibrary.GetWeapon(weaponID).BaseCooldown)
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

			float damage = WeaponLibrary.GetWeapon(weaponID).BaseDamage + Stats.Strength;

			switch (WeaponLibrary.GetWeapon(weaponID).AttackType)
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
			IEnumerable<Collider> hits = Physics.OverlapSphere(position, WeaponLibrary.GetWeapon(weaponID).BaseRange / 2.0f, AttackableLayer)
				.Where(hit => !hit.transform.gameObject.CompareTag(transform.tag))
				.Except(GetComponents<Collider>());

			Attack(hits, damage);
			animator.Play(WeaponLibrary.GetWeapon(weaponID).WeaponAttackAnimationClip);
			RpcPlaySound(WeaponLibrary.GetWeapon(weaponID).WeaponAttackSfx);
			yield return null;
		}

		IEnumerator AttackRanged(Transform target, float damage)
		{
		    Vector3 startPos = transform.position;
		    Vector3 targetPos = target.position; 
		    
		    float distance = Vector3.Distance(startPos, targetPos);
		    
		    float flightDuration = distance / ProjectileSpeed; 

		    animator.Play(WeaponLibrary.GetWeapon(weaponID).WeaponAttackAnimationClip);
		    StartCoroutine(AttackRangedVisuals(startPos, targetPos));
		    float timer = 0f;

		    while (timer < flightDuration)
		    {
		        timer += Time.deltaTime;

		        float radius = WeaponLibrary.GetWeapon(weaponID).AttackWidth / 2.0f;

		        IEnumerable<Collider> hits = Physics.OverlapSphere(target.position, radius, AttackableLayer)
			        .Where(hit => !hit.transform.gameObject.CompareTag(transform.tag))
			        .Except(GetComponents<Collider>())
			        .ToList();
		        
		        if (hits.Any())
		        {
			        Attack(hits, damage);
			        break;
		        }

		        yield return null;
		    }
		}
		
		IEnumerator AttackRangedVisuals(Vector3 startPos, Vector3 targetPos)
		{
			yield return new WaitForSeconds(0.25f);

			RpcPlaySound(WeaponLibrary.GetWeapon(weaponID).WeaponAttackSfx);
			RpcSpawnProjectile(startPos, targetPos);
		}

		[ClientRpc]
		void RpcSpawnProjectile(Vector3 startPos, Vector3 targetPos)
		{
		    WeaponScriptable weapon = WeaponLibrary.GetWeapon(weaponID);
		    GameObject prefab = weapon.ProjectilePrefab;
		    
		    GameObject projectile = Instantiate(prefab, startPos, Quaternion.identity);
		    projectile.transform.LookAt(targetPos);
		    
		    StartCoroutine(MoveProjectileTo(projectile, targetPos, ProjectileSpeed));
		}

		IEnumerator MoveProjectileTo(GameObject projectile, Vector3 targetPos, float speed)
		{
			while (projectile != null && Vector3.Distance(projectile.transform.position, targetPos) > 0.05f)
			{
				projectile.transform.position = Vector3.MoveTowards(
					projectile.transform.position,
					targetPos,
					speed * Time.deltaTime
				);

				projectile.transform.LookAt(targetPos);

				yield return null;
			}

			if (projectile != null)
			{
				projectile.transform.position = targetPos;
			}

			Destroy(projectile, 0.1f);
		}

		IEnumerator AttackArea(Transform target, float damage)
		{
			RpcPlaySound(WeaponLibrary.GetWeapon(weaponID).WeaponAttackSfx);
			animator.Play(WeaponLibrary.GetWeapon(weaponID).WeaponAttackAnimationClip);
			
			yield return new WaitForSeconds(WeaponLibrary.GetWeapon(weaponID).CastTime);
			
			IEnumerable<Collider> hits = Physics.OverlapSphere(target.position, WeaponLibrary.GetWeapon(weaponID).AreaDiameter / 2.0f, AttackableLayer)
				.Where(hit => !hit.transform.gameObject.CompareTag(transform.tag))
				.Except(GetComponents<Collider>());
			
			Attack(hits, damage);
			RpcPlaySound(WeaponLibrary.GetWeapon(weaponID).HitSfx);
			RpcSpawnAreaDamage(target.position);
			yield return null;
		}
		
		[ClientRpc]
		void RpcSpawnAreaDamage(Vector3 targetPosition)
		{
			GameObject instantiated = Instantiate(WeaponLibrary.GetWeapon(weaponID).ProjectilePrefab, targetPosition, Quaternion.identity);
			Destroy(instantiated, 4.0f);
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

		[ClientRpc]
		void RpcPlaySound(string soundName)
		{
			Prefabs.GetInstance().PlaySound(soundName, transform);
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
			if (!Mouse.current.rightButton.wasPressedThisFrame)
			{
				return;
			}

			if (!isLocalPlayer)
			{
				return;
			}
			
			CmdSwapWeapons(ray);
		}
		
		[Command]
		void CmdSwapWeapons(Ray ray)
		{
			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				PhysicalWeapon physicalWeapon = hit.collider.GetComponent<PhysicalWeapon>();

				if (!physicalWeapon)
				{
					return;
				}
				
				if (lastWeaponHit != physicalWeapon)
				{
					lastWeaponHit = physicalWeapon;
					lastWeaponHit.SetHoverEffect();
				}
				
				weaponID = physicalWeapon.Swap(weaponID);
				movementController.Stop();
			}
		}
		
		[Server]
		public void SwapWeapons(int newWeaponID)
		{
			weaponID = newWeaponID;
		}
		
		[Server]
		void OnDeath(GameObject lastAttacker)
		{
			if (playerController)
			{
				return;
			}
			GameObject droppedWeapon = Instantiate(Prefabs.GetInstance().PhysicalWeapon, transform.position, Quaternion.identity);
			droppedWeapon.GetComponent<PhysicalWeapon>().SetWeaponId(weaponID);
			NetworkServer.Spawn(droppedWeapon);
		}
		
		void OnWeaponIDChanged(int _, int newID)
		{

			WeaponScriptable weaponData = WeaponLibrary.GetWeapon(newID);
			if (currentObj)
			{
				Destroy(currentObj);
			}

			if (weaponData != null && weaponData.Prefab != null)
			{
				currentObj = Instantiate(weaponData.Prefab, Hand.transform);
				currentObj.transform.localPosition = HandOffset;
				currentObj.transform.localRotation = Quaternion.Euler(HandRotation);
				currentObj.transform.localScale = Vector3.one * 0.5f;
			}
		}
	}
}
