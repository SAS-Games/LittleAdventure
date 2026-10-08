using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    internal static class PolygonStreamingVolumeEditorUtility
    {
        private static readonly GUIContent[] StandardShapeOptions =
        {
            new("Box (Cached Bounds)"),
            new("Polygon Prism (XY + Z Depth)"),
            new("Horizontal Polygon Prism (XZ + Y Height)")
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
            Color previousColor = Handles.color;
            bool removeMode = Event.current.control || Event.current.command;
            float handleZ = (polygon.MinZ + polygon.MaxZ) * 0.5f;
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                Vector2 vertex = polygon.Vertices[i];
                Vector3 position = new(vertex.x, vertex.y, handleZ);
                float size = HandleUtility.GetHandleSize(position) * 0.06f;

                if (removeMode && polygon.Vertices.Count > 3)
                {
                    Handles.color = Color.red;
                    if (Handles.Button(position, Quaternion.identity, size, size,
                            Handles.DotHandleCap))
                    {
                        Undo.RecordObject(undoTarget, undoName);
                        polygon.RemoveVertexAt(i);
                        EditorUtility.SetDirty(undoTarget);
                        Handles.color = previousColor;
                        return true;
                    }
                    continue;
                }

                Handles.color = previousColor;
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

            if (!removeMode)
            {
                Handles.color = Color.green;
                for (int i = 0; i < polygon.Vertices.Count; i++)
                {
                    Vector2 current = polygon.Vertices[i];
                    Vector2 next = polygon.Vertices[(i + 1) % polygon.Vertices.Count];
                    Vector2 midpoint = (current + next) * 0.5f;
                    Vector3 position = new(midpoint.x, midpoint.y, handleZ);
                    float size = HandleUtility.GetHandleSize(position) * 0.035f;
                    if (!Handles.Button(position, Quaternion.identity, size, size,
                            Handles.RectangleHandleCap))
                        continue;

                    Undo.RecordObject(undoTarget, undoName);
                    polygon.InsertVertex(i + 1, midpoint);
                    EditorUtility.SetDirty(undoTarget);
                    Handles.color = previousColor;
                    return true;
                }
            }

            Handles.color = previousColor;
            return changed;
        }

        public static void DrawWire(XZPolygonStreamingVolume polygon, Color color)
        {
            if (polygon?.Vertices == null || polygon.Vertices.Count < 2)
                return;

            Handles.color = color;
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                Vector2 current = polygon.Vertices[i];
                Vector2 next = polygon.Vertices[(i + 1) % polygon.Vertices.Count];
                Vector3 bottomCurrent = new(current.x, polygon.MinY, current.y);
                Vector3 bottomNext = new(next.x, polygon.MinY, next.y);
                Vector3 topCurrent = new(current.x, polygon.MaxY, current.y);
                Vector3 topNext = new(next.x, polygon.MaxY, next.y);
                Handles.DrawLine(bottomCurrent, bottomNext);
                Handles.DrawLine(topCurrent, topNext);
                Handles.DrawLine(bottomCurrent, topCurrent);
            }
        }

        public static bool DrawVertexHandles(XZPolygonStreamingVolume polygon,
            Object undoTarget, string undoName)
        {
            if (polygon?.Vertices == null)
                return false;

            bool changed = false;
            Color previousColor = Handles.color;
            bool removeMode = Event.current.control || Event.current.command;
            float handleY = (polygon.MinY + polygon.MaxY) * 0.5f;
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                Vector2 vertex = polygon.Vertices[i];
                Vector3 position = new(vertex.x, handleY, vertex.y);
                float size = HandleUtility.GetHandleSize(position) * 0.06f;

                if (removeMode && polygon.Vertices.Count > 3)
                {
                    Handles.color = Color.red;
                    if (Handles.Button(position, Quaternion.identity, size, size,
                            Handles.DotHandleCap))
                    {
                        Undo.RecordObject(undoTarget, undoName);
                        polygon.RemoveVertexAt(i);
                        EditorUtility.SetDirty(undoTarget);
                        Handles.color = previousColor;
                        return true;
                    }
                    continue;
                }

                Handles.color = previousColor;
                EditorGUI.BeginChangeCheck();
                Vector3 moved = Handles.Slider2D(
                    position,
                    Vector3.up,
                    Vector3.right,
                    Vector3.forward,
                    size,
                    Handles.DotHandleCap,
                    Vector2.zero);
                if (!EditorGUI.EndChangeCheck())
                    continue;

                Undo.RecordObject(undoTarget, undoName);
                polygon.SetVertex(i, new Vector2(moved.x, moved.z));
                EditorUtility.SetDirty(undoTarget);
                changed = true;
            }

            if (!removeMode)
            {
                Handles.color = Color.green;
                for (int i = 0; i < polygon.Vertices.Count; i++)
                {
                    Vector2 current = polygon.Vertices[i];
                    Vector2 next = polygon.Vertices[(i + 1) % polygon.Vertices.Count];
                    Vector2 midpoint = (current + next) * 0.5f;
                    Vector3 position = new(midpoint.x, handleY, midpoint.y);
                    float size = HandleUtility.GetHandleSize(position) * 0.035f;
                    if (!Handles.Button(position, Quaternion.identity, size, size,
                            Handles.RectangleHandleCap))
                        continue;

                    Undo.RecordObject(undoTarget, undoName);
                    polygon.InsertVertex(i + 1, midpoint);
                    EditorUtility.SetDirty(undoTarget);
                    Handles.color = previousColor;
                    return true;
                }
            }

            Handles.color = previousColor;
            return changed;
        }

        private static void DrawShapePopup(Rect position, SerializedProperty volume, Bounds initialBounds)
        {
            object value = volume.managedReferenceValue;
            int current = value switch
            {
                PolygonStreamingVolume => 1,
                XZPolygonStreamingVolume => 2,
                _ => 0
            };
            GUIContent[] options = StandardShapeOptions;

            if (value != null && value is not PolygonStreamingVolume && value is not XZPolygonStreamingVolume)
            {
                current = 3;
                options = new[]
                {
                    StandardShapeOptions[0],
                    StandardShapeOptions[1],
                    StandardShapeOptions[2],
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
                2 => new XZPolygonStreamingVolume(initialBounds),
                _ => value
            };
            volume.isExpanded = selected is 1 or 2;
        }
    }
}
