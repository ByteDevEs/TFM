using UnityEngine;
namespace Controllers
{
	public class CameraController : MonoBehaviour
	{
		public Camera Camera { get; private set; }

		void Awake()
		{
			Camera = GetComponent<Camera>();
		}

		public void SetPosition(Vector2 mousePos, Vector3 playerPosition)
		{
			Vector2 viewportPos = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);

			Vector2 relativeMousePos = viewportPos - Vector2.one * 0.5f;

			Vector3 localMouseOffset = new Vector3(relativeMousePos.x, relativeMousePos.y, 0f);

			Vector3 worldMouseOffset = transform.TransformDirection(localMouseOffset);

			Vector3 targetPosition = playerPosition + Settings.GetInstance().CameraOffset + worldMouseOffset;

			if (Vector3.Distance(transform.position, targetPosition) > 50f)
			{
				transform.position = targetPosition;
			}

			transform.position = Vector3.LerpUnclamped(transform.position, targetPosition, Time.deltaTime * 2.0f);
		}
	}
}
