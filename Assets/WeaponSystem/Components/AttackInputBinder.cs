using SAS.StateMachineCharacterController;
using SAS.WeaponSystem.Components;

namespace SAS.WeaponSystem
{
    public class AttackInputBinder : WeaponComponent<AttackInputComponentData, EmptyAttackData>
    {
        public override void Init()
        {
            base.Init();
            var fireCommand = new FireCommand(Data.AttackInputKey, _weapon, GetComponentInParent<FSMCharacterController>());
            GetComponentInParent<InputHandler>().RegisterInputCommand(Data.AttackInputKey, fireCommand, true);
        }
    }
}
