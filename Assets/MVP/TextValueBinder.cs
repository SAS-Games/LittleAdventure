using SAS.Utilities.TagSystem;
using TMPro;
using UnityEngine;

public class TextValueBinder<T> : ProxyViewBinder<T>
{
    [FieldRequiresChild] protected TMP_Text m_TextMesh;
    [SerializeField] private string m_ReplacableText = "{Value}";

    private string _originalText;

    protected virtual void Awake()
    {
        this.Initialize();
        _originalText = m_TextMesh.text;
    }

    protected override void OnValueChanged(T value)
    {
        string displayValue = value != null ? value.ToString() : string.Empty;
        m_TextMesh.text = _originalText.Replace(m_ReplacableText, displayValue);
    }
}