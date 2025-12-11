using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Mirror;
using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Level
{
	public class DefaultLevelGrid : LevelGrid
	{
		int[] triangles = Array.Empty<int>();
		List<(int u, int v, float distance)> allEdges;
		List<(int u, int v, float distance)> unusedEdges;
		readonly HashSet<(int u, int v)> uniqueEdges = new HashSet<(int u, int v)>();
		List<int> mstLines;

		[Server] public void SrvGenerateLevelGrid()
		{
			rooms = CreateRooms();
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
			levelCells = new bool[gridX * gridY];
			for (int i = 0; i < gridX; i++)
			{
				for (int j = 0; j < gridY; j++)
				{
					foreach (int4 room in rooms)
					{
						if (IsOverlapping(room, new int4(i, j, 1, 1)))
						{
							levelCells[i * gridX + j] = true;
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

				int4 roomA = rooms[indexA];
				int4 roomB = rooms[indexB];

				CarvePath(roomA, roomB);
			}
		}

		[Server] public void SrvGenerateStartAndExit()
		{
			int startRoomIndex = Random.Range(0, rooms.Length);
			int exitRoomIndex = Random.Range(0, rooms.Length);

			int attempts = 0;
			while (startRoomIndex == exitRoomIndex && attempts < 100)
			{
				exitRoomIndex = Random.Range(0, rooms.Length);
				attempts++;
			}

			int4 startRoom = rooms[startRoomIndex];
			int4 exitRoom = rooms[exitRoomIndex];

			startPosition = FindRandomWallForRoom(startRoom);
			exitPosition = FindRandomWallForRoom(exitRoom);
		}

		public override void GenerateMesh()
		{
			print("Generating mesh");
			
			mesh = new GameObject($"LevelMesh_{level}");
			mesh.transform.SetParent(transform, false); 
			mesh.transform.localPosition = Vector3.zero;
			
			for (int x = 0; x < levelCells.Length; x++)
			{
				int i = x % gridX;
				int j = x / gridX;
				GameObject cellContainer = new GameObject($"CellContainer_{i}_{j}");
				cellContainer.transform.SetParent(mesh.transform, false);
				cellContainer.transform.localPosition = new Vector3(i * roomSize, 0, j * roomSize);
				cellContainer.transform.localScale = Vector3.one * roomSize;

				if (startPosition.xy.Equals(new int2(i, j)))
				{
					GameObject stairsDown = Instantiate(startPrefab, cellContainer.transform);
					stairsDown.transform.localPosition = new Vector3(0, -0.5f, 0);
					stairsDown.transform.rotation = Quaternion.Euler(0, startPosition.z, 0);
					stairsDown.name = $"StairsDown_{i}_{j}";
					stairsDown.GetComponent<GateController>().currentLevel = level;
					continue;
				}

				if (exitPosition.xy.Equals(new int2(i, j)))
				{
					GameObject stairsUp = Instantiate(exitPrefab, cellContainer.transform);
					stairsUp.transform.localPosition = new Vector3(0, -0.5f, 0);
					stairsUp.transform.rotation = Quaternion.Euler(0, exitPosition.z, 0);
					stairsUp.name = $"StairsUp_{i}_{j}";
					stairsUp.GetComponent<GateController>().currentLevel = level;
					continue;
				}
				
				if (levelCells[i * gridX + j])
				{
					GameObject floor = Instantiate(floorPrefab, cellContainer.transform);
					floor.transform.localPosition = new Vector3(0, -0.5f, 0);
					floor.name = $"Floor_{i}_{j}";
				}
				else
				{
					GameObject wall = Instantiate(wallPrefab, cellContainer.transform);
					wall.transform.localPosition = new Vector3(0, -0.5f, 0);
					wall.name = $"Wall_{i}_{j}";
				}
			}

			surface.BuildNavMesh();
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

				if (x < 0 || x >= gridX || y < 0 || y >= gridY) continue;

				if (!levelCells[x * gridX + y])
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
					levelCells[x * gridX + yFixed] = true;
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
					levelCells[xFixed * gridX + y] = true;
				}
			}
		}

		bool IsValid(int x, int y)
		{
			return x >= 0 && x < gridX && y >= 0 && y < gridY;
		}

		int4[] CreateRooms()
		{
			List<int4> generatedRooms = new List<int4>();
			int roomCount = Random.Range(minRoomCount, maxRoomCount);
			const int maxAttempts = 100;

			for (int i = 0; i < roomCount; i++)
			{
				bool canBePlaced = false;
				int currentAttempts = 0;

				while (!canBePlaced && currentAttempts < maxAttempts)
				{
					currentAttempts++;
					int w = Random.Range(minRoomSize, maxRoomSize);
					int h = Random.Range(minRoomSize, maxRoomSize);
					int x = Random.Range(1, gridX - w - 1);
					int y = Random.Range(1, gridY - h - 1);
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
			IPoint[] points = rooms
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

			int[] parent = new int[rooms.Length];
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

			Vector2 posU = new Vector2(rooms[u].x, rooms[u].y);
			Vector2 posV = new Vector2(rooms[v].x, rooms[v].y);
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
			if (rooms == null || triangles == null)
			{
				return;
			}

			if (triangles.Length <= 0)
			{
				return;
			}

			if (levelCells != null && levelCells.Length != 0)
			{
				for (int i = 0; i < gridX; i++)
				{
					for (int j = 0; j < gridY; j++)
					{
						Gizmos.color = levelCells[i * gridX + j] ? Color.mediumPurple : Color.white;
						Gizmos.DrawCube(new Vector3(i * roomSize, 0, j * roomSize), Vector3.one * 0.5f * roomSize);

						if (!startPosition.xy.Equals(new int2(i, j)) && !exitPosition.xy.Equals(new int2(i, j)))
						{
							continue;
						}
						
						Gizmos.color = Color.yellow;
						Gizmos.DrawCube(new Vector3(i * roomSize, 0, j * roomSize), Vector3.one * 0.5f * roomSize + Vector3.up * 2f);
					}
				}
				
				return;
			}

			Gizmos.color = Color.red;
			foreach (int4 room in rooms)
			{
				Gizmos.DrawCube(new Vector3(room.x * roomSize, 0, room.y * roomSize), new Vector3(room.z * roomSize, roomSize, room.w * roomSize));
			}

			if (mstLines != null && mstLines.Count != 0)
			{
				Gizmos.color = Color.blue;
				for (int i = 0; i < mstLines.Count; i += 2)
				{
					int indexA = mstLines[i];
					int indexB = mstLines[i + 1];

					if (indexA >= rooms.Length || indexB >= rooms.Length)
					{
						continue;
					}

					Vector3 pA = new Vector3(rooms[indexA].x * roomSize, 0, rooms[indexA].y * roomSize);
					Vector3 pB = new Vector3(rooms[indexB].x * roomSize, 0, rooms[indexB].y * roomSize);

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

				if (index0 >= rooms.Length || index1 >= rooms.Length || index2 >= rooms.Length)
				{
					continue;
				}

				Vector3 p0 = new Vector3(rooms[index0].x * roomSize, 0, rooms[index0].y * roomSize);
				Vector3 p1 = new Vector3(rooms[index1].x * roomSize, 0, rooms[index1].y * roomSize);
				Vector3 p2 = new Vector3(rooms[index2].x * roomSize, 0, rooms[index2].y * roomSize);

				Gizmos.DrawLine(p0, p1);
				Gizmos.DrawLine(p1, p2);
				Gizmos.DrawLine(p2, p0);
			}
		}
	}
}
