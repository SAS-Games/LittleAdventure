using UnityEngine;

namespace SAS.StringTest
{
    public class ReferenceStringDropdownAttribute : PropertyAttribute
    {
        public string SourceFieldName { get; private set; }

        public ReferenceStringDropdownAttribute() { }

        public ReferenceStringDropdownAttribute(string sourceFieldName)
        {
            this.SourceFieldName = sourceFieldName;
        }
    }

}