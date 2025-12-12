using System;
using Mirror;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
namespace Level
{
	[RequireComponent(typeof(NavMeshSurface),  typeof(NetworkIdentity))]
	public abstract class LevelGrid : NetworkBehaviour
	{
		public GameObject floorPrefab;
		public GameObject wallPrefab;
		public GameObject startPrefab;
		public GameObject exitPrefab;
		public float roomSize = 1;
		public int gridX = 32;
		public int gridY = 32;
		public int minRoomCount = 3;
		public int maxRoomCount = 7;
		public int minRoomSize = 1;
		public int maxRoomSize = 3;
		[HideInInspector] public int4[] rooms = Array.Empty<int4>();
		[SyncVar] public int3 startPosition, exitPosition;
		[SyncVar] public int level;

		[SyncVar] protected bool[] levelCells;
		[field: SyncVar(hook = nameof(PlayerCountChanged))]
		int PlayersInRoom { get; set; }
		protected GameObject container;
		protected GameObject mesh;
		protected NavMeshSurface surface;

		void Awake()
		{
			surface = GetComponent<NavMeshSurface>();
			container =  new GameObject("LevelGrid");
			container.transform.SetParent(transform, false); 
		}

		public abstract void GenerateMesh();

		[Server]
		public void AddPlayer()
		{
			PlayersInRoom++;
		}

		[Server]
		public void RemovePlayer()
		{
			PlayersInRoom--;
		}
		
		public void PlayerCountChanged(int oldPlayerCount, int newPlayerCount)
		{
			if (newPlayerCount == 0)
			{
				container.SetActive(false);
			}
			else if (oldPlayerCount == 0)
			{
				if (mesh)
				{
					container.SetActive(true);
				}
				else
				{
					GenerateMesh();
				}
				
				foreach (NavMeshAgent meshAgent in container.GetComponentsInChildren<NavMeshAgent>())
				{
					meshAgent.enabled = true;
				}
			}
		}
	}
}
