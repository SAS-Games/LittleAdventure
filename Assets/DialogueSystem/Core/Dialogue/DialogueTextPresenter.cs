using UnityEngine;
using TMPro;
using SAS.Utilities.TagSystem;

namespace SAS.DialogueSystem
{
    public class DialogueTextPresenter : MonoBehaviour
    {
        [FieldRequiresSelf] private TMP_Text m_DialogueText;
        [FieldRequiresSelf] protected ITypewriterEffect _typewriterEffect;
        [FieldRequiresParent] protected DialogueHandler _dialogueHandler;

        protected virtual void Awake()
        {
            this.Initialize();
            _dialogueHandler.OnEnterDialogueMode += OnEnterDialogueMode;
            _dialogueHandler.OnExitDialogueMode += ExitDialogueMode;
            _dialogueHandler.OnSkipRequested += Skip;
            _dialogueHandler.OnStoryContinue += OnStoryContinue;

            _typewriterEffect.CompleteTextRevealed += () =>
            {
                _dialogueHandler.OnStoryMessageShown?.Invoke(_dialogueHandler.CurrentStory);
                if (_dialogueHandler.CurrentStory.currentChoices.Count == 0)
                    _dialogueHandler.ContinueStory();
            };

        }

        private void Skip() => _typewriterEffect.Skip(true);
        protected virtual void OnEnterDialogueMode() { }
        protected virtual void ExitDialogueMode() => m_DialogueText.text = "";
        protected virtual void OnStoryContinue(string textToDisplay) => _typewriterEffect.StartTyping(textToDisplay);

    }
}
