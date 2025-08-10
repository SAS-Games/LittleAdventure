using Ink.Runtime;
using SAS.Utilities.TagSystem;
using UniRx;
using UnityEngine;

public interface IAnimatorProcessor
{
    string Tag { get; }
    void Process(string value);
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
            _dialogueHandler.OnStoryMessageShown += OnTextRevealed;
        }

        private void OnDestroy()
        {
            _dialogueHandler.OnStoryMessageShown -= OnTextRevealed;
        }

        private void OnTextRevealed(Story story)
        {
            if (_dialogueHandler.TagProcessContext.CurrentSpeakerId == m_Tag)
            {
                if (!string.IsNullOrEmpty(m_IdleAnimState))
                    _animatorState.Value = m_IdleAnimState;
            }
        }

        void IAnimatorProcessor.Process(string tagValue)
        {
            _animatorState.SetValueAndForceNotify(tagValue);
        }
    }
}