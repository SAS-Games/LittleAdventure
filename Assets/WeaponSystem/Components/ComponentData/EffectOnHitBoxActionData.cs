using System;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class EffectOnHitBoxActionData : ComponentData<EmptyAttackData>
    {
        [field: SerializeField] public string EventName { get; private set; } = "PlaySlash";
        protected override void SetComponentDependency()
        {
            ComponentDependency = typeof(EffectOnHitBoxAction);
        }
    }
}