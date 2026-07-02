using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class ActionHitBoxData : ComponentData<AttackActionHitBox3D>
    {
        [field: SerializeField] public LayerMask DetectableLayers { get; private set; }
        protected override void SetComponentDependency()
        {
            ComponentDependency = typeof(ActionHitBox);
        }
    }
}