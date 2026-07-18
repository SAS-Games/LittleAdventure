using SAS.Core.TagSystem;
using UnityEngine;

namespace SAS.DialogueSystem
{
    public class ContinueIconPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject m_ContinueIcon;
        [FieldRequiresParent] protected DialogueHandler _dialogueHandler;

        protected virtual void Awake()
        {
            this.Initialize();
            if (_dialogueHandler != null)
                _dialogueHandler.OnStateChanged += HandleStateChanged;
        }

        private void OnDestroy()
        {
            if (_dialogueHandler == null)
                return;

            _dialogueHandler.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(DialogueSessionState state)
        {
            if (m_ContinueIcon != null)
                m_ContinueIcon.SetActive(state == DialogueSessionState.WaitingForAdvance);
        }
    }
}
