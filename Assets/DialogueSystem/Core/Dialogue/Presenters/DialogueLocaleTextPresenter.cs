using SAS.Core.TagSystem;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace SAS.DialogueSystem
{
    [RequireComponent(typeof(LocaleTextTagProcessor)), DisallowMultipleComponent]
    public class DialogueLocaleTextPresenter : DialogueTextPresenter
    {
        [SerializeField] private LocalizeStringEvent m_LocalizedStringEvent;
        [SerializeField] private string m_LocalizedTableName = "DialogueTextTable";

        protected override void OnEnterDialogueMode()
        {
            if (m_LocalizedStringEvent != null && _typewriterEffect != null)
                m_LocalizedStringEvent.OnUpdateString.AddListener(_typewriterEffect.StartTyping);
        }

        protected override void ExitDialogueMode()
        {
            base.ExitDialogueMode();
            if (m_LocalizedStringEvent != null && _typewriterEffect != null)
                m_LocalizedStringEvent.OnUpdateString.RemoveListener(_typewriterEffect.StartTyping);
        }

        protected override void OnDestroy()
        {
            if (m_LocalizedStringEvent != null && _typewriterEffect != null)
                m_LocalizedStringEvent.OnUpdateString.RemoveListener(_typewriterEffect.StartTyping);

            base.OnDestroy();
        }

        protected override void OnLineReady(DialogueLineContext lineContext)
        {
            if (lineContext == null)
                return;

            if (!string.IsNullOrEmpty(lineContext.Locale) && m_LocalizedStringEvent != null)
            {
                ApplyLineAudio(lineContext);
                m_LocalizedStringEvent.StringReference = new LocalizedString(m_LocalizedTableName, lineContext.Locale);
            }
            else
                base.OnLineReady(lineContext);
        }
    }
}
