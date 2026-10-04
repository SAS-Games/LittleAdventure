using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    [CustomPropertyDrawer(typeof(PolygonStreamingVolume))]
    internal sealed class PolygonStreamingVolumeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            if (!property.isExpanded)
                return line;

            SerializedProperty vertices = property.FindPropertyRelative("vertices");
            float height = line + spacing;
            height += EditorGUI.GetPropertyHeight(vertices, true) + spacing;
            height += (line + spacing) * 2f;

            if (property.managedReferenceValue is PolygonStreamingVolume polygon && !polygon.IsValid)
                height += EditorGUIUtility.singleLineHeight * 2f + spacing;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            Rect row = new(position.x, position.y, position.width, line);

            property.isExpanded = EditorGUI.Foldout(row, property.isExpanded, label, true);
            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;
            SerializedProperty vertices = property.FindPropertyRelative("vertices");
            SerializedProperty minZ = property.FindPropertyRelative("minZ");
            SerializedProperty maxZ = property.FindPropertyRelative("maxZ");

            row.y += line + spacing;
            float verticesHeight = EditorGUI.GetPropertyHeight(vertices, true);
            EditorGUI.PropertyField(new Rect(row.x, row.y, row.width, verticesHeight), vertices, true);
            row.y += verticesHeight + spacing;
            EditorGUI.PropertyField(row, minZ, new GUIContent("Back Z"));
            row.y += line + spacing;
            EditorGUI.PropertyField(row, maxZ, new GUIContent("Front Z"));
            row.y += line + spacing;

            if (property.managedReferenceValue is PolygonStreamingVolume polygon && !polygon.IsValid)
            {
                EditorGUI.HelpBox(
                    new Rect(row.x, row.y, row.width, line * 2f),
                    "Use at least three finite XY vertices, a positive Z depth, and a simple non-crossing outline.",
                    MessageType.Error);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
}
