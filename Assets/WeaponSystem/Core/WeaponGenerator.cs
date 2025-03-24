using SAS.Utilities.TagSystem;
using SAS.WeaponSystem.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAS.WeaponSystem
{
    [RequireComponent(typeof(Weapon))]
    public class WeaponGenerator : MonoBehaviour
    {
        public event Action OnWeaponGenerating;

        [FieldRequiresChild] private Weapon _weapon;
        [FieldRequiresParent] WeaponInventory _weaponInventory;
        private Animator _animator;

        private List<WeaponComponent> _componentAlreadyOnWeapon = new List<WeaponComponent>();
        private List<WeaponComponent> _componentsAddedToWeapon = new List<WeaponComponent>();
        private List<Type> _componentDependencies = new List<Type>();


        private void Start()
        {
            this.Initialize();
            _weaponInventory.OnWeaponDataChanged += HandleWeaponDataChanged;
            _animator = GetComponentInChildren<Animator>();
            if (_weaponInventory.TryGetWeapon(0, out var data))
                GenerateWeapon(data);
        }

        private void GenerateWeapon(WeaponDataSO data)
        {
            OnWeaponGenerating?.Invoke();

            _weapon.SetData(data);

            if (data is null)
            {
                _weapon.CanEnterAttack = false;
                return;
            }

            _componentAlreadyOnWeapon.Clear();
            _componentsAddedToWeapon.Clear();
            _componentDependencies.Clear();

            _componentAlreadyOnWeapon = GetComponents<WeaponComponent>().ToList();

            _componentDependencies = data.GetAllDependencies();

            foreach (var dependency in _componentDependencies)
            {
                if (_componentsAddedToWeapon.FirstOrDefault(component => component.GetType() == dependency))
                    continue;

                var weaponComponent =
                    _componentAlreadyOnWeapon.FirstOrDefault(component => component.GetType() == dependency);

                if (weaponComponent == null)
                {
                    weaponComponent = gameObject.AddComponent(dependency) as WeaponComponent;
                }

                weaponComponent.Init();

                _componentsAddedToWeapon.Add(weaponComponent);
            }

            var componentsToRemove = _componentAlreadyOnWeapon.Except(_componentsAddedToWeapon);

            foreach (var weaponComponent in componentsToRemove)
            {
                Destroy(weaponComponent);
            }
            if (_animator && data.AnimatorController)
                _animator.runtimeAnimatorController = data.AnimatorController;

            _weapon.CanEnterAttack = true;
        }

        private void HandleWeaponDataChanged(int inputIndex, WeaponDataSO data)
        {
            //if (inputIndex != (int)combatInput)
            //    return;

            GenerateWeapon(data);
        }

        private void OnDestroy()
        {
            if (_weaponInventory != null)
                _weaponInventory.OnWeaponDataChanged -= HandleWeaponDataChanged;
        }
    }
}