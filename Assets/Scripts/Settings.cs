using UnityEngine;

public class Settings : MonoBehaviour
{
    static Settings _instance;
    public static Settings GetInstance() => _instance;

    [Header("Camera")]
    public float cameraDistance = 10f;
    public Vector3 CameraOffset => (Vector3.left + Vector3.up * Mathf.Sqrt(2) + Vector3.back) * cameraDistance;

    [Header("Movement")]
    public LayerMask groundLayerMask;

    void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
