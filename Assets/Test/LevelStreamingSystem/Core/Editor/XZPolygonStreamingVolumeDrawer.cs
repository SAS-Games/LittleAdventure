using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    [CustomPropertyDrawer(typeof(XZPolygonStreamingVolume))]
    internal sealed class XZPolygonStreamingVolumeDrawer : PropertyDrawer
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
            height += line + spacing;
            height += (line + spacing) * 2f;

            if (property.managedReferenceValue is XZPolygonStreamingVolume polygon && !polygon.IsValid)
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
            SerializedProperty minY = property.FindPropertyRelative("minY");
            SerializedProperty maxY = property.FindPropertyRelative("maxY");

            row.y += line + spacing;
            float verticesHeight = EditorGUI.GetPropertyHeight(vertices, true);
            EditorGUI.PropertyField(new Rect(row.x, row.y, row.width, verticesHeight), vertices,
                new GUIContent("XZ Vertices"), true);
            row.y += verticesHeight + spacing;

            DrawVertexButtons(row, vertices);
            row.y += line + spacing;
            EditorGUI.PropertyField(row, minY, new GUIContent("Bottom Y"));
            row.y += line + spacing;
            EditorGUI.PropertyField(row, maxY, new GUIContent("Top Y"));
            row.y += line + spacing;

            if (property.managedReferenceValue is XZPolygonStreamingVolume polygon && !polygon.IsValid)
            {
                EditorGUI.HelpBox(
                    new Rect(row.x, row.y, row.width, line * 2f),
                    "Use at least three finite XZ vertices, a positive Y height, and a simple non-crossing outline.",
                    MessageType.Error);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        private static void DrawVertexButtons(Rect row, SerializedProperty vertices)
        {
            const float gap = 4f;
            float width = (row.width - gap) * 0.5f;
            if (GUI.Button(new Rect(row.x, row.y, width, row.height), "Add Vertex"))
                AddVertex(vertices);

            using (new EditorGUI.DisabledScope(vertices.arraySize <= 3))
            {
                if (GUI.Button(new Rect(row.x + width + gap, row.y, width, row.height), "Remove Last"))
                    vertices.DeleteArrayElementAtIndex(vertices.arraySize - 1);
            }
        }

        private static void AddVertex(SerializedProperty vertices)
        {
            int count = vertices.arraySize;
            vertices.InsertArrayElementAtIndex(count);
            SerializedProperty added = vertices.GetArrayElementAtIndex(count);
            Vector2 value = Vector2.zero;
            if (count >= 2)
            {
                Vector2 last = vertices.GetArrayElementAtIndex(count - 1).vector2Value;
                Vector2 first = vertices.GetArrayElementAtIndex(0).vector2Value;
                value = (last + first) * 0.5f;
            }
            else if (count == 1)
            {
                value = vertices.GetArrayElementAtIndex(0).vector2Value + Vector2.right;
            }
            added.vector2Value = value;
        }
    }
}
