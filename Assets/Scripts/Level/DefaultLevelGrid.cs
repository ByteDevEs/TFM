using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Level
{
	public class DefaultLevelGrid : LevelGrid
	{
		public float SpawnEnemyProbability = 0.125f;
		public float MaxSpawnEnemyProbability = 0.375f;
		public int MaxEnemiesPerCell = 3;
		
		int[] triangles = Array.Empty<int>();
		List<(int u, int v, float distance)> allEdges;
		List<(int u, int v, float distance)> unusedEdges;
		readonly HashSet<(int u, int v)> uniqueEdges = new HashSet<(int u, int v)>();
		List<int> mstLines;
		LevelGenerator levelGenerator;

		new void Awake()
		{
			levelGenerator = FindAnyObjectByType<LevelGenerator>();

			base.Awake();
		}

		[Server] public void SrvGenerateLevelGrid()
		{
			Rooms = CreateRooms();
			triangles = CreateTriangles();
		}

		[Server] public void SrvGenerateMst()
		{
			mstLines = CalculateMinimumSpanningTree(triangles);

			const float reusePercentage = 0.125f;
			int numCyclesToAdd = Mathf.RoundToInt(mstLines.Count * reusePercentage);


			for (int i = 0; i < numCyclesToAdd && i < unusedEdges.Count; i++)
			{
				mstLines.Add(unusedEdges[i].u);
				mstLines.Add(unusedEdges[i].v);
			}
		}

		[Server] public void SrvGenerateCells()
		{
			LevelCells = new bool[GridX * GridY];
			for (int i = 0; i < GridX; i++)
			{
				for (int j = 0; j < GridY; j++)
				{
					foreach (int4 room in Rooms)
					{
						if (IsOverlapping(room, new int4(i, j, 1, 1)))
						{
							LevelCells[i * GridX + j] = true;
						}
					}
				}
			}

			if (mstLines == null)
			{
				return;
			}
			
			for (int i = 0; i < mstLines.Count; i += 2)
			{
				int indexA = mstLines[i];
				int indexB = mstLines[i + 1];

				int4 roomA = Rooms[indexA];
				int4 roomB = Rooms[indexB];

				CarvePath(roomA, roomB);
			}
		}

		[Server] public void SrvGenerateStartAndExit()
		{
			int startRoomIndex = Random.Range(0, Rooms.Length);
			int exitRoomIndex = Random.Range(0, Rooms.Length);

			int attempts = 0;
			while (startRoomIndex == exitRoomIndex && attempts < 100)
			{
				exitRoomIndex = Random.Range(0, Rooms.Length);
				attempts++;
			}

			int4 startRoom = Rooms[startRoomIndex];
			int4 exitRoom = Rooms[exitRoomIndex];

			StartPosition = FindRandomWallForRoom(startRoom);
			ExitPosition = FindRandomWallForRoom(exitRoom);
		}
		
		[Server]
		public void SrvGenerateEnemies()
		{
			for (int x = 0; x < LevelCells.Length; x++)
			{
				int i = x % GridX;
				int j = x / GridX;
				
				if (LevelCells[i * GridX + j])
				{
					float mE = Random.Range(1, MaxEnemiesPerCell);
					float r = Random.Range(0f, 1f);
					float probability = Mathf.Lerp(SpawnEnemyProbability, MaxSpawnEnemyProbability, Mathf.Clamp01(Level / (levelGenerator.LastLevel/4.0f)));
					for (int k = 0; k < mE; k++)
					{
						if (r < probability)
						{
							Vector2 circle = Random.insideUnitCircle;
							Vector3 localPos = new Vector3(i * RoomSize, 0, j * RoomSize) + new Vector3(circle.x, 0, circle.y);
							GameObject enemyGo = Instantiate(Prefabs.GetInstance().PrototypeEnemy, Container.transform.position + localPos, Quaternion.identity, Container.transform);
							NetworkServer.Spawn(enemyGo);
						}
					}
				}
			}
		}

		protected override void GenerateMesh()
		{
			print("Generating mesh");
			
			Mesh = new GameObject($"LevelMesh_{Level}");
			Mesh.transform.SetParent(Container.transform); 
			Mesh.transform.localPosition = Vector3.zero;
			
			for (int x = 0; x < LevelCells.Length; x++)
			{
				int i = x % GridX;
				int j = x / GridX;
				GameObject cellContainer = new GameObject($"CellContainer_{i}_{j}");
				cellContainer.transform.SetParent(Mesh.transform);
				cellContainer.transform.localPosition = new Vector3(i * RoomSize, 0, j * RoomSize);
				cellContainer.transform.localScale = Vector3.one * RoomSize;

				if (StartPosition.xy.Equals(new int2(i, j)))
				{
					GameObject stairsDown = Instantiate(StartPrefab, cellContainer.transform);
					stairsDown.transform.localPosition = new Vector3(0, -0.5f, 0);
					stairsDown.transform.rotation = Quaternion.Euler(0, StartPosition.z, 0);
					stairsDown.name = $"StairsDown_{i}_{j}";
					stairsDown.GetComponent<GateController>().CurrentLevel = Level;
					continue;
				}

				if (ExitPosition.xy.Equals(new int2(i, j)))
				{
					GameObject stairsUp = Instantiate(ExitPrefab, cellContainer.transform);
					stairsUp.transform.localPosition = new Vector3(0, -0.5f, 0);
					stairsUp.transform.rotation = Quaternion.Euler(0, ExitPosition.z, 0);
					stairsUp.name = $"StairsUp_{i}_{j}";
					stairsUp.GetComponent<GateController>().CurrentLevel = Level;
					continue;
				}
				
				if (LevelCells[i * GridX + j])
				{
					GameObject floor = Instantiate(FloorPrefab, cellContainer.transform);
					floor.transform.localPosition = new Vector3(0, -0.5f, 0);
					floor.name = $"Floor_{i}_{j}";
				}
				else
				{
					GameObject wallContainer = new GameObject($"WallContainer_{i}_{j}");
					wallContainer.transform.SetParent(cellContainer.transform, false);
					if (i - 1 > 0 && LevelCells[(i - 1) * GridX + j])
					{
						const int rot = 90;
						GameObject wall = Instantiate(WallPrefab, wallContainer.transform);
						wall.transform.localPosition = new Vector3(0, -0.5f, 0);
						wall.transform.localRotation = Quaternion.Euler(0, rot, 0);
						wall.name = $"Wall_{rot}_{i}_{j}";
					}
					
					if (j - 1 > 0 && LevelCells[i * GridX + (j - 1)])
					{
						const int rot = 0;
						GameObject wall = Instantiate(WallPrefab, wallContainer.transform);
						wall.transform.localPosition = new Vector3(0, -0.5f, 0);
						wall.transform.localRotation = Quaternion.Euler(0, rot, 0);
						wall.name = $"Wall_{rot}_{i}_{j}";
					}
					
					bool n  = IsWall(i - 1, j);     // Up
					bool s  = IsWall(i + 1, j);     // Down
					bool w  = IsWall(i, j - 1);     // Left
					bool e  = IsWall(i, j + 1);     // Right
					bool nw = IsWall(i - 1, j - 1); // Up-Left
					bool ne = IsWall(i - 1, j + 1); // Up-Right
					bool se = IsWall(i + 1, j + 1); // Down-Right
					bool sw = IsWall(i + 1, j - 1); // Down-Left
					
					if (n && w || !n && !w && nw)
					{
						SpawnColumn(wallContainer, 0, i, j);
					}
					
					if (n && e || !n && !e && ne)
					{
						SpawnColumn(wallContainer, 90, i, j);
					}
					
					if (s && e || !s && !e && se)
					{
						SpawnColumn(wallContainer, 180, i, j);
					}
					
					if (s && w || !s && !w && sw)
					{
						SpawnColumn(wallContainer, 270, i, j); 
					}
				}
			}
		}
		
		bool IsWall(int r, int c)
		{
			if (r < 0 || r >= GridX || c < 0 || c >= GridY)
				return false;
    
			return LevelCells[r * GridX + c];
		}
		
		void SpawnColumn(GameObject cellContainer, int rot, int r, int c)
		{
			GameObject column = Instantiate(ColumnPrefab, cellContainer.transform);
			column.transform.localPosition = new Vector3(0, -0.5f, 0); 
			column.transform.localRotation = Quaternion.Euler(0, rot, 0);
			column.name = $"Column_{rot}_{r}_{c}";
		}

		int3 FindRandomWallForRoom(int4 room)
		{
			List<int3> validWalls = new List<int3>();

			int xStart = room.x;
			int xEnd = room.x + room.z - 1;
			int yStart = room.y;
			int yEnd = room.y + room.w - 1;

			AddValidWalls(xStart, xEnd, yStart - 1, true, 0, validWalls);
			AddValidWalls(xStart, xEnd, yEnd + 1, true, 180, validWalls);
			AddValidWalls(yStart, yEnd, xStart - 1, false, 90, validWalls);
			AddValidWalls(yStart, yEnd, xEnd + 1, false, 270, validWalls);

			return validWalls.Count > 0 ? validWalls[Random.Range(0, validWalls.Count)] : new int3(room.x + room.z / 2, room.y + room.w / 2, 0);
		}

		void AddValidWalls(int rangeMin, int rangeMax, int fixedAxis, bool isHorizontal, int rotationY, List<int3> results)
		{
			for (int i = rangeMin; i <= rangeMax; i++)
			{
				int x = isHorizontal ? i : fixedAxis;
				int y = isHorizontal ? fixedAxis : i;

				if (x < 0 || x >= GridX || y < 0 || y >= GridY) continue;

				if (!LevelCells[x * GridX + y])
				{
					results.Add(new int3(x, y, rotationY));
				}
			}
		}

		void CarvePath(int4 roomA, int4 roomB)
		{
			int2 startPos = new int2(roomA.x + roomA.z / 2, roomA.y + roomA.w / 2);
			int2 endPos = new int2(roomB.x + roomB.z / 2, roomB.y + roomB.w / 2);

			bool goHorizontalFirst = Random.value > 0.5f;

			if (goHorizontalFirst)
			{
				CarveHorizontal(startPos.x, endPos.x, startPos.y);
				CarveVertical(startPos.y, endPos.y, endPos.x);
			}
			else
			{
				CarveVertical(startPos.y, endPos.y, startPos.x);
				CarveHorizontal(startPos.x, endPos.x, endPos.y);
			}
		}

		void CarveHorizontal(int xStart, int xEnd, int yFixed)
		{
			int low = math.min(xStart, xEnd);
			int high = math.max(xStart, xEnd);

			for (int x = low; x <= high; x++)
			{
				if (IsValid(x, yFixed))
				{
					LevelCells[x * GridX + yFixed] = true;
				}
			}
		}

		void CarveVertical(int yStart, int yEnd, int xFixed)
		{
			int low = math.min(yStart, yEnd);
			int high = math.max(yStart, yEnd);

			for (int y = low; y <= high; y++)
			{
				if (IsValid(xFixed, y))
				{
					LevelCells[xFixed * GridX + y] = true;
				}
			}
		}

		bool IsValid(int x, int y)
		{
			return x >= 0 && x < GridX && y >= 0 && y < GridY;
		}

		int4[] CreateRooms()
		{
			List<int4> generatedRooms = new List<int4>();
			int roomCount = Random.Range(MinRoomCount, MaxRoomCount);
			const int maxAttempts = 100;

			for (int i = 0; i < roomCount; i++)
			{
				bool canBePlaced = false;
				int currentAttempts = 0;

				while (!canBePlaced && currentAttempts < maxAttempts)
				{
					currentAttempts++;
					int w = Random.Range(MinRoomSize, MaxRoomSize);
					int h = Random.Range(MinRoomSize, MaxRoomSize);
					int x = Random.Range(1, GridX - w - 1);
					int y = Random.Range(1, GridY - h - 1);
					int4 newRoom = new int4(x, y, w, h);

					canBePlaced = generatedRooms.All(r => !IsOverlappingWithPadding(newRoom, r));

					if (canBePlaced)
					{
						generatedRooms.Add(newRoom);
					}
				}
			}
			return generatedRooms.ToArray();
		}

		int[] CreateTriangles()
		{
			IPoint[] points = Rooms
				.Select(room => (IPoint)new Point(room.x, room.y))
				.ToArray();

			Delaunator delaunator = new Delaunator(points);
			return delaunator.Triangles;
		}

		List<int> CalculateMinimumSpanningTree(int[] originalTriangles)
		{
			allEdges = new List<(int, int, float)>();
			unusedEdges = new List<(int, int, float)>();

			for (int i = 0; i < originalTriangles.Length; i += 3)
			{
				int a = originalTriangles[i];
				int b = originalTriangles[i + 1];
				int c = originalTriangles[i + 2];

				TryAddUniqueEdge(a, b);
				TryAddUniqueEdge(b, c);
				TryAddUniqueEdge(c, a);
			}

			allEdges.Sort((x, y) => x.distance.CompareTo(y.distance));

			List<int> mstResult = new List<int>();

			int[] parent = new int[Rooms.Length];
			for (int i = 0; i < parent.Length; i++) parent[i] = i;

			foreach ((int u, int v, float distance) edge in allEdges)
			{
				int rootU = edge.u;
				while (parent[rootU] != rootU)
				{
					rootU = parent[rootU];
				}

				int rootV = edge.v;
				while (parent[rootV] != rootV)
				{
					rootV = parent[rootV];
				}

				if (rootU == rootV)
				{
					unusedEdges.Add(edge);
				}
				else
				{
					parent[rootV] = rootU;
					mstResult.Add(edge.u);
					mstResult.Add(edge.v);
				}
			}

			return mstResult;
		}

		void TryAddUniqueEdge(int u, int v)
		{
			int smaller = Math.Min(u, v);
			int larger = Math.Max(u, v);

			if (!uniqueEdges.Add((smaller, larger)))
			{
				return;
			}

			Vector2 posU = new Vector2(Rooms[u].x, Rooms[u].y);
			Vector2 posV = new Vector2(Rooms[v].x, Rooms[v].y);
			float distance = Vector2.Distance(posU, posV);

			allEdges.Add((u, v, distance));
		}

		static bool IsOverlappingWithPadding(int4 a, int4 b)
		{
			const int padding = 3;

			return a.x - padding < b.x + b.z &&
			       a.x + a.z + padding > b.x &&
			       a.y - padding < b.y + b.w &&
			       a.y + a.w + padding > b.y;
		}

		static bool IsOverlapping(int4 a, int4 b)
		{
			return a.x < b.x + b.z &&
			       a.x + a.z > b.x &&
			       a.y < b.y + b.w &&
			       a.y + a.w > b.y;
		}

		void OnDrawGizmos()
		{
			if (Rooms == null || triangles == null)
			{
				return;
			}

			if (triangles.Length <= 0)
			{
				return;
			}

			if (LevelCells != null && LevelCells.Length != 0)
			{
				for (int i = 0; i < GridX; i++)
				{
					for (int j = 0; j < GridY; j++)
					{
						Gizmos.color = LevelCells[i * GridX + j] ? Color.mediumPurple : Color.white;
						Gizmos.DrawCube(new Vector3(i * RoomSize, 0, j * RoomSize), Vector3.one * 0.5f * RoomSize);

						if (!StartPosition.xy.Equals(new int2(i, j)) && !ExitPosition.xy.Equals(new int2(i, j)))
						{
							continue;
						}
						
						Gizmos.color = Color.yellow;
						Gizmos.DrawCube(new Vector3(i * RoomSize, 0, j * RoomSize), Vector3.one * 0.5f * RoomSize + Vector3.up * 2f);
					}
				}
				
				return;
			}

			Gizmos.color = Color.red;
			foreach (int4 room in Rooms)
			{
				Gizmos.DrawCube(new Vector3(room.x * RoomSize, 0, room.y * RoomSize), new Vector3(room.z * RoomSize, RoomSize, room.w * RoomSize));
			}

			if (mstLines != null && mstLines.Count != 0)
			{
				Gizmos.color = Color.blue;
				for (int i = 0; i < mstLines.Count; i += 2)
				{
					int indexA = mstLines[i];
					int indexB = mstLines[i + 1];

					if (indexA >= Rooms.Length || indexB >= Rooms.Length)
					{
						continue;
					}

					Vector3 pA = new Vector3(Rooms[indexA].x * RoomSize, 0, Rooms[indexA].y * RoomSize);
					Vector3 pB = new Vector3(Rooms[indexB].x * RoomSize, 0, Rooms[indexB].y * RoomSize);

					Gizmos.DrawLine(pA, pB);
				}
				
				return;
			}

			Gizmos.color = Color.green;
			for (int i = 0; i < triangles.Length; i += 3)
			{
				if (i + 2 >= triangles.Length)
				{
					break;
				}

				int index0 = triangles[i];
				int index1 = triangles[i + 1];
				int index2 = triangles[i + 2];

				if (index0 >= Rooms.Length || index1 >= Rooms.Length || index2 >= Rooms.Length)
				{
					continue;
				}

				Vector3 p0 = new Vector3(Rooms[index0].x * RoomSize, 0, Rooms[index0].y * RoomSize);
				Vector3 p1 = new Vector3(Rooms[index1].x * RoomSize, 0, Rooms[index1].y * RoomSize);
				Vector3 p2 = new Vector3(Rooms[index2].x * RoomSize, 0, Rooms[index2].y * RoomSize);

				Gizmos.DrawLine(p0, p1);
				Gizmos.DrawLine(p1, p2);
				Gizmos.DrawLine(p2, p0);
			}
		}
	}
}
