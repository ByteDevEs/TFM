using Mirror;
using UnityEngine;
namespace UI
{
	public class DamageTextSpawner : NetworkBehaviour
	{
		public GameObject DamageTextPrefab;

		[ClientRpc]
		public void RpcSpawnDamageText(float amount, Vector3 position)
		{
			SpawnDamageText(amount, position);
		}
		
		public void SpawnDamageText(float amount, Vector3 position)
		{
			GameObject obj = Instantiate(DamageTextPrefab, position, DamageTextPrefab.transform.rotation);
			obj.GetComponent<DamageText>().Set(amount);
		}
	}
}
