using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    [CustomPropertyDrawer(typeof(StreamingPrefabReference))]
    internal sealed class StreamingPrefabReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            StreamingAssetReferenceDrawerGui.Draw(position, property, label, typeof(GameObject));
        }
    }

    [CustomPropertyDrawer(typeof(StreamingSceneReference))]
    internal sealed class StreamingSceneReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            StreamingAssetReferenceDrawerGui.Draw(position, property, label, typeof(SceneAsset));
        }
    }

    internal static class StreamingAssetReferenceDrawerGui
    {
        public static void Draw(Rect position, SerializedProperty property, GUIContent label, System.Type assetType)
        {
            SerializedProperty guidProperty = property.FindPropertyRelative("m_AssetGUID");
            if (guidProperty == null)
            {
                EditorGUI.LabelField(position, label.text, "Invalid streaming asset reference");
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guidProperty.stringValue);
            Object currentAsset = string.IsNullOrWhiteSpace(assetPath)
                ? null
                : AssetDatabase.LoadAssetAtPath(assetPath, assetType);

            if (currentAsset == null && !string.IsNullOrWhiteSpace(guidProperty.stringValue))
            {
                label = new GUIContent(label.text,
                    $"The stored asset GUID could not be resolved: {guidProperty.stringValue}");
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            Object selectedAsset = EditorGUI.ObjectField(position, label, currentAsset, assetType, false);
            if (EditorGUI.EndChangeCheck())
            {
                string selectedPath = selectedAsset == null ? null : AssetDatabase.GetAssetPath(selectedAsset);
                guidProperty.stringValue = string.IsNullOrWhiteSpace(selectedPath)
                    ? string.Empty
                    : AssetDatabase.AssetPathToGUID(selectedPath);

                AddressablesEditorValidation.NotifyAssetAssigned(guidProperty.stringValue);

                ClearString(property, "m_SubObjectName");
                ClearString(property, "m_SubObjectType");
                ClearString(property, "m_SubObjectGUID");
                SerializedProperty changedProperty = property.FindPropertyRelative("m_EditorAssetChanged");
                if (changedProperty != null)
                    changedProperty.boolValue = true;
            }
            EditorGUI.EndProperty();
        }

        private static void ClearString(SerializedProperty property, string fieldName)
        {
            SerializedProperty child = property.FindPropertyRelative(fieldName);
            if (child != null)
                child.stringValue = string.Empty;
        }
    }
}
