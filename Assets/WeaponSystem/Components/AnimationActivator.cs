using SAS.WeaponSystem.Components;

public class AnimationActivator : WeaponComponent<AnimationActivatorData, EmptyAttackData>
{
    protected override void HandleEnter()
    {
        base.HandleEnter();
        _weapon.Animator.SetBool(Data.ParamName, true);
    }

    protected override void HandleExit()
    {
        base.HandleExit();
        _weapon.Animator.SetBool(Data.ParamName, false);
    }
}
