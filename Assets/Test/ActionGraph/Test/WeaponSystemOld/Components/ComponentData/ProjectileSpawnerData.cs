using SAS.Pool;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class ProjectileSpawnerData : ComponentData<AttackProjectileSpawner>
    {
        [field: SerializeField] public ComponentPoolSO<Poolable> ObjectPool { get; private set; }
        [field: SerializeField] public string AnimationEventName { get; private set; } = "OnAttackAction";

        protected override void SetComponentDependency()
        {
            ComponentDependency = typeof(ProjectileSpawner);
        }
    }
}