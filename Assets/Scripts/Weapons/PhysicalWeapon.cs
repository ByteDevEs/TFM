using System.Linq;
using Helpers;
using Mirror;
using UnityEngine;
namespace Weapons
{
    public class PhysicalWeapon : HoverableObject
    {
        [SyncVar(hook = nameof(OnWeaponIDChanged))] 
        public int WeaponID = -1;
        
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
        
        void OnWeaponIDChanged(int _, int newID)
        {
            if (currentObj)
            {
                Destroy(currentObj);
            }

            WeaponScriptable weaponData = WeaponLibrary.GetWeapon(newID);

            if (weaponData != null && weaponData.Prefab != null)
            {
                currentObj = Instantiate(weaponData.Prefab, transform);
            }
        }
        
        [Server]
        public void SetWeaponId(int weaponID)
        {
            WeaponID = weaponID;
        }
        
		[Server]
        public int Swap(int weaponID)
        {
            int w = WeaponID;
            WeaponID = weaponID;
            return w;
        }
    }
}
