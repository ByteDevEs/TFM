using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Controllers
{
	[RequireComponent(typeof(NavMeshAgent), typeof(NetworkTransformReliable))]
	public class MovementController : NetworkBehaviour
	{
		public Vector3 Destination { get; private set; }
		public float RemainingDistance => navMeshAgent.enabled ? navMeshAgent.remainingDistance : Mathf.Infinity;
		public float StoppingDistance => navMeshAgent.enabled ? navMeshAgent.stoppingDistance : Mathf.Infinity;
		
		NavMeshAgent navMeshAgent;

		void Awake()
		{
			navMeshAgent = GetComponent<NavMeshAgent>();
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

		[Command]
		void CmdMove(Ray ray)
		{
			if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, Settings.GetInstance().GroundLayerMask))
			{
				return;
			}

			Destination = hit.point;
			navMeshAgent.SetDestination(Destination);
		}

		[Command]
		void CmdStop()
		{
			Destination = transform.position;
			navMeshAgent.SetDestination(Destination);
		}

		[Server]
		public void SrvStop()
		{
			Destination = transform.position;
			navMeshAgent.SetDestination(Destination);
		}
	}
}