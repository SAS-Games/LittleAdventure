using System;
using SAS.WeaponSystem;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
    public class ComboWeapon : MonoBehaviour, IWeapon, IActionGraphExecutionController, IAttackInputReceiver
    {
        [SerializeField] private ActionGraphAsset comboGraphConfig;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform hitOrigin;

        private readonly WeaponContext _context = new WeaponContext();
        private readonly ActionGraphExecutor _executor = new ActionGraphExecutor();
        public bool IsInUse => _executor.IsExecuting;
        public bool IsExecuting => _executor.IsExecuting;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInParent<Animator>();

            BuildGraph();
        }

        private void OnDisable()
        {
            CancelExecution();
        }

        private void OnDestroy()
        {
            CancelExecution();
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
            if (IsInUse)
                return;

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

        private async Awaitable ExecuteGraphAsync()
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

    }
}
