using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers
{
	[RequireComponent(typeof(NetworkIdentity), typeof(PlayerInput), typeof(MovementController))]
	[RequireComponent(typeof(AttackController))]
	public class PlayerController : NetworkBehaviour
	{
		PlayerInput playerInput;
		CameraController cameraController;
		MovementController movementController;
		AttackController attackController;

		void Start()
		{
			if (!isLocalPlayer)
			{
				return;
			}

			playerInput = GetComponent<PlayerInput>();
			cameraController = Instantiate(Prefabs.GetInstance().cameraPrefab);
			movementController = GetComponent<MovementController>();
			attackController = GetComponent<AttackController>();
			attackController.SwapWeapons(Prefabs.GetInstance().weaponPool[Random.Range(0, Prefabs.GetInstance().weaponPool.Count)]);
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

			if (!attackController.TryAttack(ray))
			{
				if (!attackController.isAttackingTarget)
				{
					movementController.Move(ray);
				}
			}

			attackController.SwapWeapons(ray);
		}
	}
}
