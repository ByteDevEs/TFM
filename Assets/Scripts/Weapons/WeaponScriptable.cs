#if UNITY_EDITOR
using Helpers;
#endif
using UnityEngine;
namespace Weapons
{
	[CreateAssetMenu(fileName = "WeaponScriptable", menuName = "Scriptable Objects/WeaponScriptable")]
	public class WeaponScriptable : ScriptableObject
	{
		public new string name;
		public GameObject prefab;
		public float baseDamage;
		public float baseRange;
		public float baseCooldown;
		public AttackType attackType;

		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Melee)]
		#endif
		public float attackDegree;
		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Ranged)]
		#endif
		public float attackWidth;
		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Ranged)]
		#endif
		public float traverseSpeed;
		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Ranged, (int)AttackType.Area)]
		#endif
		public float areaDiameter;
		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Area)]
		#endif
		public float castTime;
	}

	public enum AttackType
	{
		Melee,
		Area,
		Ranged
	}
}
