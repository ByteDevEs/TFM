using Helpers;
using Mirror;
using UnityEngine;
namespace Weapons
{
    public class PhysicalWeapon : HoverableObject
    {
        WeaponScriptable weapon;

        public override void SetHoverEffect()
        {
            base.SetHoverEffect();
        }

        public override void SetClickEffect()
        {
            base.SetClickEffect();
        }

        public override void RemoveHoverEffect()
        {
            base.RemoveHoverEffect();
        }
        
        public WeaponScriptable Swap(WeaponScriptable weaponScriptable)
        {
            WeaponScriptable w = weapon;
            weapon = weaponScriptable;
            return w;
        }
    }
}
