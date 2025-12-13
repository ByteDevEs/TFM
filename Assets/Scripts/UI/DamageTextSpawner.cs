using Mirror;
using UnityEngine;
namespace UI
{
	public class DamageTextSpawner : NetworkBehaviour
	{
		public GameObject DamageTextPrefab;

		public void SpawnDamageText(float amount, Vector3 position)
		{
			GameObject obj = Instantiate(DamageTextPrefab, position, DamageTextPrefab.transform.rotation);
			obj.GetComponent<DamageText>().Set(amount);
		}
	}
}
