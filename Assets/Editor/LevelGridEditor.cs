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
			if (targetGrid.Rooms == null || targetGrid.Rooms.Length == 0) return;

			GUIStyle style = new GUIStyle
			{
				normal =
				{
					textColor = Color.yellow
				},
				fontSize = 20
			};

			for (int i = 0; i < targetGrid.Rooms.Length; i++)
			{
				int4 room = targetGrid.Rooms[i];

				float centerX = room.x * targetGrid.RoomSize + room.z / 2f;
				float centerZ = room.y * targetGrid.RoomSize + room.w / 2f;
				Vector3 worldPosition = new Vector3(centerX, 0.5f, centerZ);

				Handles.Label(worldPosition, i.ToString(), style);
			}
			
			float scenterX = targetGrid.StartPosition.x * targetGrid.RoomSize;
			float scenterZ = targetGrid.StartPosition.y * targetGrid.RoomSize;
			Vector3 sworldPosition = new Vector3(scenterX, 0.5f, scenterZ);

			Handles.Label(sworldPosition, targetGrid.StartPosition.z.ToString(), style);
			
			float ecenterX = targetGrid.ExitPosition.x * targetGrid.RoomSize;
			float ecenterZ = targetGrid.ExitPosition.y * targetGrid.RoomSize;
			Vector3 eworldPosition = new Vector3(ecenterX, 0.5f, ecenterZ);

			Handles.Label(eworldPosition, targetGrid.ExitPosition.z.ToString(), style);

			SceneView.RepaintAll();
		}
	}
}