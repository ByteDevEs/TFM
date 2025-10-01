using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkIdentity), typeof(PlayerInput), typeof(MovementController))]
public class PlayerController : NetworkBehaviour
{
	private PlayerInput playerInput;
	private CameraController cameraController;
	private MovementController movementController;

	[SyncVar]
	private CharacterStats stats;

	public MovementController GetMovementController() => movementController;

	void Awake()
	{
		if (!isLocalPlayer)
		{
			return;
		}
		
		stats = new CharacterStats();
		playerInput = GetComponent<PlayerInput>();
		cameraController = Instantiate(Prefabs.GetInstance().CameraPrefab);
		movementController = GetComponent<MovementController>();
	}

    void Update()
    {
		if (!isLocalPlayer)
		{
			return;
		}
		
		cameraController.SetPosition(transform.position + Settings.GetInstance().GetCameraOffset());
    }
}