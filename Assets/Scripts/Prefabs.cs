using System.Collections.Generic;
using Controllers;
using UnityEngine;
using Weapons;

public class Prefabs : MonoBehaviour
{
	static Prefabs instance;
	public static Prefabs GetInstance() => instance;

	public CameraController CameraPrefab;
	public GameObject PrototypeEnemy;
	public List<WeaponScriptable> WeaponPool;

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
