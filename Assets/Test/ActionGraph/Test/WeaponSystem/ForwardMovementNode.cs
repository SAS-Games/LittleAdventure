using System;
using System.Threading;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
    [Serializable]
    public class WeaponForwardMovementData
    {
        [Tooltip("Forward speed applied while this node is active.")]
        public float velocity = 5f;

        [Tooltip("How long this node contributes movement velocity.")]
        public float duration = 0.05f;

        [Tooltip("How this velocity combines with character movement.")]
        public MovementVelocityContributionMode velocityContributionMode = MovementVelocityContributionMode.OverrideHorizontal;

        [Tooltip("When multiple override contributions are active, the highest priority wins.")]
        public int velocityContributionPriority = 100;
    }

    [NodeBinding(typeof(ForwardMovementNode))]
    [Serializable]
    public class WeaponForwardMovementProvider : ActionDataProvider<WeaponForwardMovementData>, IIndexedActionDataProvider
    {
    }

    [ActionNodeMenu("Weapon/Forward Movement")]
    public class ForwardMovementNode : ActionNode<WeaponForwardMovementData>
    {
        public ForwardMovementNode(ActionDataProvider<WeaponForwardMovementData> dataProvider) : base(dataProvider)
        {
        }

        public override async Task ExecuteAsync(ActionContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var weaponContext = WeaponNodeUtility.RequireWeaponContext(context);
            var data = WeaponNodeUtility.GetAttackData(_dataProvider, weaponContext) ?? new WeaponForwardMovementData();

            float velocity = data.velocity;
            float duration = data.duration;

            if (duration <= 0f || Mathf.Approximately(velocity, 0f) || weaponContext.Owner == null)
            {
                return;
            }

            IMovementVelocityComposer movementVelocityComposer = WeaponNodeUtility.GetMovementVelocityComposer(weaponContext);
            IMovementVectorHandler movementVectorHandler = movementVelocityComposer == null ? WeaponNodeUtility.GetMovementVectorHandler(weaponContext) : null;

            if (movementVelocityComposer == null && movementVectorHandler == null)
            {
                Debug.LogWarning("No movement composer or movement vector handler was found on the weapon owner or parents.");
                return;
            }

            Component movementTargetComponent = WeaponNodeUtility.GetMovementTargetComponent(movementVelocityComposer, movementVectorHandler);
            Vector3 direction = WeaponNodeUtility.GetMovementForward(weaponContext, movementTargetComponent).normalized;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    token.ThrowIfCancellationRequested();

                    Vector3 movementVelocity = velocity * direction;
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
    }
}