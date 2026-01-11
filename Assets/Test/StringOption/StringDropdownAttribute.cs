using UnityEngine;

public class StringDropdownAttribute : PropertyAttribute
{
    public string SourceFieldName { get; private set; }

    public StringDropdownAttribute() { }

    public StringDropdownAttribute(string sourceFieldName)
    {
        this.SourceFieldName = sourceFieldName;
    }
}
