using Ink.Runtime;
using SAS.Utilities.TagSystem;
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
            _dialogueHandler.OnStoryContinue += OnStoryContinue;
            _dialogueHandler.OnStoryMessageShown += OnStoryMessageShown;
        }

        private void OnStoryMessageShown(Story story) => m_ContinueIcon.SetActive(story.currentChoices.Count == 0);
        private void OnStoryContinue(string textToDisplay) => m_ContinueIcon.SetActive(false);
    }
}
