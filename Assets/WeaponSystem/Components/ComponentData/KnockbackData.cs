using SAS.StateMachineGraph.Utilities;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class KnockbackData : ComponentData<AttackKnockback>
    {
        protected override void SetComponentDependency()
        {
            ComponentDependency = typeof(Knockback);
        }
    }
}