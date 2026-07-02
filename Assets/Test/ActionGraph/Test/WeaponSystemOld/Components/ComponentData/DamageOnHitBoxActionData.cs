using System;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class DamageOnHitBoxActionData : ComponentData<AttackDamage>
    {
        protected override void SetComponentDependency()
        {
            ComponentDependency = typeof(DamageOnHitBoxAction);
        }
    }
}