using SAS.Core.TagSystem;
using UnityEngine;
using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;

[DefaultExecutionOrder(100)] // after controller movement
public class CharacterAnimationBridge : MonoBehaviour, IStateAction
{
    [SerializeField] CharacterAnimatorDriver animatorDriver;
    [SerializeField] FSMCharacterController controller;
    private string _animName;

    void Reset()
    {
        animatorDriver = GetComponent<CharacterAnimatorDriver>();
        controller = GetComponent<FSMCharacterController>();
    }

    void Update()
    {
        if (!animatorDriver || !controller)
            return;

        float speed = controller.NormalizedMoveInput;
        float verticalVelocity = controller.VerticalVelocity.y;
        bool grounded = controller.IsGrounded;

        animatorDriver.UpdateLocomotion(
            speed,
            verticalVelocity,
            grounded);
    }

    public void OnInitialize(Actor actor, Tag tag, string key)
    {
       _animName = key;
    }

    public void Execute(ActionExecuteEvent executeEvent)
    {
        animatorDriver.RequestAction(_animName);
    }
}