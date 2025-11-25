using Level;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(DefaultLevelGrid))]
	public class LevelGridEditor : UnityEditor.Editor
	{
		DefaultLevelGrid targetGrid;

		void OnEnable()
		{
			targetGrid = (DefaultLevelGrid)target;
		}

		void OnSceneGUI()
		{
			if (targetGrid.rooms == null || targetGrid.rooms.Length == 0) return;

			GUIStyle style = new GUIStyle
			{
				normal =
				{
					textColor = Color.yellow
				},
				fontSize = 20
			};

			for (int i = 0; i < targetGrid.rooms.Length; i++)
			{
				int4 room = targetGrid.rooms[i];

				float centerX = room.x * targetGrid.roomSize + room.z / 2f;
				float centerZ = room.y * targetGrid.roomSize + room.w / 2f;
				Vector3 worldPosition = new Vector3(centerX, 0.5f, centerZ);

				Handles.Label(worldPosition, i.ToString(), style);
			}
			
			float scenterX = targetGrid.startPosition.x * targetGrid.roomSize;
			float scenterZ = targetGrid.startPosition.y * targetGrid.roomSize;
			Vector3 sworldPosition = new Vector3(scenterX, 0.5f, scenterZ);

			Handles.Label(sworldPosition, targetGrid.startPosition.z.ToString(), style);
			
			float ecenterX = targetGrid.exitPosition.x * targetGrid.roomSize;
			float ecenterZ = targetGrid.exitPosition.y * targetGrid.roomSize;
			Vector3 eworldPosition = new Vector3(ecenterX, 0.5f, ecenterZ);

			Handles.Label(eworldPosition, targetGrid.exitPosition.z.ToString(), style);

			SceneView.RepaintAll();
		}
	}
}