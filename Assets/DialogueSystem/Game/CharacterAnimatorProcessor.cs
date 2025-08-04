using SAS.Utilities.TagSystem;
using System.Linq;
using UniRx;
using UnityEngine;
using Debug = SAS.Debug;

public class CharacterAnimatorProcessor : MonoBehaviour
{
    [SerializeField] private string m_Tag;
    [Inject] private IDialogueHandler _dialogueHandler;
    [SerializeField] private Animator m_Animator;

    void Start()
    {
        var animatorProcessors = m_Animator.GetComponentsInChildren<IAnimatorProcessor>(true);
        var animatorProcessor = animatorProcessors.FirstOrDefault(p => p.Tag == m_Tag);

        if (animatorProcessor == null)
        {
            Debug.LogWarning($"No IAnimatorProcessor found with matching tag '{m_Tag}' " +
             $"Checked {animatorProcessors.Length} processors in children. " +
             $"Ensure that a ProxyAnimatorProcessor with tag '{m_Tag}' exists as a child of DialogueHandler.", "DialogueHandler");
            return;
        }

        animatorProcessor.AnimatorState
            .Subscribe(state =>
            {
                if (m_Animator != null)
                    m_Animator.Play(state);
            })
            .AddTo(this);
    }
}
