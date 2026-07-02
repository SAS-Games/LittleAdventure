using SAS.WeaponSystem.Components;

public class MovementData : ComponentData<MovementAttackData>
{
    protected override void SetComponentDependency()
    {
        ComponentDependency = typeof(Movement);
    }
}