using System.Linq;
using Helpers;
using Mirror;
using UnityEngine;
namespace Weapons
{
    public class PhysicalWeapon : HoverableObject
    {
        [SyncVar(hook = nameof(OnWeaponChanged))] public WeaponScriptable weapon;
        
        GameObject currentObj;

        public override void SetHoverEffect()
        {
            Children = transform.GetComponentsInChildren<Transform>().Select(t => t.gameObject).ToArray();
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
        
        void OnWeaponChanged(WeaponScriptable _, WeaponScriptable newValue)
        {
            if (currentObj)
            {
                Destroy(currentObj);
            }
            currentObj = Instantiate(newValue.Prefab, transform);
        }
        
		[Server]
        public void SetWeapon(WeaponScriptable weaponScriptable)
        {
            weapon = weaponScriptable;
        }
        
		[Server]
        public WeaponScriptable Swap(WeaponScriptable weaponScriptable)
        {
            WeaponScriptable w = weapon;
            weapon = weaponScriptable;
            return w;
        }
    }
}
