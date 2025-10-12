using Controllers;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
    static Prefabs _instance;
    public static Prefabs GetInstance() => _instance;

    public CameraController cameraPrefab;

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
