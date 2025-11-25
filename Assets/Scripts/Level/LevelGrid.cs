using Unity.AI.Navigation;
using UnityEngine;
namespace Level
{
	public abstract class LevelGrid : MonoBehaviour
	{
		public GameObject floorPrefab;
		public GameObject wallPrefab;
		public GameObject startPrefab;
		public GameObject exitPrefab;
		public float roomSize = 1;

		protected NavMeshSurface surface;

		void Start()
		{
			surface = GetComponent<NavMeshSurface>();
		}

		public abstract void GenerateLevelGrid();
		public abstract void GenerateMst();
		public abstract void GenerateCells();
		public abstract void GenerateStartAndExit();
		public abstract void GenerateMesh();
	}
}
