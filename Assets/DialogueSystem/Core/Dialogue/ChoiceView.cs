using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class ChoiceView : MonoBehaviour
{
    [SerializeField] private Button m_Button;
    [SerializeField] private TMP_Text m_Text;
    [SerializeField] private LocalizeStringEvent m_LocalizedStringEvent;
    [SerializeField] private string m_LocalizedTableName = "DialogueTextTable";

    private void Awake()
    {
        if (m_LocalizedStringEvent != null)
            m_LocalizedStringEvent.OnUpdateString.AddListener(SetText);
    }

    private void OnDestroy()
    {
        if (m_LocalizedStringEvent != null)
            m_LocalizedStringEvent.OnUpdateString.RemoveListener(SetText);
    }

    public void SetText(string text)
    {
        if (m_Text != null)
            m_Text.text = text; 
    }

    public void SetLocalText(string id)
    {
        SetLocalText(id, string.Empty);
    }

    public void SetLocalText(string id, string fallbackText)
    {
        if (m_LocalizedStringEvent == null || string.IsNullOrEmpty(id))
        {
            SetText(fallbackText);
            return;
        }

        SetText(fallbackText);
        m_LocalizedStringEvent.StringReference = new LocalizedString(m_LocalizedTableName, id);
    }

    public void BindSelectedEvent(UnityAction<int> action, int parameter)
    {
        if (m_Button == null)
            return;

        m_Button.onClick.RemoveAllListeners();
        m_Button.onClick.AddListener(() => action(parameter));
    }

    public void ClearSelectedEvents()
    {
        if (m_Button != null)
            m_Button.onClick.RemoveAllListeners();
    }

}
