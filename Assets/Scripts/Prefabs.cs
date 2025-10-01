using JetBrains.Annotations;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
    private static Prefabs instance;
    public static Prefabs GetInstance() => instance;

    public CameraController CameraPrefab;

    void Awake()
    {
        if (instance is null)
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
