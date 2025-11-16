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
		}

		void Update()
		{
			if (!isLocalPlayer)
			{
				return;
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
