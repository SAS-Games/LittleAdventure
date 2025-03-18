using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class ActionHitBoxData : ComponentData<AttackActionHitBox3D>
    {
        [field: SerializeField] public  LayerMask DetectableLayers { get; private set; }
        [field: SerializeField] public string AnimationEventName { get; private set; } = "OnAttackAction";

        protected override void SetComponentDependency()
        {
            ComponentDependency = typeof(ActionHitBox);
        }
    }
}