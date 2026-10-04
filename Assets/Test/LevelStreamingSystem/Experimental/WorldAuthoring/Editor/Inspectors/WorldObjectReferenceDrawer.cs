using UnityEditor;
using UnityEngine;

namespace SAS.WorldStreaming.Editor
{
    [CustomPropertyDrawer(typeof(WorldObjectReference))]
    internal sealed class WorldObjectReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty idProperty = property.FindPropertyRelative("m_GlobalObjectId");
            SerializedProperty pathProperty = property.FindPropertyRelative("m_HierarchyPath");

            GameObject current = Resolve(idProperty.stringValue);
            GUIContent fieldLabel = string.IsNullOrWhiteSpace(pathProperty.stringValue)
                ? label
                : new GUIContent(label.text, pathProperty.stringValue);

            EditorGUI.BeginProperty(position, fieldLabel, property);
            EditorGUI.BeginChangeCheck();
            GameObject selected = EditorGUI.ObjectField(
                position,
                fieldLabel,
                current,
                typeof(GameObject),
                true) as GameObject;

            if (EditorGUI.EndChangeCheck())
            {
                if (selected == null)
                {
                    idProperty.stringValue = string.Empty;
                    pathProperty.stringValue = string.Empty;
                }
                else
                {
                    idProperty.stringValue = GlobalObjectId.GetGlobalObjectIdSlow(selected).ToString();
                    pathProperty.stringValue = BuildHierarchyPath(selected.transform);
                }
            }

            EditorGUI.EndProperty();
        }

        private static GameObject Resolve(string value)
        {
            if (!GlobalObjectId.TryParse(value, out GlobalObjectId id))
                return null;

            Object resolved = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
            if (resolved is GameObject gameObject)
                return gameObject;
            return resolved is Component component ? component.gameObject : null;
        }

        private static string BuildHierarchyPath(Transform target)
        {
            string path = target.name;
            while (target.parent != null)
            {
                target = target.parent;
                path = $"{target.name}/{path}";
            }

            return path;
        }
    }
}
