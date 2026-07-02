using System;
using System.Threading;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SAS.ActionGraph.WeaponSystem
{
public class ComboWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private ActionGraphAsset comboGraphConfig;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform hitOrigin;
    [SerializeField] private bool registerInput = true;
    [SerializeField] private string attackInputKey = "Attack";
    [SerializeField] private bool forwardInputToFsmController;

    private readonly WeaponContext _context = new WeaponContext();
    private ExecutionGraph _graph;
    private CancellationTokenSource _cts;
    private Task _currentExecution;
    private bool _inputRegistered;

    public string AttackInputKey => attackInputKey;
    public int CurrentAttackIndex => _context.CurrentAttackIndex;
    public bool IsInUse => _currentExecution != null && !_currentExecution.IsCompleted;

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
        CancelGraph();
    }

    private void OnDestroy()
    {
        CancelGraph();
    }

    public void SetGraph(ActionGraphAsset graphConfig)
    {
        comboGraphConfig = graphConfig;
        BuildGraph();
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
        CancelGraph();
    }

    public void SetCurrentAttackIndex(int index)
    {
        _context.CurrentAttackIndex = Mathf.Max(0, index);
    }

    public void ResetCombo()
    {
        _context.ResetCombo();
        _graph?.Reset();
    }

    public void NotifyAttackStepEnded()
    {
        _context.EndAttack();
    }

    public void BuildGraph()
    {
        if (comboGraphConfig == null || comboGraphConfig.root == null)
        {
            _graph = null;
            return;
        }

        PopulateContext();
        _graph = new ExecutionGraph(comboGraphConfig.root);
        _graph.Initialize(_context);
    }

    private void StartGraph()
    {
        if (_graph == null)
            BuildGraph();

        if (_graph == null)
        {
            Debug.LogWarning($"{nameof(ComboWeapon)} has no combo graph.", this);
            return;
        }

        _context.MarkAttackInputAccepted(Time.time);
        _ = ExecuteGraphAsync();
    }

    private async Task ExecuteGraphAsync()
    {
        CancelTokenOnly();

        _cts = new CancellationTokenSource();
        PopulateContext();
        _context.ResetCombo();

        try
        {
            _currentExecution = _graph.ExecuteAsync(_context, _cts.Token);
            await _currentExecution;
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
            _cts?.Dispose();
            _cts = null;
            _currentExecution = null;
        }
    }

    private void RecordAttackInput()
    {
        _context.RecordAttackInput(Time.time);
    }

    private void PopulateContext()
    {
        _context.Owner = gameObject;
        _context.ComboWeapon = this;
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

        FSMCharacterController fsmController = forwardInputToFsmController ? GetComponentInParent<FSMCharacterController>() : null;
        if (inputHandler.TryGetInputCommand(attackInputKey, out IInputCommand existingCommand))
        {
            if (existingCommand is ChainedInputCommand chainedCommand)
            {
                ComboWeaponInputCommand.AddWeaponHandlers(chainedCommand, this, fsmController);
                _inputRegistered = true;
                return;
            }

            Debug.LogWarning($"{nameof(ComboWeapon)} found existing input command '{attackInputKey}', but it is not a {nameof(ChainedInputCommand)}.", this);
            return;
        }

        inputHandler.RegisterInputCommand(attackInputKey, new ComboWeaponInputCommand(attackInputKey, this, fsmController), true);
        _inputRegistered = true;
    }

    private void CancelGraph()
    {
        CancelTokenOnly();
        _currentExecution = null;
        _context.ResetCombo();
    }

    private void CancelTokenOnly()
    {
        if (_cts == null)
            return;

        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
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

    public static void AddWeaponHandlers(ChainedInputCommand command, ComboWeapon weapon, FSMCharacterController fsmController)
    {
        if (command == null || weapon == null)
            return;

        command.AddHandler(InputActionPhase.Performed, new ConditionalInputHandler(() => true, _ =>
        {
            weapon.SetAttackInput(true);
            fsmController?.OnFire();
        }), WeaponInputPriority);

        command.AddHandler(InputActionPhase.Canceled, new ConditionalInputHandler(() => true, _ =>
        {
            fsmController?.OnFireCanceled();
        }), WeaponInputPriority);
    }
}
}

