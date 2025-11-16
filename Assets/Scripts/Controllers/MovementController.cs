using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Controllers
{
	[RequireComponent(typeof(NavMeshAgent), typeof(NetworkTransformReliable))]
	public class MovementController : NetworkBehaviour
	{
		NavMeshAgent navMeshAgent;
		Vector3 destination;

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
			destination = position;
			navMeshAgent.SetDestination(destination);
		}
		
		[Server]
		public void SrvMove(Vector3 position)
		{
			destination = position;
			navMeshAgent.SetDestination(destination);
		}

		[Command]
		void CmdMove(Ray ray)
		{
			if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, Settings.GetInstance().groundLayerMask))
			{
				return;
			}

			destination = hit.point;
			navMeshAgent.SetDestination(destination);
		}

		[Command]
		void CmdStop()
		{
			destination = transform.position;
			navMeshAgent.SetDestination(destination);
		}

		[Server]
		public void SrvStop()
		{
			destination = transform.position;
			navMeshAgent.SetDestination(destination);
		}
	}
}