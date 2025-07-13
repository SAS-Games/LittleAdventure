namespace SAS.WeaponSystem.Components
{
    public class AttackBlock : ComponentData<BlockData>
    {
        protected override void SetComponentDependency()
        {
            ComponentDependency = typeof(Block);
        }
    }
}
