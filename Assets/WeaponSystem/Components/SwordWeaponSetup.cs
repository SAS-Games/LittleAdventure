using SAS.WeaponSystem.Components;
using UnityEngine;

namespace SAS.WeaponSystem
{
    public class SwordWeaponSetup : WeaponComponent<SwordWeaponSetupComponenetData, EmptyAttackData>
    {
        private GameObject _leftWeaponInstance;
        private GameObject _rightWeaponInstance;

        public override void Init()
        {
            base.Init();
            AttachWeapons(transform.root);
        }


        /// <summary>
        /// Attaches left and right weapons to their respective sockets.
        /// </summary>
        private void AttachWeapons(Transform root)
        {
            if (Data.LeftWeapon != null)
                _leftWeaponInstance = AttachWeapon(Data.LeftWeapon, FindByFullPath(root, Data.LeftSocketPath));

            if (Data.RightWeapon != null)
                _rightWeaponInstance = AttachWeapon(Data.RightWeapon, FindByFullPath(root, Data.RightSocketPath));
        }

        private Transform FindByFullPath(Transform root, string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return null;

            // Strip root name prefix if included
            if (fullPath.StartsWith(root.name + "/"))
                fullPath = fullPath.Substring(root.name.Length + 1);

            var socket = root.Find(fullPath);
            if (socket == null)
                Debug.LogWarning($"[SwordWeaponSetup] Socket not found: {fullPath} under {root.name}");

            return socket;
        }

        private GameObject AttachWeapon(GameObject weaponPrefab, Transform socket)
        {
            if (weaponPrefab == null || socket == null)
                return null;

            var weaponInstance = Object.Instantiate(weaponPrefab, socket, false);

            // Reset local transform
            weaponInstance.transform.localPosition = Vector3.zero;
            weaponInstance.transform.localRotation = Quaternion.identity;
            weaponInstance.transform.localScale = Vector3.one;

            return weaponInstance;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_leftWeaponInstance != null)
                Destroy(_leftWeaponInstance);

            if (_rightWeaponInstance != null)
                Destroy(_rightWeaponInstance);
        }
    }
}
