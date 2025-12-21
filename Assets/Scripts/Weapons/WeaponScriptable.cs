#if UNITY_EDITOR
using Helpers;
#endif
using UnityEngine;
namespace Weapons
{
	[CreateAssetMenu(fileName = "WeaponScriptable", menuName = "Scriptable Objects/WeaponScriptable")]
	public class WeaponScriptable : ScriptableObject
	{
		public int WeaponID;
		public string Name;
		public GameObject Prefab;
		public float BaseDamage;
		public float BaseRange;
		public float BaseCooldown;
		public AttackType AttackType;

		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Melee)]
		#endif
		public float AttackDegree;
		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Ranged)]
		#endif
		public float AttackWidth;
		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Ranged)]
		#endif
		public float TraverseSpeed;
		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Ranged, (int)AttackType.Area)]
		#endif
		public float AreaDiameter;
		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Area)]
		#endif
		public float CastTime;
	}

	public enum AttackType
	{
		Melee,
		Area,
		Ranged
	}
}
