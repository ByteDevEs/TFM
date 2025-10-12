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

		public void Move(Ray ray)
		{
			if (!isLocalPlayer)
			{
				return;
			}

			if (!Mouse.current.press.isPressed) return;

			if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, Settings.GetInstance().groundLayerMask))
				return;
			
			destination = hit.point;
			navMeshAgent.SetDestination(destination);
		}
	}
}