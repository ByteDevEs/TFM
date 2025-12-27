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
		public GameObject FloorPrefab;
		public GameObject WallPrefab;
		public GameObject ColumnPrefab;
		public GameObject StartPrefab;
		public GameObject ExitPrefab;
		public float RoomSize = 1;
		public int GridX = 32;
		public int GridY = 32;
		public int MinRoomCount = 3;
		public int MaxRoomCount = 7;
		public int MinRoomSize = 1;
		public int MaxRoomSize = 3;
		[HideInInspector] public int4[] Rooms = Array.Empty<int4>();
		[SyncVar] public int3 StartPosition, ExitPosition;
		[SyncVar] public int Level;

		[SyncVar] protected bool[] LevelCells;
		[field: SyncVar(hook = nameof(PlayerCountChanged))]
		int PlayersInRoom { get; set; }
		protected GameObject Container;
		protected GameObject Mesh;
		protected NavMeshSurface Surface;

		void Awake()
		{
			Surface = GetComponent<NavMeshSurface>();
			Container =  new GameObject("LevelGrid");
			Container.transform.SetParent(transform, false); 
		}

		protected abstract void GenerateMesh();

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
				Container.SetActive(false);
			}
			else if (oldPlayerCount == 0)
			{
				if (Mesh)
				{
					Container.SetActive(true);
				}
				else
				{
					GenerateMesh();
				}
				
				foreach (NavMeshAgent meshAgent in Container.GetComponentsInChildren<NavMeshAgent>())
				{
					meshAgent.enabled = true;
				}
			}
		}
	}
}
