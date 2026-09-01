using System;
using UnityEngine;

namespace SAS.Checkpoints
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class CheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private Checkpoint m_Checkpoint;

        [SerializeField] private string m_PlayerTag = "Player";

        [SerializeField] private bool m_DisableAfterActivation;

        private bool _activationInProgress;

        private async void OnTriggerEnter(Collider other)
        {
            if (_activationInProgress || other == null || !other.CompareTag(m_PlayerTag))
                return;

            if (m_Checkpoint == null)
            {
                Debug.LogError($"Checkpoint trigger '{name}' has no checkpoint.", this);
                return;
            }

            _activationInProgress = true;

            try
            {
                bool activated = await m_Checkpoint.CompleteAsync();

                if (activated && m_DisableAfterActivation)
                    enabled = false;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Checkpoint trigger '{name}' failed.\n{exception}", this);
            }
            finally
            {
                _activationInProgress = false;
            }
        }

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();

            if (trigger != null)
                trigger.isTrigger = true;

            m_Checkpoint = GetComponentInParent<Checkpoint>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_Checkpoint == null)
                Debug.LogWarning($"Checkpoint trigger '{name}' has no checkpoint.", this);

            if (string.IsNullOrWhiteSpace(m_PlayerTag))
                Debug.LogWarning($"Checkpoint trigger '{name}' has an empty player tag.", this);
        }
#endif
    }
}