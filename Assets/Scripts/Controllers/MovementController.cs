using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Controllers
{
	[RequireComponent(typeof(NavMeshAgent), typeof(NetworkTransformReliable))]
	public class MovementController : NetworkBehaviour
	{
		static readonly int Walking = Animator.StringToHash("Walking");
		[SerializeField] float DashDistance = 5f;
		[SerializeField] float DashDuration = 0.2f;
		[SerializeField] float DashCooldown = 2f;
		
		[SerializeField] float FootstepSoundMaxCooldown = 0.33f;
		public float footstepSoundCooldown;
		
		public float Speed;
		float baseSpeed;
		
		public Vector3 Destination { get; private set; }
		public float RemainingDistance => navMeshAgent.enabled ? navMeshAgent.remainingDistance : Mathf.Infinity;
		public float StoppingDistance => navMeshAgent.enabled ? navMeshAgent.stoppingDistance : Mathf.Infinity;

		AttackController attackController;
		Animator animator;
		NavMeshAgent navMeshAgent;
		double lastDashTime;
		bool isDashing;

		void Awake()
		{
			navMeshAgent = GetComponent<NavMeshAgent>();
			attackController = GetComponent<AttackController>();
			animator = GetComponent<Animator>();
			baseSpeed = navMeshAgent.speed;
		}

		void Update()
		{
			navMeshAgent.speed = baseSpeed + Speed - 1;
			Speed = attackController.Stats.Speed;

			if (navMeshAgent.velocity != Vector3.zero)
			{
				footstepSoundCooldown -= Time.deltaTime * (navMeshAgent.speed / baseSpeed);
				if (footstepSoundCooldown <= 0)
				{
					Prefabs.GetInstance().PlaySound("Footstep", transform);
					footstepSoundCooldown = FootstepSoundMaxCooldown;
				}
			}

			if (isServer && animator)
			{
				bool isWalking = navMeshAgent.velocity.magnitude > 0;
				animator.speed = isWalking ? navMeshAgent.speed / baseSpeed : 1.0f;
				animator.SetBool(Walking, navMeshAgent.velocity.magnitude > 0);
			}
		}

		public void Move(Vector3 position)
		{
			if (!isLocalPlayer)
			{
				return;
			}
			
			CmdMove(position);
		}

		public void Move(Ray ray)
		{
			if (!Mouse.current.press.isPressed)
			{
				return;
			}

			if (!isLocalPlayer)
			{
				return;
			}
			
			CmdMove(ray);
		}

		public void Dash()
		{
			if (!isLocalPlayer)
			{
				return;
			}
			
			if (NetworkTime.time < lastDashTime + DashCooldown || isDashing) return;

			CmdDash();
		}

		public void Stop()
		{
			if (!isLocalPlayer)
			{
				return;
			}
			
			CmdStop();
		}

		[Command]
		void CmdMove(Vector3 position)
		{
			Destination = position;
			navMeshAgent.SetDestination(Destination);
		}
		
		[Server]
		public void SrvMove(Vector3 position)
		{
			Destination = position;
			navMeshAgent.SetDestination(Destination);
		}

		[Server]
		IEnumerator ServerDashRoutine()
		{
			isDashing = true;
			lastDashTime = NetworkTime.time;
			RpcPlaySound("Dash");

			Vector3 dashDirection = transform.forward;
			if (navMeshAgent.velocity.sqrMagnitude > 0.1f)
			{
				dashDirection = navMeshAgent.velocity.normalized;
			}

			navMeshAgent.ResetPath();
            
			float timer = 0f;
			float speed = DashDistance / DashDuration;

			while (timer < DashDuration)
			{
				navMeshAgent.Move(dashDirection * (speed * Time.deltaTime));
                
				timer += Time.deltaTime;
				yield return null;
			}

			navMeshAgent.velocity = Vector3.zero;
			isDashing = false;
		}
		
		[ClientRpc]
		void RpcPlaySound(string soundName)
		{
			Prefabs.GetInstance().PlaySound(soundName, transform);
		}
		[Command]
		void CmdMove(Ray ray)
		{
			if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, Settings.GetInstance().GroundLayerMask))
			{
				return;
			}
			
			Debug.Log("Clicking: " + hit.transform.name);

			Destination = hit.point;
			navMeshAgent.SetDestination(Destination);
		}

		[Command]
		void CmdStop()
		{
			Destination = transform.position;
			navMeshAgent.SetDestination(Destination);
		}
		
		[Command]
		void CmdDash()
		{
			// Server validates cooldown
			if (NetworkTime.time < lastDashTime + DashCooldown || isDashing) return;

			// Start the dash logic
			StartCoroutine(ServerDashRoutine());
		}

		[Server]
		public void SrvStop()
		{
			Destination = transform.position;
			navMeshAgent.SetDestination(Destination);
		}
	}
}