using System;
using System.Threading;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class WeaponMovementData
{
    [Tooltip("Direction in the movement target's local space.")]
    public Vector3 localDirection = Vector3.forward;

    [Tooltip("Movement speed applied while this node is active.")]
    public float speed = 4f;

    [Tooltip("How long this node contributes movement velocity.")]
    public float duration = 0.15f;

    [Tooltip("How this velocity combines with character movement.")]
    public MovementVelocityContributionMode velocityContributionMode = MovementVelocityContributionMode.OverrideHorizontal;

    [Tooltip("When multiple override contributions are active, the highest priority wins.")]
    public int velocityContributionPriority = 100;
}

[NodeBinding(typeof(MovementNode))]
[Serializable]
public class WeaponMovementProvider : ActionDataProvider<WeaponMovementData>, IIndexedActionDataProvider
{
}

[ActionNodeMenu("Weapon/Movement")]
public class MovementNode : ActionNode<WeaponMovementData>
{
    public MovementNode(ActionDataProvider<WeaponMovementData> dataProvider) : base(dataProvider)
    {
    }

    public override async Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        var data = WeaponNodeUtility.GetAttackData(_dataProvider, weaponContext);

        if (data == null || data.duration <= 0f || data.speed == 0f || weaponContext.Owner == null)
        {
            return;
        }

        var movementVelocityComposer = WeaponNodeUtility.GetMovementVelocityComposer(weaponContext);
        var movementVectorHandler = movementVelocityComposer == null
            ? WeaponNodeUtility.GetMovementVectorHandler(weaponContext)
            : null;

        if (movementVelocityComposer == null && movementVectorHandler == null)
        {
            Debug.LogWarning("[WeaponMovement] No movement composer or movement vector handler was found on the weapon owner or parents.");
            return;
        }

        Component movementTargetComponent = WeaponNodeUtility.GetMovementTargetComponent(movementVelocityComposer, movementVectorHandler);
        Transform origin = movementTargetComponent != null
            ? movementTargetComponent.transform
            : weaponContext.OriginTransform != null ? weaponContext.OriginTransform : weaponContext.Owner.transform;

        Vector3 direction = origin.TransformDirection(data.localDirection);
        if (direction.sqrMagnitude <= 0f)
            direction = origin.forward;
        direction.Normalize();

        float elapsed = 0f;

        try
        {
            while (elapsed < data.duration)
            {
                token.ThrowIfCancellationRequested();

                Vector3 movementVelocity = direction * data.speed;
                if (movementVelocityComposer != null)
                    movementVelocityComposer.SetMovementVelocityContribution(this, movementVelocity, data.velocityContributionMode, data.velocityContributionPriority);
                else
                    movementVectorHandler.MovementVector = movementVelocity;

                elapsed += Time.deltaTime;

                await Awaitable.NextFrameAsync();
            }
        }
        finally
        {
            if (movementVelocityComposer != null)
                movementVelocityComposer.ClearMovementVelocityContribution(this);
            else
                movementVectorHandler.MovementVector = Vector3.zero;
        }
    }

    private static WeaponContext RequireWeaponContext(ActionContext context)
    {
        var weaponContext = context as WeaponContext;
        if (weaponContext == null)
            throw new InvalidOperationException("Movement node requires WeaponContext.");

        return weaponContext;
    }
}
}


