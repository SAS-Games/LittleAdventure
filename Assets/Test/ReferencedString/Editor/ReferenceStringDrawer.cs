using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace SAS.StringTest
{
    [CustomPropertyDrawer(typeof(ReferenceString))]
    public class ReferenceStringDrawer : PropertyDrawer
    {
        private bool isRenaming;
        private string renameBuffer;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var guidProp = property.FindPropertyRelative("guid");
            var resolvedNameProp = property.FindPropertyRelative("resolvedName");
            var sourceOptionsProp = property.FindPropertyRelative("sourceOptions");

            var targetObject = property.serializedObject.targetObject;
            ReferenceStringDropdownAttribute attr = fieldInfo.GetCustomAttribute<ReferenceStringDropdownAttribute>();

            ReferenceStringOptions stringOptions = null;

            // Try from attribute source field
            if (attr != null && !string.IsNullOrEmpty(attr.SourceFieldName))
            {
                var sourceFieldInfo = targetObject.GetType().GetField(attr.SourceFieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (sourceFieldInfo != null)
                    stringOptions = sourceFieldInfo.GetValue(targetObject) as ReferenceStringOptions;
                else
                {
                    // Try load from resources
                    string resourcePath = attr.SourceFieldName;
                    if (resourcePath.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase))
                        resourcePath = resourcePath.Substring(0, resourcePath.Length - 6);

                    stringOptions = Resources.Load<ReferenceStringOptions>(resourcePath);
                }
            }

            // If none found, try the serialized sourceOptions field
            if (stringOptions == null && sourceOptionsProp.objectReferenceValue != null)
                stringOptions = sourceOptionsProp.objectReferenceValue as ReferenceStringOptions;


            // If still null, load default
            if (stringOptions == null)
                stringOptions = Resources.Load<ReferenceStringOptions>("StringOptions/DefaultStringOptionsSO");

            if (stringOptions == null)
            {
                EditorGUI.HelpBox(position, "Missing StringOptions asset.", MessageType.Error);
                return;
            }

            string currentGUID = guidProp.stringValue;
            string currentName = stringOptions.GetNameByGUID(currentGUID);

            // If missing, try lastKnownName from ReferenceString
            if (string.IsNullOrEmpty(currentName))
            {
                var refString = fieldInfo.GetValue(targetObject) as ReferenceString;
                currentName = refString?.GetLastKnownName();
            }

            // Layout
            Rect dropdownRect = new Rect(position.x, position.y, position.width - 50, position.height);
            Rect renameRect = new Rect(position.x + position.width - 48, position.y, 22, position.height);
            Rect addRect = new Rect(position.x + position.width - 24, position.y, 22, position.height);

            if (isRenaming && !string.IsNullOrEmpty(currentGUID))
            {
                EditorGUI.BeginChangeCheck();
                renameBuffer = EditorGUI.DelayedTextField(dropdownRect, label.text, renameBuffer);
                if (EditorGUI.EndChangeCheck())
                {
                    // Duplicate check
                    if (stringOptions.Entries.Exists(e =>
                        e.name == renameBuffer && e.guid != currentGUID))
                    {
                        EditorUtility.DisplayDialog("Duplicate Key Name",
                            $"A key with the name '{renameBuffer}' already exists.\nPlease choose a unique name.",
                            "OK");
                        return;
                    }

                    // Apply rename
                    Undo.RecordObject(stringOptions, "Rename String Option");
                    var entry = stringOptions.Entries.Find(e => e.guid == currentGUID);
                    if (entry != null) entry.name = renameBuffer;
                    EditorUtility.SetDirty(stringOptions);
                    AssetDatabase.SaveAssets();
                    isRenaming = false;
                }
            }
            else
            {
                // Build dropdown
                List<string> displayList = new List<string> { "<None>" };
                int selectedIndex = 0;

                for (int i = 0; i < stringOptions.Entries.Count; i++)
                {
                    var entry = stringOptions.Entries[i];
                    displayList.Add(entry.name);
                    if (entry.guid == currentGUID)
                        selectedIndex = i + 1;
                }

                // Missing entry display
                // Missing entry display
                if (selectedIndex == 0 && !string.IsNullOrEmpty(currentGUID))
                {
                    string missingLabel = !string.IsNullOrEmpty(currentName)
                        ? $"❌ {currentName} (missing)"
                        : $"❌ Missing GUID: {currentGUID}";
                    displayList.Insert(1, missingLabel);
                    selectedIndex = 1;
                }

                int newIndex = EditorGUI.Popup(dropdownRect, label.text, selectedIndex, displayList.ToArray());

                if (newIndex != selectedIndex)
                {
                    if (newIndex == 0)
                    {
                        guidProp.stringValue = "";
                        resolvedNameProp.stringValue = "";
                        sourceOptionsProp.objectReferenceValue = stringOptions;
                    }
                    else
                    {
                        var entry = stringOptions.Entries[newIndex - 1];
                        guidProp.stringValue = entry.guid;
                        resolvedNameProp.stringValue = entry.name;
                    }
                }
            }

            // Rename button
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(currentGUID)))
            {
                if (GUI.Button(renameRect, "✎"))
                {
                    isRenaming = !isRenaming;
                    renameBuffer = currentName;
                }
            }

            // Add button
            if (GUI.Button(addRect, "+"))
            {
                string newKeyName = ObjectNames.GetUniqueName(
                    stringOptions.Entries.ConvertAll(e => e.name).ToArray(), "NewKey");

                Undo.RecordObject(stringOptions, "Add String Option");
                stringOptions.AddEntry(newKeyName);
                EditorUtility.SetDirty(stringOptions);

                var newEntry = stringOptions.Entries[stringOptions.Entries.Count - 1];
                guidProp.stringValue = newEntry.guid;
                resolvedNameProp.stringValue = newEntry.name;
                sourceOptionsProp.objectReferenceValue = stringOptions;
            }
        }
    }
}
