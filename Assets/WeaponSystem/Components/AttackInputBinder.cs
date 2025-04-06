using SAS.StateMachineCharacterController;
using SAS.WeaponSystem.Components;

namespace SAS.WeaponSystem
{
    public class AttackInputBinder : WeaponComponent<AttackInputComponentData, EmptyAttackData>
    {
        public override void Init()
        {
            base.Init();
            var fireCommand = new FireCommand(Data.AttackInputKey, GetComponentInParent<FSMCharacterController>());
            fireCommand.AddFirePerformedCallback(_ =>
            {
                _weapon.CurrentInput = true;
            });
            fireCommand.AddFireCanceledCallback(_ =>
            {
                _weapon.CurrentInput = false;
            });
            GetComponentInParent<InputHandler>().CreateInputCommand(Data.AttackInputKey, fireCommand, true);
        }
    }
}
