using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Controllers
{
	[RequireComponent(typeof(NetworkIdentity), typeof(PlayerInput), typeof(MovementController))]
	public class PlayerController : NetworkBehaviour
	{
		PlayerInput playerInput;
		CameraController cameraController;
		MovementController movementController;

		[SyncVar]
		CharacterStats stats;

		void Start()
		{
			if (!isLocalPlayer)
			{
				return;
			}
		
			stats = new CharacterStats();
			playerInput = GetComponent<PlayerInput>();
			cameraController = Instantiate(Prefabs.GetInstance().cameraPrefab);
			movementController = GetComponent<MovementController>();
		}

		void Update()
		{
			if (!isLocalPlayer)
			{
				return;
			}

			Vector2 mousePos = Mouse.current.position.ReadValue();

			cameraController.SetPosition(mousePos, transform.position);

			Ray ray = cameraController.Camera.ScreenPointToRay(Mouse.current.position.ReadValue());
			movementController.Move(ray);
		}
	}
}