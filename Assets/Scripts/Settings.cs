using UnityEngine;

public class Settings : MonoBehaviour
{
	static Settings instance;
	public static Settings GetInstance() => instance;

	[Header("Camera")]
	public float CameraDistance = 10f;
	public Vector3 CameraOffset => (Vector3.left + Vector3.up * Mathf.Sqrt(2) + Vector3.back) * CameraDistance;
	public float MusicVolume { get; set; }
	public float SfxVolume { get; set; }

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
		
		SfxVolume = PlayerPrefs.GetFloat("SFXVolume", 100.0f);
		MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 100.0f);
	}
}
