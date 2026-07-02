using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
public class Weapon : MonoBehaviour
{
    [SerializeField] private ActionGraphAsset attackGraphConfig;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform hitOrigin;
    [SerializeField] private float comboResetDelay = 0.75f;
    [SerializeField] private bool queueInputWhileExecuting = true;

    private readonly WeaponContext _context = new WeaponContext();
    private ExecutionGraph _graph;
    private CancellationTokenSource _cts;
    private Task _currentExecution;
    private int _comboIndex;
    private float _lastAttackEndTime = -999f;
    private bool _queuedAttack;

    public int ComboIndex => _comboIndex;
    public bool IsExecuting => _currentExecution != null && !_currentExecution.IsCompleted;

    private void Awake()
    {
        BuildGraph();
    }

    private void OnDisable()
    {
        CancelAttack();
    }

    public void BuildGraph()
    {
        if (attackGraphConfig == null || attackGraphConfig.root == null)
        {
            _graph = null;
            return;
        }

        if (animator == null)
            animator = GetComponentInParent<Animator>();

        _context.Owner = gameObject;
        _context.Weapon = this;
        _context.Animator = animator;
        _context.FirePoint = firePoint != null ? firePoint : transform;
        _context.HitOrigin = hitOrigin != null ? hitOrigin : transform;

        _graph = new ExecutionGraph(attackGraphConfig.root);
        _graph.Initialize(_context);
    }

    public async void Attack()
    {
        if (_graph == null)
            BuildGraph();

        if (_graph == null)
        {
            Debug.LogWarning($"{nameof(Weapon)} has no attack graph.", this);
            return;
        }

        if (IsExecuting)
        {
            if (queueInputWhileExecuting)
                _queuedAttack = true;
            return;
        }

        if (Time.time - _lastAttackEndTime > comboResetDelay)
            ResetCombo();

        try
        {
            do
            {
                _queuedAttack = false;
                await ExecuteAttackOnce();
            }
            while (_queuedAttack);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void CancelAttack()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        _currentExecution = null;
        _queuedAttack = false;
        ResetCombo();
    }

    public void ResetCombo()
    {
        _comboIndex = 0;
        _queuedAttack = false;
        _context.CurrentAttackIndex = 0;
        _graph?.Reset();
    }

    public void AdvanceCombo(int comboCount, bool resetGraphWhenWrapped)
    {
        int safeComboCount = Mathf.Max(1, comboCount);
        _comboIndex = (_comboIndex + 1) % safeComboCount;
        _context.CurrentAttackIndex = _comboIndex;

        if (_comboIndex == 0 && resetGraphWhenWrapped)
            _graph?.Reset();
    }

    private async Task ExecuteAttackOnce()
    {
        _cts = new CancellationTokenSource();
        _context.BeginAttack(_comboIndex);

        try
        {
            _currentExecution = _graph.ExecuteAsync(_context, _cts.Token);
            await _currentExecution;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _lastAttackEndTime = Time.time;
            _cts?.Dispose();
            _cts = null;
            _currentExecution = null;
        }
    }
}
}

