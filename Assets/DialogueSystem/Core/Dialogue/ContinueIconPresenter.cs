using Ink.Runtime;
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
            _dialogueHandler.OnLineReady += OnLineReady;
            _dialogueHandler.OnLineMessageShown += OnLineMessageShown;
        }

        private void OnDestroy()
        {
            if (_dialogueHandler == null)
                return;

            _dialogueHandler.OnLineReady -= OnLineReady;
            _dialogueHandler.OnLineMessageShown -= OnLineMessageShown;
        }

        private void OnLineMessageShown(Story story, DialogueLineContext lineContext)
        {
            if (story != null && m_ContinueIcon != null)
                m_ContinueIcon.SetActive(story.currentChoices.Count == 0);
        }

        private void OnLineReady(DialogueLineContext lineContext)
        {
            if (m_ContinueIcon != null)
                m_ContinueIcon.SetActive(false);
        }
    }
}
