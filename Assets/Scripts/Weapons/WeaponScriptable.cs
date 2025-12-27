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
		public string WeaponAttackSfx;
		public string WeaponAttackAnimationClip;

		#if UNITY_EDITOR
		[ShowIf(nameof(AttackType), (int)AttackType.Melee)]
		#endif
		public float AttackDegree;
		#if UNITY_EDITOR
		[ShowIf(nameof(AttackType), (int)AttackType.Ranged)]
		#endif
		public float AttackWidth;
		#if UNITY_EDITOR
		[ShowIf(nameof(AttackType), (int)AttackType.Ranged)]
		#endif
		public float TraverseSpeed;
		#if UNITY_EDITOR
		[ShowIf(nameof(AttackType), (int)AttackType.Ranged, (int)AttackType.Area)]
		#endif
		public float AreaDiameter;
		#if UNITY_EDITOR
		[ShowIf(nameof(AttackType), (int)AttackType.Area)]
		#endif
		public float CastTime;
		#if UNITY_EDITOR
		[ShowIf(nameof(AttackType), (int)AttackType.Area)]
		#endif
		public string HitSfx;
	}

	public enum AttackType
	{
		Melee,
		Area,
		Ranged
	}
}
