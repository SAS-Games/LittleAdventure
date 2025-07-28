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
        m_LocalizedStringEvent.OnUpdateString.AddListener(SetText);

    }

    public void SetText(string text)
    {
        m_Text.text = text; 
    }

    public void SetLocalText(string id)
    {
        m_LocalizedStringEvent.StringReference = new LocalizedString(m_LocalizedTableName, id);
    }

    public void BindSelectedEvent(UnityAction<int> action, int parameter)
    {
        m_Button.onClick.AddListener(() => action(parameter));
    }

}
