using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace SAS.StringTest
{
    [CustomPropertyDrawer(typeof(ReferenceString))]
    public class ReferenceStringDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, bool> RenameStates = new();
        private static readonly Dictionary<string, string> RenameBuffers = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var guidProp = property.FindPropertyRelative("guid");
            var resolvedNameProp = property.FindPropertyRelative("resolvedName");
            var sourceOptionsProp = property.FindPropertyRelative("sourceOptions");

            string propertyKey = property.serializedObject.targetObject.GetInstanceID() + "_" + property.propertyPath;

            bool isRenaming = RenameStates.TryGetValue(propertyKey, out var state) && state;
            string renameBuffer = RenameBuffers.TryGetValue(propertyKey, out var buffer) ? buffer : "";

            var targetObject = property.serializedObject.targetObject;
            ReferenceStringDropdownAttribute attr = fieldInfo.GetCustomAttribute<ReferenceStringDropdownAttribute>();
            ReferenceStringOptions stringOptions = null;

            // Resolve options source
            if (attr != null && !string.IsNullOrEmpty(attr.SourceFieldName))
            {
                var sourceFieldInfo = targetObject.GetType().GetField(attr.SourceFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (sourceFieldInfo != null)
                    stringOptions = sourceFieldInfo.GetValue(targetObject) as ReferenceStringOptions;
                else
                {
                    string resourcePath = attr.SourceFieldName;
                    if (resourcePath.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase))
                        resourcePath = resourcePath.Substring(0, resourcePath.Length - 6);

                    stringOptions = Resources.Load<ReferenceStringOptions>(resourcePath);
                }
            }

            if (stringOptions == null && sourceOptionsProp.objectReferenceValue != null)
                stringOptions = sourceOptionsProp.objectReferenceValue as ReferenceStringOptions;

            if (stringOptions == null)
                stringOptions = Resources.Load<ReferenceStringOptions>("StringOptions/DefaultStringOptionsSO");

            if (stringOptions == null)
            {
                EditorGUI.HelpBox(position, "Missing StringOptions asset.", MessageType.Error);
                return;
            }

            string currentGUID = guidProp.stringValue;
            string currentName = stringOptions.GetNameByGUID(currentGUID);

            // Fallback for deleted/missing entries
            if (string.IsNullOrEmpty(currentName))
                currentName = resolvedNameProp.stringValue;

            // Layout

            Rect dropdownRect = new Rect(position.x, position.y, position.width - 50, position.height);
            Rect renameRect = new Rect(position.x + position.width - 48, position.y, 22, position.height);
            Rect addRect = new Rect(position.x + position.width - 24, position.y, 22, position.height);

            // Rename Mode
            if (isRenaming && !string.IsNullOrEmpty(currentGUID))
            {
                EditorGUI.BeginChangeCheck();

                renameBuffer = EditorGUI.DelayedTextField(dropdownRect, label.text, renameBuffer);

                if (EditorGUI.EndChangeCheck())
                {
                    if (stringOptions.Entries.Exists(e => e.name == renameBuffer && e.guid != currentGUID))
                    {
                        EditorUtility.DisplayDialog("Duplicate Key Name", $"A key with the name '{renameBuffer}' already exists.\nPlease choose a unique name.", "OK");
                        return;
                    }

                    Undo.RecordObject(stringOptions, "Rename String Option");

                    var entry = stringOptions.Entries.Find(e => e.guid == currentGUID);
                    if (entry != null)
                        entry.name = renameBuffer;

                    resolvedNameProp.stringValue = renameBuffer;
                    stringOptions.RebuildCache();

                    EditorUtility.SetDirty(stringOptions);
                    AssetDatabase.SaveAssets();

                    RenameStates[propertyKey] = false;
                }

                RenameBuffers[propertyKey] = renameBuffer;
            }
            else
            {
                // Dropdown
                List<string> displayList = new List<string> { "<None>" };
                int selectedIndex = 0;

                for (int i = 0; i < stringOptions.Entries.Count; i++)
                {
                    var entry = stringOptions.Entries[i];
                    displayList.Add(entry.name);
                    if (entry.guid == currentGUID)
                        selectedIndex = i + 1;
                }

                if (selectedIndex == 0 && !string.IsNullOrEmpty(currentGUID))
                {
                    string missingLabel = !string.IsNullOrEmpty(currentName) ? $"❌ {currentName} (missing)" : $"❌ Missing GUID";
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
                        sourceOptionsProp.objectReferenceValue = stringOptions;
                    }
                }
            }

            // Rename Button
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(currentGUID)))
            {
                if (GUI.Button(renameRect, "✎"))
                {
                    RenameStates[propertyKey] = !isRenaming;
                    RenameBuffers[propertyKey] = currentName;
                }
            }

            // Add Button
            if (GUI.Button(addRect, "+"))
            {
                string newKeyName = ObjectNames.GetUniqueName(stringOptions.Entries.ConvertAll(e => e.name).ToArray(), "NewKey");
                Undo.RecordObject(stringOptions, "Add String Option");
                stringOptions.AddEntry(newKeyName);

                EditorUtility.SetDirty(stringOptions);

                var newEntry = stringOptions.Entries[stringOptions.Entries.Count - 1];
                guidProp.stringValue = newEntry.guid;
                resolvedNameProp.stringValue = newEntry.name;
                sourceOptionsProp.objectReferenceValue = stringOptions;
                AssetDatabase.SaveAssets();
            }

            property.serializedObject.ApplyModifiedProperties();
        }
    }
}