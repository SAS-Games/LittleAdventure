using System;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SAS.ActionGraph.WeaponSystem
{
    public class ComboWeapon : MonoBehaviour, IWeapon, IActionGraphExecutionController
    {
        [SerializeField] private ActionGraphAsset comboGraphConfig;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform hitOrigin;
        [SerializeField] private bool registerInput = true;
        [SerializeField] private string attackInputKey = "Attack";
        [SerializeField] private bool forwardInputToFsmController;

        private readonly WeaponContext _context = new WeaponContext();
        private readonly ActionGraphExecutor _executor = new ActionGraphExecutor();
        private bool _inputRegistered;

        public string AttackInputKey => attackInputKey;
        public bool IsInUse => _executor.IsExecuting;
        public bool IsExecuting => _executor.IsExecuting;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInParent<Animator>();

            BuildGraph();
        }

        private void Start()
        {
            RegisterInput();
        }

        private void OnDisable()
        {
            CancelExecution();
        }

        private void OnDestroy()
        {
            CancelExecution();
        }

        public void Attack()
        {
            Enter();
        }

        public void SetAttackInput(bool isPressed)
        {
            if (!isPressed)
                return;

            RecordAttackInput();

            if (!IsInUse)
                StartGraph();
        }

        public void Enter()
        {
            SetAttackInput(true);
        }

        public void Exit()
        {
            CancelExecution();
        }

        public void CancelExecution()
        {
            _context.ResetCombo();
            _executor.CancelExecution();
        }

        private void BuildGraph()
        {
            PopulateContext();
            _executor.Build(comboGraphConfig, _context);
        }

        private void StartGraph()
        {
            if (!_executor.HasGraph)
                BuildGraph();

            if (!_executor.HasGraph)
            {
                Debug.LogWarning($"{nameof(ComboWeapon)} has no combo graph.", this);
                return;
            }

            _context.MarkAttackInputAccepted(Time.time);
            _ = ExecuteGraphAsync();
        }

        private async Task ExecuteGraphAsync()
        {
            PopulateContext();
            _context.ResetCombo();

            try
            {
                await _executor.ExecuteAsync(_context);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                _context.EndAttack();
            }
        }

        private void RecordAttackInput()
        {
            _context.RecordAttackInput(Time.time);
        }

        private void PopulateContext()
        {
            _context.Owner = gameObject;
            _context.Animator = animator;
            _context.HitOrigin = hitOrigin != null ? hitOrigin : transform;
            _context.FirePoint = transform;
        }

        private void RegisterInput()
        {
            if (_inputRegistered || !registerInput)
                return;

            InputHandler inputHandler = GetComponentInParent<InputHandler>();
            if (inputHandler == null)
                return;

            if (string.IsNullOrEmpty(attackInputKey))
                return;

            FSMCharacterController fsmController =
                forwardInputToFsmController ? GetComponentInParent<FSMCharacterController>() : null;
            if (inputHandler.TryGetInputCommand(attackInputKey, out IInputCommand existingCommand))
            {
                if (existingCommand is ChainedInputCommand chainedCommand)
                {
                    ComboWeaponInputCommand.AddWeaponHandlers(chainedCommand, this, fsmController);
                    _inputRegistered = true;
                    return;
                }

                Debug.LogWarning(
                    $"{nameof(ComboWeapon)} found existing input command '{attackInputKey}', but it is not a {nameof(ChainedInputCommand)}.",
                    this);
                return;
            }

            inputHandler.RegisterInputCommand(attackInputKey,
                new ComboWeaponInputCommand(attackInputKey, this, fsmController), true);
            _inputRegistered = true;
        }

    }

    public class ComboWeaponInputCommand : ChainedInputCommand
    {
        private const int WeaponInputPriority = 100;

        protected override string InputActionName { get; }

        public ComboWeaponInputCommand(string inputActionKey, ComboWeapon weapon, FSMCharacterController fsmController)
        {
            InputActionName = inputActionKey;
            AddWeaponHandlers(this, weapon, fsmController);
        }

        public static void AddWeaponHandlers(ChainedInputCommand command, ComboWeapon weapon,
            FSMCharacterController fsmController)
        {
            if (command == null || weapon == null)
                return;

            command.AddHandler(InputActionPhase.Performed, new ConditionalInputHandler(() => true, _ =>
            {
                weapon.SetAttackInput(true);
                fsmController?.OnFire();
            }), WeaponInputPriority);

            command.AddHandler(InputActionPhase.Canceled,
                new ConditionalInputHandler(() => true, _ => { fsmController?.OnFireCanceled(); }),
                WeaponInputPriority);
        }
    }
}
