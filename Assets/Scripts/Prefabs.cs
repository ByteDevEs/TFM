using Controllers;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
    static Prefabs instance;
    public static Prefabs GetInstance() => instance;

    public CameraController cameraPrefab;

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
