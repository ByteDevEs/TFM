using System;
using UnityEngine;

public class Settings : MonoBehaviour
{
    private static Settings instance;
    public static Settings GetInstance() => instance;

    public float CameraDistance = 10f;
    public Vector3 GetCameraOffset()
    {
        return (Vector3.left + Vector3.up * Mathf.Sqrt(2) + Vector3.back) * CameraDistance;
    }

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
