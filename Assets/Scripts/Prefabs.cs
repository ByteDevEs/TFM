using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Helpers;
using UnityEngine;
using Weapons;

public class Prefabs : MonoBehaviour
{
	static Prefabs instance;
	public static Prefabs GetInstance() => instance;

	public CameraController CameraPrefab;
	public GameObject PrototypeEnemy;
	public List<WeaponScriptable> WeaponPool;
	public GameObject PhysicalWeapon;
	public UDictionary<string, List<AudioClip>> Sounds;
	public List<AudioClip> MusicClips;

	void Awake()
	{
		if (instance is null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
			PlayLoopMusic();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void PlaySound(string soundName, Transform parent = null)
	{
		AudioClip sound = Sounds[soundName].ElementAt(Random.Range(0, Sounds[soundName].Count));

		GameObject o = new GameObject(sound.name);
		o.AddComponent<AudioSource>().clip = sound;
		o.GetComponent<AudioSource>().spatialBlend = 0.0f;
		if (parent != null)
		{
			o.transform.SetParent(parent);
			o.transform.localPosition = Vector3.zero;
			o.transform.localRotation = Quaternion.identity;
			o.GetComponent<AudioSource>().spatialize = true;
			o.GetComponent<AudioSource>().spatialBlend = 1.0f;
		}
		else
		{
			DontDestroyOnLoad(o);
		}
		
		o.GetComponent<AudioSource>().pitch = Random.Range(0.95f, 1.05f);
		o.GetComponent<AudioSource>().Play();
		
		StartCoroutine(DestroySoundGameObject(o, o.GetComponent<AudioSource>().clip.length + 1));
	}

	void PlayLoopMusic()
	{
		AudioClip sound = MusicClips[Random.Range(0, MusicClips.Count)];
		
		GameObject o = new GameObject(sound.name);
		o.AddComponent<AudioSource>().clip = sound;
		o.GetComponent<AudioSource>().spatialBlend = 0.0f;
		DontDestroyOnLoad(o);
		
		o.GetComponent<AudioSource>().volume = 0.1f;
		o.GetComponent<AudioSource>().Play();
		
		StartCoroutine(DestroySoundGameObject(o, o.GetComponent<AudioSource>().clip.length + 1));
		Invoke(nameof(PlayLoopMusic), o.GetComponent<AudioSource>().clip.length);
	}
	
	IEnumerator DestroySoundGameObject(GameObject o, float clipLength)
	{
		yield return new WaitForSeconds(clipLength);
		Destroy(o);
	}
}
