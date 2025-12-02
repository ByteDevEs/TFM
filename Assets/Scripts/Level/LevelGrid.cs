using System;
using Mirror;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;
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
		
		[SyncVar]
		protected bool[] levelCells;
		[SyncVar] public int3 startPosition, exitPosition;
		
		[SyncVar] protected int level = 0;
		[SyncVar(hook = nameof(PlayerCountChanged))] protected int playersInRoom = 0;
		protected GameObject mesh;
		
		protected NavMeshSurface surface;

		void Awake()
		{
			surface = GetComponent<NavMeshSurface>();
		}

		public abstract void GenerateMesh();

		[Server]
		public void AddPlayer()
		{
			playersInRoom++;
		}

		[Server]
		public void RemovePlayer()
		{
			playersInRoom--;
		}
		
		public void PlayerCountChanged(int oldPlayerCount, int newPlayerCount)
		{
			if (newPlayerCount == 0)
			{
				Destroy(mesh);
			}
			else if (oldPlayerCount == 0)
			{
				GenerateMesh();
			}
		}
	}
}
