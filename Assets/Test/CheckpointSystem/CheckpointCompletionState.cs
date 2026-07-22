using System;
using System.Collections.Generic;
using SAS.Core.TagSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAS.Checkpoints
{
    [DisallowMultipleComponent]
    public sealed class CheckpointCompletionState : MonoBehaviour
    {
        [SerializeField] private string m_CheckpointId;

        [Tooltip("Objects that are enabled after this checkpoint is completed.")]
        [SerializeField] private List<GameObject> m_EnableWhenCompleted = new();

        [Tooltip("Objects that are disabled after this checkpoint is completed.")]
        [SerializeField] private List<GameObject> m_DisableWhenCompleted = new();

        [Tooltip("When incomplete, applies the opposite state to both object lists.")]
        [FormerlySerializedAs("m_RestoreObjectsIfIncomplete")]
        [SerializeField] private bool m_ApplyIncompleteState = true;

        [Tooltip("Disables this GameObject after completion. Keep this component on an always-loaded controller when possible.")]
        [SerializeField] private bool m_DisableSelfWhenCompleted;

        [Inject] private ICheckpointProgressService _checkpointProgressService;

        public string CheckpointId => m_CheckpointId;

        private void Awake()
        {
            this.InjectFieldBindings();

            if (_checkpointProgressService == null)
            {
                Debug.LogError($"{nameof(CheckpointCompletionState)} on '{name}' " + "could not resolve the checkpoint progress service.", this);
                return;
            }

            _checkpointProgressService.Initialized += OnProgressInitialized;
            _checkpointProgressService.CheckpointCompleted += OnCheckpointCompleted;
            _checkpointProgressService.ProgressReset += OnProgressReset;

            if (_checkpointProgressService.IsInitialized)
                Refresh();
        }

        private void OnDestroy()
        {
            if (_checkpointProgressService == null)
                return;

            _checkpointProgressService.Initialized -= OnProgressInitialized;
            _checkpointProgressService.CheckpointCompleted -= OnCheckpointCompleted;
            _checkpointProgressService.ProgressReset -= OnProgressReset;
        }

        public void Refresh()
        {
            if (_checkpointProgressService == null || !_checkpointProgressService.IsInitialized)
                return;

            ApplyState(_checkpointProgressService.IsCompleted(m_CheckpointId));
        }

        private void OnProgressInitialized()
        {
            Refresh();
        }

        private void OnCheckpointCompleted(string checkpointId)
        {
            if (!string.Equals(checkpointId, m_CheckpointId, StringComparison.Ordinal))
                return;

            ApplyState(true);
        }

        private void OnProgressReset()
        {
            ApplyState(false);
        }

        private void ApplyState(bool isCompleted)
        {
            if (!isCompleted && !m_ApplyIncompleteState)
                return;

            SetActive(m_EnableWhenCompleted, isCompleted);
            SetActive(m_DisableWhenCompleted, !isCompleted);

            if (m_DisableSelfWhenCompleted)
                gameObject.SetActive(!isCompleted);
        }

        private static void SetActive(IEnumerable<GameObject> targets, bool isActive)
        {
            if (targets == null)
                return;

            foreach (GameObject target in targets)
            {
                if (target != null)
                    target.SetActive(isActive);
            }
        }
    }
}
