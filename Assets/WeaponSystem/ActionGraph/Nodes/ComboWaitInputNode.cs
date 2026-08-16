using System;
using System.Threading;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class ComboWaitInputData
{
    public int layer;
    public int comboCount;
    public float inputDelay = 0.1f;
    public float requiredAnimationProgress = 0.35f;
    public string stateTag = "Attack";
    public bool bufferEarlyInput = true;
}

[NodeBinding(typeof(ComboWaitInputNode))]
[Serializable]
public class ComboWaitInputProvider : ActionDataProvider<ComboWaitInputData>, IIndexedActionDataProvider
{
}

[ActionNodeMenu("Weapon/Combo Wait Input", "Waits for buffered attack input during the animation's combo window.")]
public class ComboWaitInputNode : WeaponActionNode<ComboWaitInputData>
{
    public ComboWaitInputNode(ActionDataProvider<ComboWaitInputData> dataProvider) : base(dataProvider)
    {
    }

    public override async Awaitable ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        var data = GetAttackData(weaponContext) ?? new ComboWaitInputData();

        float inputDelay = data.inputDelay;
        float requiredProgress = data.requiredAnimationProgress;
        string stateTag = data.stateTag;

        weaponContext.ComboInputAccepted = false;

        if (weaponContext.Animator == null)
            return;

        if (data.comboCount > 0 && weaponContext.CurrentAttackIndex >= data.comboCount - 1)
        {
            await WeaponNodeUtility.WaitForAnimationExitAsync(weaponContext, data.layer, data.stateTag, token);
            return;
        }

        int seenInputVersion = weaponContext.AttackInputVersion;
        bool enteredAttackState = false;
        bool hasBufferedInput = false;

        while (true)
        {
            token.ThrowIfCancellationRequested();

            AnimatorStateInfo stateInfo = weaponContext.Animator.GetCurrentAnimatorStateInfo(data.layer);
            bool inAttackState = stateInfo.IsTag(stateTag);

            if (inAttackState)
                enteredAttackState = true;
            else if (enteredAttackState)
                return;

            if (weaponContext.AttackInputVersion > seenInputVersion)
            {
                seenInputVersion = weaponContext.AttackInputVersion;
                float inputTime = weaponContext.LastAttackInputTime;
                if (inputTime - weaponContext.LastAcceptedAttackInputTime >= inputDelay)
                {
                    hasBufferedInput = true;
                    weaponContext.MarkAttackInputAccepted(inputTime);
                }
            }

            if (hasBufferedInput && inAttackState && (!data.bufferEarlyInput || stateInfo.normalizedTime >= requiredProgress))
            {
                weaponContext.ComboInputAccepted = true;
                return;
            }

            await Awaitable.NextFrameAsync(token);
        }
    }

}
}

