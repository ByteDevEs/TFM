using Mirror;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(NetworkTransformReliable))]
public class MovementController : NetworkBehaviour
{
	private NavMeshAgent navMeshAgent;

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
		
		navMeshAgent.Move(position);
	}
}
