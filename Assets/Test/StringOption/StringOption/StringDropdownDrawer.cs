using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System;

[CustomPropertyDrawer(typeof(StringDropdownAttribute))]
public class StringDropdownDrawer : PropertyDrawer
{
    private const string DEFAULT_EDITOR_RESOURCE_PATH = "StringOptions/DefaultStringOptionsSO.asset";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (StringDropdownAttribute)attribute;

        var targetObject = property.serializedObject.targetObject;
        StringOptions stringOptions = null;

        // If a field name is specified, try to find it
        if (!string.IsNullOrEmpty(attr.SourceFieldName))
        {
            var fieldInfo = targetObject.GetType().GetField(attr.SourceFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
                stringOptions = fieldInfo.GetValue(targetObject) as StringOptions;
            else
            {
                var sourceFieldName = attr.SourceFieldName;
                // If it doesn't contain ".asset", append it
                if (!sourceFieldName.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                    sourceFieldName += ".asset";
                stringOptions = EditorGUIUtility.Load($"StringOptions/{sourceFieldName}") as StringOptions;
            }
        }

        // If no field was provided or the value is null, load default
        if (stringOptions == null)
            stringOptions = EditorGUIUtility.Load(DEFAULT_EDITOR_RESOURCE_PATH) as StringOptions;

        if (stringOptions == null)
        {
            EditorGUI.HelpBox(position, $"No StringOptionsSO found. Set field or add 'Resources/{DEFAULT_EDITOR_RESOURCE_PATH}.asset'", MessageType.Error);
            return;
        }

        var availableOptions = stringOptions.Values;
        string currentValue = property.stringValue;

        // Prepare dropdown list
        List<string> displayList = new List<string> { "<None>" }; // This represents an empty string

        int selectedIndex = 0; // Default to <None>

        // Add actual options
        for (int i = 0; i < availableOptions.Count; i++)
        {
            string option = availableOptions[i];
            displayList.Add(option);

            if (option == currentValue)
                selectedIndex = i + 1; // +1 because <None> is at index 0
        }

        // If value not found, show missing
        if (!string.IsNullOrEmpty(currentValue) && selectedIndex == 0)
        {
            displayList.Insert(1, $"❌ {currentValue} (missing)");
            selectedIndex = 1;
        }

        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, displayList.ToArray());

        // Apply value if changed
        if (newIndex != selectedIndex)
        {
            string selectedValue = displayList[newIndex];

            if (selectedValue.StartsWith("❌"))
                return;

            property.stringValue = (newIndex == 0) ? "" : selectedValue;
        }
    }
}