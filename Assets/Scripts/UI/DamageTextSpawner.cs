using Mirror;
using UnityEngine;
namespace UI
{
	public class DamageTextSpawner : NetworkBehaviour
	{
		public GameObject damageTextPrefab;

		public void SpawnDamageText(float amount, Vector3 position)
		{
			GameObject obj = Instantiate(damageTextPrefab, position, damageTextPrefab.transform.rotation);
			obj.GetComponent<DamageText>().Set(amount);
		}
	}
}
