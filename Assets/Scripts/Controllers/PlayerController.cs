using System.Collections.Generic;
using System.Linq;
using Mirror;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Weapons;

namespace Controllers
{
	[RequireComponent(typeof(NetworkIdentity), typeof(PlayerInput), typeof(MovementController))]
	[RequireComponent(typeof(AttackController))]
	public class PlayerController : NetworkBehaviour
	{
		public static PlayerController LocalPlayer;

		public float ReviveTime = 3f;
		public float ReviveDistance = 5f;
		public GameObject BodyObject;
		public GameObject TombObject;

		[SyncVar] [HideInInspector] public PlayerController NearestPlayer;
		[SyncVar] public bool CanReviveNearPlayer;
		
		
		[SyncVar] public float ReviveTimer;
		[SyncVar(hook = nameof(PlayerDied))] public bool IsDead;
		
		CameraController cameraController;
		MovementController movementController;
		public AttackController AttackController { get; private set; }
		public HealthController HealthController { get; private set; }

		void Start()
		{
			movementController = GetComponent<MovementController>();
			AttackController = GetComponent<AttackController>();
			HealthController = GetComponent<HealthController>();

			if (isServer)
			{
				AttackController.SwapWeapons(WeaponLibrary.GetRandomWeaponID());
			}
			
			if (!isLocalPlayer)
			{
				return;
			}

			LocalPlayer = this;

			GetComponent<PlayerInput>();
			cameraController = Instantiate(Prefabs.GetInstance().CameraPrefab);
		
			UIDocumentController.GetInstance().OpenGameMenu();
		}
		
		void Update()
		{
			if (IsDead)
			{
				if (isServer)
				{
					AttackController.SrvStopAttacking();
				}
				
				return;
			}
			
			if (isServer)
			{
				ServerUpdateReviveStatus();
			}

			if (isLocalPlayer)
			{
				ClientHandleInput();
			}
		}

		[Server] 
		void ServerUpdateReviveStatus()
		{
			IEnumerable<PlayerController> players = FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
				.Except(new[]
				{
					this
				});
    
			CanReviveNearPlayer = players
				.Any(player => Vector3.Distance(transform.position, player.transform.position) < ReviveDistance
				               && player.GetComponent<PlayerController>() is { IsDead: true });
		}
		
		[Client] 
		void ClientHandleInput()
		{
			if (!isLocalPlayer || IsDead)
			{
				return;
			}

			Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

			UIDocument uiDocument = UIDocumentController.GetInstance().Document;
			VisualElement rootVisualElement = uiDocument.rootVisualElement;

			Vector2 panelLocalPos = RuntimePanelUtils.ScreenToPanel(
				rootVisualElement.panel, 
				mouseScreenPos
			);

			cameraController.SetPosition(mouseScreenPos, transform.position);

			VisualElement pickedElement = rootVisualElement.panel.Pick(panelLocalPos);

			if (pickedElement != null && pickedElement != rootVisualElement)
			{
				return; 
			}

			if (Keyboard.current.qKey.wasPressedThisFrame)
			{
				HealthController.TakePotion();
			}
			
			if (Keyboard.current.shiftKey.wasPressedThisFrame)
			{
				movementController.Dash();
			}

			Ray ray = cameraController.Camera.ScreenPointToRay(mouseScreenPos);

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
				CmdReviveTeammateClose(Time.unscaledDeltaTime);
			}
		}

		[Command]
		void CmdReviveTeammateClose(float delta)
		{
			IEnumerable<PlayerController> players = FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
				.Except(new[]
				{
					this
				});
			
			foreach (PlayerController player in players)
			{
				if (ReferenceEquals(player, this))
				{
					continue;
				}
      
				if (Vector3.Distance(transform.position, player.transform.position) < ReviveDistance
				    && player.GetComponent<PlayerController>() is { IsDead: true } pC)
				{
					NearestPlayer = pC;
					break;
				}
      
				NearestPlayer = null;
			}

			if (!NearestPlayer)
			{
				return;
			}
			
			NearestPlayer.ReviveTimer += delta;
			// Debug.Log("Reviving " + NearestPlayer.name + " who is dead " + NearestPlayer.IsDead + ": " + reviveTimer / ReviveTime);
			if (NearestPlayer.ReviveTimer > ReviveTime && NearestPlayer.IsDead)
			{
				NearestPlayer.ReviveTimer = 0f;
				NearestPlayer.IsDead = false;
				NearestPlayer.HealthController.SrvRevive();
			}
		}

		void PlayerDied(bool _, bool newValue)
		{
			TombObject.SetActive(newValue);
			BodyObject.SetActive(!newValue);
		}
	}
}
