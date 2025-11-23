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

				float centerX = room.x + room.z / 2f;
				float centerZ = room.y + room.w / 2f;
				Vector3 worldPosition = new Vector3(centerX, 0.5f, centerZ);

				Handles.Label(worldPosition, i.ToString(), style);
			}

			SceneView.RepaintAll();
		}
	}
}