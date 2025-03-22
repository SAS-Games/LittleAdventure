using SAS.StateMachineCharacterController;
using UnityEngine;

public class PlayerKnockback : KnockbackBase
{
    private IMovementVectorHandler _movementVectorHandler;
    private void Start()
    {
        _movementVectorHandler = GetComponent<IMovementVectorHandler>();
    }
    protected override void ApplyKnockback(Vector3 movement)
    {
        _movementVectorHandler.MovementVector = movement;
    }
}
