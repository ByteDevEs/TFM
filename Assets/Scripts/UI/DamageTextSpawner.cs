using Mirror;
using UnityEngine;
namespace UI
{
	public class DamageTextSpawner : NetworkBehaviour
	{
		public GameObject damageTextPrefab;

		public void SpawnDamageText(int amount, Vector3 position, Transform parent)
		{
			GameObject obj = Instantiate(damageTextPrefab, parent);
			obj.transform.position = position;
			obj.GetComponent<DamageText>().Set(amount);
		}
	}
}
