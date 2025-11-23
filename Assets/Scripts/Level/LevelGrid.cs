using UnityEngine;
namespace Level
{
	public abstract class LevelGrid : MonoBehaviour
	{
		public abstract void GenerateLevelGrid();
		public abstract void GenerateMST();
	}
}
