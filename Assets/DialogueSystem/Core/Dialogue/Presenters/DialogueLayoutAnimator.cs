using SAS.Core.TagSystem;
using UnityEngine;

namespace SAS.DialogueSystem
{
    [RequireComponent(typeof(LayoutAnimatorTagProcessor), typeof(Animator))]
    public class DialogueLayoutAnimator : MonoBehaviour
    {
        [FieldRequiresSelf] private Animator m_LayoutAnimator;
        [FieldRequiresSelf] private LayoutAnimatorTagProcessor _tagProcessor;
        [FieldRequiresParent] private DialogueHandler _dialogueHandler;

        void Awake()
        {
            this.Initialize();
            _dialogueHandler.OnEnterDialogueMode += OnEnterDialogueMode;
        }

        private void OnEnterDialogueMode()
        {
            m_LayoutAnimator?.Play("None");
        }
    }
}
