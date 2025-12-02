using System;
using Controllers;
using Lobby;
using Mirror;
using UnityEngine;
namespace Level
{
	public class GateController : MonoBehaviour
	{
		static float maxCooldown = 0.5f;
		static float cooldown = 0.5f;

		public enum GateType
		{
			Start,
			Exit,
			Door
		}
		
		public GateType gateType;
		public int currentLevel;

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
				if (gateType.Equals(GateType.Start))
				{
					if (currentLevel == 0)
					{
						return;
					}
					(NetworkManager.singleton as GameManager)?.SrvMovePlayerToLevel(other.gameObject, currentLevel, -1);
					cooldown = maxCooldown;
				}
				else if (gateType.Equals(GateType.Exit))
				{
					(NetworkManager.singleton as GameManager)?.SrvMovePlayerToLevel(other.gameObject, currentLevel, 1);
					cooldown = maxCooldown;
				}
			}
		}
	}
}
