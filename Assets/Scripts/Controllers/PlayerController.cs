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

		CameraController cameraController;
		MovementController movementController;
		AttackController attackController;
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
			attackController = GetComponent<AttackController>();
			HealthController = GetComponent<HealthController>();
			attackController.SwapWeapons(Prefabs.GetInstance().WeaponPool[Random.Range(0, Prefabs.GetInstance().WeaponPool.Count)]);
		
			UIDocumentController.GetInstance().OpenGameMenu();
		}

		void Update()
		{
			if (!isLocalPlayer)
			{
				return;
			}

			if (Keyboard.current.fKey.wasPressedThisFrame)
			{
				attackController.Stats.LevelUpProperty(nameof(CharacterStats.Strength));
			}

			Vector2 mousePos = Mouse.current.position.ReadValue();
			cameraController.SetPosition(mousePos, transform.position);

			Ray ray = cameraController.Camera.ScreenPointToRay(mousePos);

			if (attackController && !attackController.TryAttack(ray))
			{
				if (!attackController.IsAttackingTarget)
				{
					movementController.Move(ray);
				}
			}

			attackController.SwapWeapons(ray);
		}
	}
}
