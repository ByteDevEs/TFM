using Helpers;
using UnityEngine;
namespace Weapons
{
    public class PhysicalWeapon : HoverableObject
    {
        WeaponScriptable weapon;
        
        GameObject currentObj;

        public override void SetHoverEffect()
        {
            base.SetHoverEffect();
        }

        public override void SetClickEffect()
        {
            base.SetClickEffect();
        }

        public override void RemoveEffect()
        {
            base.RemoveEffect();
        }
        
        public WeaponScriptable Swap(WeaponScriptable weaponScriptable)
        {
            WeaponScriptable w = weapon;
            Destroy(currentObj);
            weapon = weaponScriptable;
            currentObj = Instantiate(weaponScriptable.Prefab, transform);
            return w;
        }
    }
}
