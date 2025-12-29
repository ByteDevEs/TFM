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
		public float FootstepSoundCooldown;
		
		public float Speed;
		float baseSpeed;
		
		public Vector3 Destination { get; private set; }
		public float RemainingDistance => NavMeshAgent.enabled ? NavMeshAgent.remainingDistance : Mathf.Infinity;
		public float StoppingDistance => NavMeshAgent.enabled ? NavMeshAgent.stoppingDistance : Mathf.Infinity;

		public AttackController AttackController;
		public Animator Animator;
		public NavMeshAgent NavMeshAgent;
		double lastDashTime;
		bool isDashing;

		void Start()
		{
			baseSpeed = NavMeshAgent.speed;
		}

		void Update()
		{
			Speed = AttackController.Stats.Speed;
			NavMeshAgent.speed = baseSpeed + Speed - 1;

			if (NavMeshAgent.velocity != Vector3.zero)
			{
				FootstepSoundCooldown -= Time.deltaTime * (NavMeshAgent.speed / baseSpeed);
				if (FootstepSoundCooldown <= 0)
				{
					Prefabs.GetInstance().PlaySound("Footstep", transform);
					FootstepSoundCooldown = FootstepSoundMaxCooldown;
				}
			}

			if (isServer && Animator)
			{
				bool isWalking = NavMeshAgent.velocity.magnitude > 0;
				Animator.speed = isWalking ? NavMeshAgent.speed / baseSpeed : 1.0f;
				Animator.SetBool(Walking, NavMeshAgent.velocity.magnitude > 0);
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
			NavMeshAgent.SetDestination(Destination);
		}
		
		[Server]
		public void SrvMove(Vector3 position)
		{
			Destination = position;
			NavMeshAgent.SetDestination(Destination);
		}

		[Server]
		IEnumerator ServerDashRoutine()
		{
			isDashing = true;
			lastDashTime = NetworkTime.time;
			RpcPlaySound("Dash");

			Vector3 dashDirection = transform.forward;
			if (NavMeshAgent.velocity.sqrMagnitude > 0.1f)
			{
				dashDirection = NavMeshAgent.velocity.normalized;
			}

			NavMeshAgent.ResetPath();
            
			float timer = 0f;
			float speed = DashDistance / DashDuration;

			while (timer < DashDuration)
			{
				NavMeshAgent.Move(dashDirection * (speed * Time.deltaTime));
                
				timer += Time.deltaTime;
				yield return null;
			}

			NavMeshAgent.velocity = Vector3.zero;
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
			
			Destination = hit.point;
			NavMeshAgent.SetDestination(Destination);
		}

		[Command]
		void CmdStop()
		{
			Destination = transform.position;
			NavMeshAgent.SetDestination(Destination);
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
			NavMeshAgent.SetDestination(Destination);
		}
		
		[Server]
		public void SrvLookAt(GameObject target)
		{
			transform.LookAt(target.transform);
		}
	}
}