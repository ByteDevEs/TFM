#if UNITY_EDITOR
using Helpers;
#endif
using UnityEngine;
namespace Weapons
{
	[CreateAssetMenu(fileName = "WeaponScriptable", menuName = "Scriptable Objects/WeaponScriptable")]
	public class WeaponScriptable : ScriptableObject
	{
		[SerializeField] public new string name;
		[SerializeField] public float baseDamage;
		[SerializeField] public float baseRange;
		[SerializeField] public float baseCooldown;
		[SerializeField] public AttackType attackType;

		[SerializeField]
		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Melee)]
		#endif
		public float attackDegree;
		[SerializeField]
		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Ranged)]
		#endif
		public float attackWidth;
		[SerializeField]
		#if UNITY_EDITOR
		[ShowIf("attackType", (int)AttackType.Area)]
		#endif
		public float areaDiameter;
	}

	public enum AttackType
	{
		Melee,
		Area,
		Ranged
	}
}
