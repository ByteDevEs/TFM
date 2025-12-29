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
		if (!instance)
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
			o.GetComponent<AudioSource>().maxDistance = 5.0f;
		}
		else
		{
			DontDestroyOnLoad(o);
		}
		
		o.GetComponent<AudioSource>().pitch = Random.Range(0.95f, 1.05f);
		o.GetComponent<AudioSource>().volume = Settings.GetInstance().SfxVolume / 100.0f;
		o.GetComponent<AudioSource>().Play();
		
		StartCoroutine(DestroySoundGameObject(o.GetComponent<AudioSource>(), o.GetComponent<AudioSource>().clip.length + 1, true));
	}

	void PlayLoopMusic()
	{
		AudioClip sound = MusicClips[Random.Range(0, MusicClips.Count)];
		
		GameObject o = new GameObject(sound.name);
		o.AddComponent<AudioSource>().clip = sound;
		o.GetComponent<AudioSource>().spatialBlend = 0.0f;
		DontDestroyOnLoad(o);
		
		o.GetComponent<AudioSource>().volume = 0.025f * (Settings.GetInstance().MusicVolume / 100.0f);
		o.GetComponent<AudioSource>().Play();
		
		StartCoroutine(DestroySoundGameObject(o.GetComponent<AudioSource>(), o.GetComponent<AudioSource>().clip.length + 1, false));
		Invoke(nameof(PlayLoopMusic), o.GetComponent<AudioSource>().clip.length);
	}
	
	IEnumerator DestroySoundGameObject(AudioSource @as, float clipLength, bool isSfx)
	{
		while (@as.volume < clipLength)
		{
			@as.volume = isSfx
				? Settings.GetInstance().SfxVolume / 100.0f
				: 0.025f * Settings.GetInstance().MusicVolume / 100.0f;
			yield return new WaitForSeconds(0.01f);
		}
		Destroy(@as);
	}
}
