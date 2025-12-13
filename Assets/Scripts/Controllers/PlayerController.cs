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
		
		public PlayerInput PlayerInput { get; private set; }
		public CameraController CameraController { get; private set; }
		public MovementController MovementController { get; private set; }
		public AttackController AttackController { get; private set; }
		public HealthController HealthController { get; private set; }

		void Start()
		{
			if (!isLocalPlayer)
			{
				return;
			}

			LocalPlayer = this;

			PlayerInput = GetComponent<PlayerInput>();
			CameraController = Instantiate(Prefabs.GetInstance().cameraPrefab);
			MovementController = GetComponent<MovementController>();
			AttackController = GetComponent<AttackController>();
			HealthController = GetComponent<HealthController>();
			AttackController.SwapWeapons(Prefabs.GetInstance().weaponPool[Random.Range(0, Prefabs.GetInstance().weaponPool.Count)]);
		
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
				AttackController.Stats.LevelUpProperty(nameof(CharacterStats.Strength));
			}

			Vector2 mousePos = Mouse.current.position.ReadValue();
			CameraController.SetPosition(mousePos, transform.position);

			Ray ray = CameraController.Camera.ScreenPointToRay(mousePos);

			if (!AttackController.TryAttack(ray))
			{
				if (!AttackController.isAttackingTarget)
				{
					MovementController.Move(ray);
				}
			}

			AttackController.SwapWeapons(ray);
		}
	}
}
