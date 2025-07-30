using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace SAS.DialogueSystem
{
    [RequireComponent(typeof(LocaleTextTagProcessor))]
    public class DialogueLocaleTextPresenter : DialogueTextPresenter
    {
        [FieldRequiresSelf] LocaleTextTagProcessor _tagProcessor;
        [SerializeField] private LocalizeStringEvent m_LocalizedStringEvent;
        [SerializeField] private string m_LocalizedTableName = "DialogueTextTable";

        protected override void OnEnterDialogueMode()
        {
            m_LocalizedStringEvent.OnUpdateString.AddListener(_typewriterEffect.StartTyping);
        }

        protected override void ExitDialogueMode()
        {
            base.ExitDialogueMode();
            m_LocalizedStringEvent.OnUpdateString.RemoveListener(_typewriterEffect.StartTyping);
        }

        protected override void OnStoryContinue(string textToDisplay)
        {
            if (!string.IsNullOrEmpty(_tagProcessor.Locale))
                m_LocalizedStringEvent.StringReference = new LocalizedString(m_LocalizedTableName, _tagProcessor.Locale);
            else
                base.OnStoryContinue(textToDisplay);
        }
    }
}
