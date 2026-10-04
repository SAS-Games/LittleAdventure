using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    internal static class PolygonStreamingVolumeEditorUtility
    {
        private static readonly GUIContent[] StandardShapeOptions =
        {
            new("Box (Cached Bounds)"),
            new("Polygon Prism")
        };

        public static float GetShapeFieldHeight(SerializedProperty volume)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (volume?.managedReferenceValue != null)
            {
                height += EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUI.GetPropertyHeight(volume, true);
            }
            return height;
        }

        public static void DrawShapeField(Rect position, SerializedProperty volume, Bounds initialBounds)
        {
            if (volume == null)
                return;

            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            Rect row = new(position.x, position.y, position.width, line);
            DrawShapePopup(row, volume, initialBounds);

            if (volume.managedReferenceValue != null)
            {
                row.y += line + spacing;
                float height = EditorGUI.GetPropertyHeight(volume, true);
                EditorGUI.PropertyField(new Rect(row.x, row.y, row.width, height),
                    volume, new GUIContent("Shape Data"), true);
            }
        }

        public static void DrawShapeFieldLayout(SerializedProperty volume, Bounds initialBounds)
        {
            if (volume == null)
                return;

            Rect row = EditorGUILayout.GetControlRect();
            DrawShapePopup(row, volume, initialBounds);
            if (volume.managedReferenceValue != null)
                EditorGUILayout.PropertyField(volume, new GUIContent("Shape Data"), true);
        }

        public static void DrawWire(PolygonStreamingVolume polygon, Color color)
        {
            if (polygon?.Vertices == null || polygon.Vertices.Count < 2)
                return;

            Handles.color = color;
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                Vector2 current = polygon.Vertices[i];
                Vector2 next = polygon.Vertices[(i + 1) % polygon.Vertices.Count];
                Vector3 backCurrent = new(current.x, current.y, polygon.MinZ);
                Vector3 backNext = new(next.x, next.y, polygon.MinZ);
                Vector3 frontCurrent = new(current.x, current.y, polygon.MaxZ);
                Vector3 frontNext = new(next.x, next.y, polygon.MaxZ);
                Handles.DrawLine(backCurrent, backNext);
                Handles.DrawLine(frontCurrent, frontNext);
                Handles.DrawLine(backCurrent, frontCurrent);
            }
        }

        public static bool DrawVertexHandles(PolygonStreamingVolume polygon,
            Object undoTarget, string undoName)
        {
            if (polygon?.Vertices == null)
                return false;

            bool changed = false;
            float handleZ = (polygon.MinZ + polygon.MaxZ) * 0.5f;
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                Vector2 vertex = polygon.Vertices[i];
                Vector3 position = new(vertex.x, vertex.y, handleZ);
                float size = HandleUtility.GetHandleSize(position) * 0.06f;

                EditorGUI.BeginChangeCheck();
                Vector3 moved = Handles.Slider2D(
                    position,
                    Vector3.forward,
                    Vector3.right,
                    Vector3.up,
                    size,
                    Handles.DotHandleCap,
                    Vector2.zero);
                if (!EditorGUI.EndChangeCheck())
                    continue;

                Undo.RecordObject(undoTarget, undoName);
                polygon.SetVertex(i, new Vector2(moved.x, moved.y));
                EditorUtility.SetDirty(undoTarget);
                changed = true;
            }

            return changed;
        }

        private static void DrawShapePopup(Rect position, SerializedProperty volume, Bounds initialBounds)
        {
            object value = volume.managedReferenceValue;
            int current = value is PolygonStreamingVolume ? 1 : 0;
            GUIContent[] options = StandardShapeOptions;

            if (value != null && value is not PolygonStreamingVolume)
            {
                current = 2;
                options = new[]
                {
                    StandardShapeOptions[0],
                    StandardShapeOptions[1],
                    new GUIContent($"Custom ({value.GetType().Name})")
                };
            }

            EditorGUI.BeginChangeCheck();
            int selected = EditorGUI.Popup(position, new GUIContent("Region Shape"), current, options);
            if (!EditorGUI.EndChangeCheck() || selected == current)
                return;

            volume.managedReferenceValue = selected switch
            {
                0 => null,
                1 => new PolygonStreamingVolume(initialBounds),
                _ => value
            };
            volume.isExpanded = selected == 1;
        }
    }
}
