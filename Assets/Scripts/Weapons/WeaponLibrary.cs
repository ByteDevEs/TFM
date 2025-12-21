using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Weapons
{
	public static class WeaponLibrary
	{
		static Dictionary<int, WeaponScriptable> weapons;

		public static WeaponScriptable GetWeapon(int id)
		{
			weapons ??= Prefabs.GetInstance().WeaponPool.ToDictionary(e => e.WeaponID, e => e);

			return weapons != null && weapons.TryGetValue(id, out WeaponScriptable weapon) ? weapon : null;
		}
		public static int GetRandomWeaponID()
		{
			weapons ??= Prefabs.GetInstance().WeaponPool.ToDictionary(e => e.WeaponID, e => e);

			int r = Random.Range(0, weapons.Count);

			return weapons[r].WeaponID;
		}
	}
}
