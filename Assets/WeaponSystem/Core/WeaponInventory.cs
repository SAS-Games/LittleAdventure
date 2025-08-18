using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.WeaponSystem
{
    public class WeaponInventory : MonoBehaviour
    {
        public enum WeaponSlot
        {
            Primary,
            Secondary,
            Tertiary,
        }

        [System.Serializable]
        public class Weapondata
        {
            public WeaponSlot Slot;
            public WeaponDataSO[] Weapons;
        }


        [field: SerializeField] public List<Weapondata> WeaponData;
        private Dictionary<WeaponSlot, WeaponDataSO> _equippedWeaponData = new Dictionary<WeaponSlot, WeaponDataSO>();
        public event Action<int, WeaponDataSO> OnWeaponDataChanged;

        public bool TrySetWeapon(WeaponSlot slot, WeaponDataSO newData, out WeaponDataSO oldData)
        {
            oldData = null;

            var weaponData = WeaponData.Find(w => w.Slot == slot);
            if (weaponData == null || weaponData.Weapons == null || weaponData.Weapons.Length == 0)
                return false;

            // check if weapon is valid for this slot
            bool validWeapon = Array.Exists(weaponData.Weapons, w => w == newData);
            if (!validWeapon)
                return false;

            // get old data if already equipped
            _equippedWeaponData.TryGetValue(slot, out oldData);

            // equip new weapon
            _equippedWeaponData[slot] = newData;

            OnWeaponDataChanged?.Invoke((int)slot, newData);
            return true;
        }

        public bool TryGetWeapon(WeaponSlot index, out WeaponDataSO data)
        {
            data = null;
            var slot = index;
            return _equippedWeaponData.TryGetValue(slot, out data);
        }

        public bool TryGetEmptyIndex(out int index)
        {
            foreach (WeaponSlot slot in Enum.GetValues(typeof(WeaponSlot)))
            {
                if (!_equippedWeaponData.ContainsKey(slot))
                {
                    index = (int)slot;
                    return true;
                }
            }

            index = -1;
            return false;
        }
    }
}