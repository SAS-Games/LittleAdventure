using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class AttackInputComponentData : ComponentData<EmptyAttackData>
    {
        [field: SerializeField] public string AttackInputKey { get; private set; }
        protected override void SetComponentDependency()
        {
            ComponentDependency = typeof(AttackInputBinder);
        }
    }
}
