using SAS.Utilities.TagSystem;
using UnityEngine;

namespace SAS.DialogueSystem
{
    public class LayoutAnimatorTagProcessor : BaseTagProcessor
    {
        public string LayoutAnim { get; private set; } = string.Empty;

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
        public override void Process(string tagValue, TagProcessContext context)
        {
            m_LayoutAnimator?.Play(tagValue);
        }

        public override void Reset()
        {
            base.Reset();
            LayoutAnim = string.Empty;
        }
    }
}