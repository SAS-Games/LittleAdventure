using SAS.WeaponSystem.Components;
using UnityEngine;

public class AnimationActivator : WeaponComponent<AnimationActivatorData, EmptyAttackData>
{
    private Animator _animator;
    public override void Init()
    {
        base.Init();
        _animator = GetComponentInParent<Animator>();

    }
    protected override void HandleEnter()
    {
        base.HandleEnter();
        _animator.SetBool(data.ParamName, true);

    }
}
