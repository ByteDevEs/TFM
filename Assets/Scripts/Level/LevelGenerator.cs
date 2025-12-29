using System.Collections.Generic;
using Mirror;
using UnityEngine;
namespace Level
{
	public class LevelGenerator : MonoBehaviour
	{
		public LevelGrid[] LevelGrids;
		public int LastLevel = 100;

		readonly Dictionary<int, LevelGrid> generatedLevelGrids = new Dictionary<int, LevelGrid>();

		public LevelGrid GetOrAddLevel(int levelNumber)
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[LevelGenerator] Client tried to generate a level. Ignored.");
				return null;
			}

			if (levelNumber < 1) return null;

			if (generatedLevelGrids.TryGetValue(levelNumber, out LevelGrid existingGrid))
			{
				if (existingGrid != null) return existingGrid;

				generatedLevelGrids.Remove(levelNumber);
			}

			print($"Generating Level {levelNumber}...");

			int levelGridCount = LevelGrids.Length;

			float safeLastLevel = LastLevel > 0 ? LastLevel : 1f;
			float ratio = levelGridCount / safeLastLevel;

			int index = Mathf.FloorToInt((levelNumber - 1) * ratio);
			index = Mathf.Clamp(index, 0, levelGridCount - 1);

			GameObject level = Instantiate(
				LevelGrids[index].gameObject,
				transform.position + Vector3.up * levelNumber * 50f,
				transform.rotation
			);

			NetworkServer.Spawn(level);

			LevelGrid selectedLevelGrid = level.GetComponent<LevelGrid>();

			if (selectedLevelGrid != null)
			{
				selectedLevelGrid.Level = levelNumber;
				generatedLevelGrids.Add(levelNumber, selectedLevelGrid);

				if (selectedLevelGrid is DefaultLevelGrid defaultGrid)
				{
					defaultGrid.SrvGenerateLevelGrid();
					defaultGrid.SrvGenerateMst();
					defaultGrid.SrvGenerateCells();
					defaultGrid.SrvGenerateStartAndExit();
				}
			}
			else
			{
				Debug.LogError($"Prefab at index {index} is missing a LevelGrid component!");
			}

			return selectedLevelGrid;
		}
	}
}