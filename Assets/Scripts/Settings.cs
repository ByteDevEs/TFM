using UnityEngine;

public class Settings : MonoBehaviour
{
	static Settings instance;
	public static Settings GetInstance() => instance;

	[Header("Camera")]
	public float CameraDistance = 10f;
	public Vector3 CameraOffset => (Vector3.left + Vector3.up * Mathf.Sqrt(2) + Vector3.back) * CameraDistance;

	[Header("Movement")]
	public LayerMask GroundLayerMask;

	void Awake()
	{
		if (!instance)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
}
