using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class SwordWeaponSetupComponenetData : ComponentData<EmptyAttackData>
    {
        [field: SerializeField] public string LeftSocketPath { get; private set; }
        [field: SerializeField] public string RightSocketPath { get; private set; }
        [field: SerializeField] public GameObject LeftWeapon { get; private set; }
        [field: SerializeField] public GameObject RightWeapon { get; private set; }

        protected override void SetComponentDependency()
        {
            ComponentDependency = typeof(SwordWeaponSetup);
        }
    }
}
