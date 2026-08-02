using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SAS.ActionGraph.WeaponSystem
{
    public enum AttackSlot
    {
        Primary,
        Secondary
    }

    public interface IAttackInputReceiver
    {
        void SetAttackInput(bool isPressed);
    }

    public interface IAttackWeaponController : IWeapon, IActionGraphExecutionController
    {
        void HandleAttackInput(AttackSlot slot);
    }

    public sealed class WeaponAttackController : MonoBehaviour, IAttackWeaponController
    {
        [Tooltip("The close-range or default weapon used by Primary Attack.")]
        [SerializeField] private MonoBehaviour primaryWeapon;
        [Tooltip("Optional weapon used by Secondary Attack, such as a bow.")]
        [SerializeField] private MonoBehaviour secondaryWeapon;
        [SerializeField] private string primaryInputKey = "Attack";
        [SerializeField] private string secondaryInputKey = "SecondaryAttack";

        private IWeapon _primaryWeapon;
        private IWeapon _secondaryWeapon;
        private IWeapon _requestedWeapon;
        private IWeapon _activeWeapon;
        private FSMCharacterController _fsmController;
        private bool _inputRegistered;
        private bool _missingSecondaryWarningShown;

        public bool IsInUse => _activeWeapon != null && _activeWeapon.IsInUse;
        public bool IsExecuting => IsInUse;

        private void Awake()
        {
            _primaryWeapon = ResolveWeapon(primaryWeapon, nameof(primaryWeapon));
            _secondaryWeapon = ResolveWeapon(secondaryWeapon, nameof(secondaryWeapon));
            _fsmController = GetComponent<FSMCharacterController>();
        }

        private void Start()
        {
            RegisterInput();
        }

        private void OnDisable()
        {
            CancelExecution();
        }

        private void Update()
        {
            if (_fsmController == null || _fsmController.isActiveAndEnabled)
                return;

            if (_requestedWeapon == null && _activeWeapon == null)
                return;

            _fsmController.OnFireCanceled();
            CancelExecution();
        }

        public void HandleAttackInput(AttackSlot slot)
        {
            // Player input must always enter through an active FSM. AI attacks
            // are initiated by their state action calling Enter() directly.
            if (_fsmController == null || !_fsmController.isActiveAndEnabled)
                return;

            IWeapon selectedWeapon = GetWeapon(slot);
            if (selectedWeapon == null)
            {
                WarnAboutMissingWeapon(slot);
                return;
            }

            if (IsInUse)
            {
                if (ReferenceEquals(selectedWeapon, _activeWeapon) && selectedWeapon is IAttackInputReceiver inputReceiver)
                {
                    inputReceiver.SetAttackInput(true);
                }

                return;
            }

            _requestedWeapon = selectedWeapon;
            _fsmController.OnFire();
        }

        public void Enter()
        {
            if (_fsmController != null && !_fsmController.isActiveAndEnabled)
            {
                CancelExecution();
                return;
            }

            if (IsInUse)
                return;

            IWeapon weapon = _requestedWeapon ?? _primaryWeapon;
            _requestedWeapon = null;
            if (weapon == null)
            {
                Debug.LogWarning($"{nameof(WeaponAttackController)} has no primary weapon assigned.", this);
                return;
            }

            _activeWeapon = weapon;
            _activeWeapon.Enter();
        }

        public void Exit()
        {
            CancelExecution();
        }

        public void CancelExecution()
        {
            _requestedWeapon = null;

            if (_activeWeapon is IActionGraphExecutionController executionController)
                executionController.CancelExecution();
            else
                _activeWeapon?.Exit();

            _activeWeapon = null;
        }

        private IWeapon GetWeapon(AttackSlot slot)
        {
            return slot == AttackSlot.Primary ? _primaryWeapon : _secondaryWeapon;
        }

        private IWeapon ResolveWeapon(MonoBehaviour weaponBehaviour, string fieldName)
        {
            if (weaponBehaviour == null)
                return null;

            if (weaponBehaviour is IWeapon weapon)
                return weapon;

            Debug.LogError($"{nameof(WeaponAttackController)}.{fieldName} must implement {nameof(IWeapon)}.", this);
            return null;
        }

        private void RegisterInput()
        {
            if (_inputRegistered)
                return;

            InputHandler inputHandler = GetComponent<InputHandler>();
            if (inputHandler == null)
                return;

            RegisterInput(inputHandler, primaryInputKey, AttackSlot.Primary);
            RegisterInput(inputHandler, secondaryInputKey, AttackSlot.Secondary);
            _inputRegistered = true;
        }

        private void RegisterInput(InputHandler inputHandler, string inputKey, AttackSlot slot)
        {
            if (string.IsNullOrWhiteSpace(inputKey))
                return;

            if (inputHandler.TryGetInputCommand(inputKey, out IInputCommand existingCommand))
            {
                if (existingCommand is ChainedInputCommand chainedCommand)
                {
                    AddInputHandlers(chainedCommand, slot);
                    return;
                }

                Debug.LogWarning($"Input command '{inputKey}' already exists and is not a {nameof(ChainedInputCommand)}.", this);
                return;
            }

            inputHandler.RegisterInputCommand(inputKey, new WeaponAttackInputCommand(inputKey, this, slot), true);
        }

        private void AddInputHandlers(ChainedInputCommand command, AttackSlot slot)
        {
            command.AddHandler(InputActionPhase.Performed, new ConditionalInputHandler(CanProcessPlayerInput, _ => HandleAttackInput(slot)), 100);
            command.AddHandler(InputActionPhase.Canceled, new ConditionalInputHandler(CanProcessPlayerInput, _ => _fsmController.OnFireCanceled()), 100);
        }

        private bool CanProcessPlayerInput()
        {
            return _fsmController != null && _fsmController.isActiveAndEnabled;
        }

        private void WarnAboutMissingWeapon(AttackSlot slot)
        {
            if (slot == AttackSlot.Secondary && _missingSecondaryWarningShown)
                return;

            if (slot == AttackSlot.Secondary)
                _missingSecondaryWarningShown = true;

            Debug.LogWarning($"No weapon is assigned to the {slot} attack slot.", this);
        }

        private sealed class WeaponAttackInputCommand : ChainedInputCommand
        {
            protected override string InputActionName { get; }

            public WeaponAttackInputCommand(string inputActionKey, WeaponAttackController controller, AttackSlot slot)
            {
                InputActionName = inputActionKey;
                controller.AddInputHandlers(this, slot);
            }
        }
    }
}
