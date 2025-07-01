using SAS.Utilities.TagSystem;
using TMPro;
using UnityEngine;

public class TextValueBinder : ProxyViewBinder<float>
{
    [FieldRequiresChild] private TMP_Text m_TextMesh;
    [SerializeField] private string m_ReplacableText = "{Value}";

    private string _text;

    private void Awake()
    {
        this.Initialize();
        _text = m_TextMesh.text;
    }

    protected override void OnValueChanged(float value)
    {
        m_TextMesh.text = _text.Replace(m_ReplacableText, value.ToString());
    }
}