using SAS.WeaponSystem.Components;
using UnityEngine;

public class AnimationActivatorData : ComponentData<EmptyAttackData>
{
    [field: SerializeField] public string ParamName { get; private set; }

    protected override void SetComponentDependency()
    {
        ComponentDependency = typeof(AnimationActivator);
    }
}
