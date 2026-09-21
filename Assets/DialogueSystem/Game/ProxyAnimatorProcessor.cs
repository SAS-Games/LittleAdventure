using SAS.Core.TagSystem;
using UniRx;
using UnityEngine;

public interface IAnimatorProcessor : IDialogueAnimationTarget
{
    string Tag { get; }
    IReadOnlyReactiveProperty<string> AnimatorState { get; }
}

namespace SAS.DialogueSystem
{
    public class ProxyAnimatorProcessor : MonoBehaviour, IAnimatorProcessor
    {
        [FieldRequiresParent] DialogueHandler _dialogueHandler;
        [SerializeField] private string m_Tag;
        [SerializeField] private string m_IdleAnimState;


        private readonly ReactiveProperty<string> _animatorState = new ReactiveProperty<string>();
        public IReadOnlyReactiveProperty<string> AnimatorState => _animatorState;
        string IAnimatorProcessor.Tag => m_Tag;

        private void Awake()
        {
            this.Initialize();
            if (_dialogueHandler != null)
                _dialogueHandler.OnLinePresented += OnTextRevealed;
        }

        private void OnDestroy()
        {
            if (_dialogueHandler != null)
                _dialogueHandler.OnLinePresented -= OnTextRevealed;
        }

        private void OnTextRevealed(DialogueLineContext lineContext)
        {
            if (lineContext == null)
                return;

            if (lineContext.CurrentSpeakerId == m_Tag)
            {
                if (!string.IsNullOrEmpty(m_IdleAnimState))
                    _animatorState.Value = m_IdleAnimState;
            }
        }

        public void Process(string tagValue)
        {
            _animatorState.SetValueAndForceNotify(tagValue);
        }
    }
}
