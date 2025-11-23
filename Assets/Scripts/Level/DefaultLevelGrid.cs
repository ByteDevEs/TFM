using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using UnityEngine;
using Unity.Mathematics;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

namespace Level
{
	public class DefaultLevelGrid : LevelGrid
	{
		public int gridX = 32;
		public int gridY = 32;

		public int minRoomCount = 3;
		public int maxRoomCount = 7;
		public int maxRoomSize = 3;

		public int4[] rooms = Array.Empty<int4>();
		int[] triangles = Array.Empty<int>();
		List<(int u, int v, float distance)> allEdges;
		List<(int u, int v, float distance)> unusedEdges;
		HashSet<(int u, int v)> uniqueEdges = new HashSet<(int u, int v)>();
		public List<int> mstLines;
		
		public override void GenerateLevelGrid()
		{
			rooms = CreateRooms();
			triangles = CreateTriangles();

			print($"Rooms: {rooms.Length}, Triangles: {triangles.Length / 3}");
		}

		public override void GenerateMst()
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
					int w = Random.Range(1, maxRoomSize);
					int h = Random.Range(1, maxRoomSize);
					int x = Random.Range(0, gridX - w);
					int y = Random.Range(0, gridY - h);
					int4 newRoom = new int4(x, y, w, h);

					canBePlaced = generatedRooms.All(r => !IsOverlapping(newRoom, r));

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

			foreach ((int, int, float) edge in allEdges)
			{
				
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

		static bool IsOverlapping(int4 a, int4 b)
		{
			const int padding = 3;

			return a.x - padding < b.x + b.z &&
			       a.x + a.z + padding > b.x &&
			       a.y - padding < b.y + b.w &&
			       a.y + a.w + padding > b.y;
		}

		void OnDrawGizmos()
		{
			if (rooms == null || triangles == null) return;

			Gizmos.color = Color.red;
			foreach (int4 room in rooms)
			{
				Gizmos.DrawCube(new Vector3(room.x, 0, room.y), new Vector3(room.z, 1, room.w));
			}

			if (triangles.Length <= 0)
			{
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

				Vector3 p0 = new Vector3(rooms[index0].x, 0, rooms[index0].y);
				Vector3 p1 = new Vector3(rooms[index1].x, 0, rooms[index1].y);
				Vector3 p2 = new Vector3(rooms[index2].x, 0, rooms[index2].y);

				Gizmos.DrawLine(p0, p1);
				Gizmos.DrawLine(p1, p2);
				Gizmos.DrawLine(p2, p0);
			}

			if (mstLines == null || mstLines.Count == 0)
			{
				return;
			}
			
			Gizmos.color = Color.blue;
			for (int i = 0; i < mstLines.Count; i += 2)
			{
				int indexA = mstLines[i];
				int indexB = mstLines[i + 1];

				if (indexA >= rooms.Length || indexB >= rooms.Length)
				{
					continue;
				}

				Vector3 pA = new Vector3(rooms[indexA].x, 0, rooms[indexA].y);
				Vector3 pB = new Vector3(rooms[indexB].x, 0, rooms[indexB].y);

				Gizmos.DrawLine(pA, pB);
			}
		}
	}
}
