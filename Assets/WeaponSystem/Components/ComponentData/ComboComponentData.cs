using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class ComboComponentData : ComponentData<ComboAttackData>
    {
        [field: SerializeField] public float InputDelay { get; private set; } = 0.25f;
        protected override void SetComponentDependency()
        {
            ComponentDependency = typeof(ComboAttack);
        }
    }
}