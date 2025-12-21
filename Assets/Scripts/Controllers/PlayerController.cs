using System.Collections.Generic;
using System.Linq;
using Lobby;
using Mirror;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers
{
	[RequireComponent(typeof(NetworkIdentity), typeof(PlayerInput), typeof(MovementController))]
	[RequireComponent(typeof(AttackController))]
	public class PlayerController : NetworkBehaviour
	{
		public static PlayerController LocalPlayer;

		public float ReviveTime = 3f;
		public float ReviveDistance = 5f;
		
		public PlayerController NearestPlayer { get; private set; }
		[SyncVar] public bool CanReviveNearPlayer;
		
		
		[SyncVar] float reviveTimer;
		[SyncVar] public bool IsDead;
		
		public float RevivalProgress => reviveTimer / ReviveTime;
		
		CameraController cameraController;
		MovementController movementController;
		public AttackController AttackController { get; private set; }
		public HealthController HealthController { get; private set; }

		void Start()
		{
			if (!isLocalPlayer)
			{
				return;
			}

			LocalPlayer = this;

			GetComponent<PlayerInput>();
			cameraController = Instantiate(Prefabs.GetInstance().CameraPrefab);
			movementController = GetComponent<MovementController>();
			AttackController = GetComponent<AttackController>();
			HealthController = GetComponent<HealthController>();
			AttackController.SwapWeapons(Prefabs.GetInstance().WeaponPool[Random.Range(0, Prefabs.GetInstance().WeaponPool.Count)]);
		
			UIDocumentController.GetInstance().OpenGameMenu();
		}
		
		void Update()
		{
			if (isServer)
			{
				ServerUpdateReviveStatus();
			}

			if (isLocalPlayer && !IsDead)
			{
				ClientHandleInput();
			}
		}

		[Server] 
		void ServerUpdateReviveStatus()
		{
			Dictionary<NetworkConnectionToClient, GameObject> players = ((GameManager)NetworkManager.singleton).Players;
    
			CanReviveNearPlayer = players
				.Where(player => player.Value && player.Value != gameObject)
				.Any(player => Vector3.Distance(transform.position, player.Value.transform.position) < ReviveDistance);
		}
		
		[Client] 
		void ClientHandleInput()
		{
			if (!isLocalPlayer || IsDead) return;
    
			
			if (Keyboard.current.qKey.wasPressedThisFrame)
			{
				HealthController.TakePotion();
			}
			
			if (Keyboard.current.shiftKey.wasPressedThisFrame)
			{
				movementController.Dash();
			}

			Vector2 mousePos = Mouse.current.position.ReadValue();
			cameraController.SetPosition(mousePos, transform.position);

			Ray ray = cameraController.Camera.ScreenPointToRay(mousePos);

			if (AttackController && !AttackController.TryAttack(ray))
			{
				if (!AttackController.IsAttackingTarget)
				{
					movementController.Move(ray);
				}
			}

			AttackController.SwapWeapons(ray);

			if (Keyboard.current.fKey.isPressed) 
			{
				CmdReviveTeammateClose();
			}
		}

		[Command]
		void CmdReviveTeammateClose()
		{
			var players = ((GameManager)NetworkManager.singleton).Players;

			foreach (KeyValuePair<NetworkConnectionToClient, GameObject> player in players)
			{
				if (!player.Value) 
				{
					continue;
				}

				if (ReferenceEquals(player.Value, gameObject))
				{
					continue;
				}
      
				if (Vector3.Distance(transform.position, player.Value.transform.position) < ReviveDistance
				    && player.Value.GetComponent<PlayerController>() is { IsDead: true } pC)
				{
					NearestPlayer = pC;
					break;
				}
      
				NearestPlayer = null;
			}

			if (NearestPlayer is null)
			{
				return;
			}

			NearestPlayer.CmdRevive();
		}
		
		[Command]
		void CmdRevive()
		{
			reviveTimer += Time.deltaTime;
			if (reviveTimer > ReviveTime)
			{
				reviveTimer = 0f;
				IsDead = false;
			}
		}
	}
}
