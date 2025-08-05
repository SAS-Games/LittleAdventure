using SAS.Utilities.TagSystem;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using Debug = SAS.Debug;

public class CharacterAnimatorProcessor : MonoBehaviour
{
    [Inject] private IDialogueHandler _dialogueHandler;
    [SerializeField] private string m_Tag;
    [SerializeField] private Animator m_Animator;
    private IDisposable _disposable;

    void Start()
    {
        this.Initialize();
    }

    public void Register()
    {
        var animatorProcessors = (_dialogueHandler as Component).GetComponentsInChildren<IAnimatorProcessor>(true);
        var animatorProcessor = animatorProcessors.FirstOrDefault(p => p.Tag == m_Tag);

        if (animatorProcessor == null)
        {
            Debug.LogWarning($"No IAnimatorProcessor found with matching tag '{m_Tag}' " +
             $"Checked {animatorProcessors.Length} processors in children. " +
             $"Ensure that a ProxyAnimatorProcessor with tag '{m_Tag}' exists as a child of DialogueHandler.", "DialogueHandler");
            return;
        }

        if (_disposable == null)
        {
           _disposable = animatorProcessor.AnimatorState
                .Subscribe(state =>
                {
                    if (m_Animator != null)
                        m_Animator.Play(state);
                })
                .AddTo(this);
        }
    }

    public void Unregister() { 
        _disposable?.Dispose();
        _disposable = null;
    }
}
