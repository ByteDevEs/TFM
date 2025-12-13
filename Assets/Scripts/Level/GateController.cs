using Lobby;
using Mirror;
using UnityEngine;
namespace Level
{
	public class GateController : MonoBehaviour
	{
		const float MaxCooldown = 0.5f;
		static float cooldown = 0.5f;

		public enum GateTypeEnum
		{
			Start,
			Exit,
			Door
		}
		
		public GateTypeEnum GateType;
		public int CurrentLevel;

		void FixedUpdate()
		{
			cooldown -= Time.fixedDeltaTime;
		}
		
		void OnTriggerEnter(Collider other)
		{
			if (!NetworkServer.active)
			{
				return;
			}
			
			if (other.CompareTag("Player") && cooldown <= 0)
			{
				if (GateType.Equals(GateTypeEnum.Start))
				{
					if (CurrentLevel == 0)
					{
						return;
					}
					(NetworkManager.singleton as GameManager)?.SrvMovePlayerToLevel(other.gameObject, CurrentLevel, -1);
					cooldown = MaxCooldown;
				}
				else if (GateType.Equals(GateTypeEnum.Exit))
				{
					(NetworkManager.singleton as GameManager)?.SrvMovePlayerToLevel(other.gameObject, CurrentLevel, 1);
					cooldown = MaxCooldown;
				}
			}
		}
	}
}
