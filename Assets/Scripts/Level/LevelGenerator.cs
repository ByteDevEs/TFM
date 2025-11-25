using System;
using UnityEngine;
namespace Level
{
	public class LevelGenerator : MonoBehaviour
	{
		public LevelGrid[] levelGrids;

		public int lastLevel = 100;

		GameObject lastGenerated;

		void GenerateLevel(int levelNumber)
		{
			int levelGridCount = levelGrids.Length;

			double ratio = (double)levelGridCount / lastLevel;
			int index = (int)Math.Floor((levelNumber - 1) * ratio);
			index = Math.Min(index, levelGridCount - 1);

			if (lastGenerated)
			{
				Destroy(lastGenerated);
			}

			lastGenerated = Instantiate(levelGrids[index].gameObject);
			LevelGrid selectedLevelGrid = lastGenerated.GetComponent<LevelGrid>();

			if (selectedLevelGrid is DefaultLevelGrid defaultGrid)
			{
				defaultGrid.GenerateLevelGrid();
			}
			else
			{
				selectedLevelGrid.GenerateLevelGrid();
			}
		}

		void GenerateMst()
		{
			LevelGrid selectedLevelGrid = lastGenerated.GetComponent<LevelGrid>();

			if (selectedLevelGrid is DefaultLevelGrid defaultGrid)
			{
				defaultGrid.GenerateMst();
			}
			else
			{
				selectedLevelGrid.GenerateMst();
			}
		}
		
		void GenerateCells()
		{
			LevelGrid selectedLevelGrid = lastGenerated.GetComponent<LevelGrid>();

			if (selectedLevelGrid is DefaultLevelGrid defaultGrid)
			{
				defaultGrid.GenerateCells();
			}
			else
			{
				selectedLevelGrid.GenerateCells();
			}
		}
		
		void GenerateStartAndExit()
		{
			LevelGrid selectedLevelGrid = lastGenerated.GetComponent<LevelGrid>();

			if (selectedLevelGrid is DefaultLevelGrid defaultGrid)
			{
				defaultGrid.GenerateStartAndExit();
			}
			else
			{
				selectedLevelGrid.GenerateStartAndExit();
			}
		}
		
		void GenerateMesh()
		{
			LevelGrid selectedLevelGrid = lastGenerated.GetComponent<LevelGrid>();

			if (selectedLevelGrid is DefaultLevelGrid defaultGrid)
			{
				defaultGrid.GenerateMesh();
			}
			else
			{
				selectedLevelGrid.GenerateMesh();
			}
		}

		void OnGUI()
		{
			if (GUI.Button(new Rect(10, 10, 100, 20), "Generate Level"))
			{
				GenerateLevel(1);
			}

			if (GUI.Button(new Rect(10, 50, 100, 20), "Generate MST"))
			{
				GenerateMst();
			}
			
			if (GUI.Button(new Rect(10, 90, 100, 20), "Generate Cells"))
			{
				GenerateCells();
			}
			
			if (GUI.Button(new Rect(10, 130, 100, 20), "Generate Start and Exit"))
			{
				GenerateStartAndExit();
			}

			if (GUI.Button(new Rect(10, 170, 100, 20), "Generate Mesh"))
			{
				GenerateMesh();
			}
		}
	}
}
