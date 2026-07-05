using UnityEngine;
using TMPro;
using SAS.Core.TagSystem;

namespace SAS.DialogueSystem
{
    public class DialogueTextPresenter : MonoBehaviour
    {
        [FieldRequiresSelf] private TMP_Text m_DialogueText;
        [FieldRequiresSelf] protected ITypewriterEffect _typewriterEffect;
        [FieldRequiresParent] protected DialogueHandler _dialogueHandler;
        private bool _skip;

        protected virtual void Awake()
        {
            this.Initialize();
            if (_dialogueHandler != null)
            {
                _dialogueHandler.OnEnterDialogueMode += OnEnterDialogueMode;
                _dialogueHandler.OnExitDialogueMode += ExitDialogueMode;
                _dialogueHandler.OnSkipRequested += Skip;
                _dialogueHandler.OnLineReady += OnLineReady;
            }

            if (_typewriterEffect != null)
                _typewriterEffect.CompleteTextRevealed += HandleCompleteTextRevealed;
        }

        protected virtual void OnDestroy()
        {
            if (_dialogueHandler != null)
            {
                _dialogueHandler.OnEnterDialogueMode -= OnEnterDialogueMode;
                _dialogueHandler.OnExitDialogueMode -= ExitDialogueMode;
                _dialogueHandler.OnSkipRequested -= Skip;
                _dialogueHandler.OnLineReady -= OnLineReady;
            }

            if (_typewriterEffect != null)
                _typewriterEffect.CompleteTextRevealed -= HandleCompleteTextRevealed;
        }

        private void HandleCompleteTextRevealed()
        {
            if (_dialogueHandler == null)
                return;

            var lineContext = _dialogueHandler.CurrentLineContext;
            _dialogueHandler.NotifyLineMessageShown(lineContext);

            var shouldAdvanceFromSkip = _skip;
            _skip = false;

            var story = _dialogueHandler.CurrentStory;
            if ((_dialogueHandler.AutoContinueToNextLine || shouldAdvanceFromSkip) &&
                story != null &&
                story.currentChoices.Count == 0)
            {
                _dialogueHandler.ContinueStory();
            }
        }

        private void Skip()
        {
            if (_skip)
                return;
            _skip = true;
            _typewriterEffect?.Skip(true);
        }

        protected virtual void OnEnterDialogueMode()
        {
        }

        protected virtual void ExitDialogueMode()
        {
            if (m_DialogueText != null)
                m_DialogueText.text = "";

            (_typewriterEffect as ITypewriterAudioEffect)?.SetDefaultAudioInfo();
        }

        protected virtual void OnLineReady(DialogueLineContext lineContext)
        {
            if (lineContext == null)
                return;

            ApplyLineAudio(lineContext);
            OnStoryContinue(lineContext.Text);
        }

        protected void ApplyLineAudio(DialogueLineContext lineContext)
        {
            if (!string.IsNullOrEmpty(lineContext.AudioInfoId))
                (_typewriterEffect as ITypewriterAudioEffect)?.SetCurrentAudioInfo(lineContext.AudioInfoId);
        }

        protected virtual void OnStoryContinue(string textToDisplay) => _typewriterEffect?.StartTyping(textToDisplay);
    }
}
