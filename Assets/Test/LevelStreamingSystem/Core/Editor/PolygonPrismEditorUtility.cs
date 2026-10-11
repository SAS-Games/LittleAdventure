using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    internal static class PolygonPrismEditorUtility
    {
        private static readonly GUIContent[] StandardShapeOptions =
        {
            new("Box", "A three-dimensional axis-aligned box."),
            new("Polygon Prism", "A polygon on XY or XZ, extruded perpendicular to that plane.")
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

        public static void DrawShapeField(Rect position, SerializedProperty volume, Bounds initialBounds,
            GUIContent label = null)
        {
            if (volume == null)
                return;

            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            Rect row = new(position.x, position.y, position.width, line);
            DrawShapePopup(row, volume, initialBounds, label);
            if (volume.managedReferenceValue != null)
            {
                row.y += line + spacing;
                float height = EditorGUI.GetPropertyHeight(volume, true);
                EditorGUI.PropertyField(new Rect(row.x, row.y, row.width, height),
                    volume, new GUIContent("Shape Data"), true);
            }
        }

        public static void DrawShapeFieldLayout(SerializedProperty volume, Bounds initialBounds,
            GUIContent label = null)
        {
            if (volume == null)
                return;

            Rect row = EditorGUILayout.GetControlRect();
            DrawShapePopup(row, volume, initialBounds, label);
            if (volume.managedReferenceValue != null)
                EditorGUILayout.PropertyField(volume, new GUIContent("Shape Data"), true);
        }

        public static void DrawWire(PolygonPrismStreamingVolume prism, Color color)
        {
            if (prism?.Vertices == null || prism.Vertices.Count < 2)
                return;

            Handles.color = color;
            for (int i = 0; i < prism.Vertices.Count; i++)
            {
                Vector2 current = prism.Vertices[i];
                Vector2 next = prism.Vertices[(i + 1) % prism.Vertices.Count];
                Vector3 minimumCurrent = ToWorld(prism, current, prism.MinExtrusion);
                Vector3 minimumNext = ToWorld(prism, next, prism.MinExtrusion);
                Vector3 maximumCurrent = ToWorld(prism, current, prism.MaxExtrusion);
                Vector3 maximumNext = ToWorld(prism, next, prism.MaxExtrusion);
                Handles.DrawLine(minimumCurrent, minimumNext);
                Handles.DrawLine(maximumCurrent, maximumNext);
                Handles.DrawLine(minimumCurrent, maximumCurrent);
            }
        }

        public static bool DrawVertexHandles(PolygonPrismStreamingVolume prism,
            Object undoTarget, string undoName)
        {
            if (prism?.Vertices == null)
                return false;

            bool changed = DrawExtrusionHandles(prism, undoTarget, undoName);
            Color previousColor = Handles.color;
            bool removeMode = Event.current.control || Event.current.command;
            float extrusion = (prism.MinExtrusion + prism.MaxExtrusion) * 0.5f;
            Vector3 normal = prism.ProjectionPlane == PolygonPrismPlane.XY
                ? Vector3.forward
                : Vector3.up;
            Vector3 secondAxis = prism.ProjectionPlane == PolygonPrismPlane.XY
                ? Vector3.up
                : Vector3.forward;

            for (int i = 0; i < prism.Vertices.Count; i++)
            {
                Vector2 vertex = prism.Vertices[i];
                Vector3 position = ToWorld(prism, vertex, extrusion);
                float size = HandleUtility.GetHandleSize(position) * 0.06f;

                if (removeMode && prism.Vertices.Count > 3)
                {
                    Handles.color = Color.red;
                    if (Handles.Button(position, Quaternion.identity, size, size, Handles.DotHandleCap))
                    {
                        Undo.RecordObject(undoTarget, undoName);
                        prism.RemoveVertexAt(i);
                        EditorUtility.SetDirty(undoTarget);
                        Handles.color = previousColor;
                        return true;
                    }
                    continue;
                }

                Handles.color = previousColor;
                EditorGUI.BeginChangeCheck();
                Vector3 moved = Handles.Slider2D(
                    position, normal, Vector3.right, secondAxis,
                    size, Handles.DotHandleCap, Vector2.zero);
                if (!EditorGUI.EndChangeCheck())
                    continue;

                Undo.RecordObject(undoTarget, undoName);
                prism.SetVertex(i, FromWorld(prism, moved));
                EditorUtility.SetDirty(undoTarget);
                changed = true;
            }

            if (!removeMode)
            {
                Handles.color = Color.green;
                for (int i = 0; i < prism.Vertices.Count; i++)
                {
                    Vector2 midpoint = (prism.Vertices[i] +
                                        prism.Vertices[(i + 1) % prism.Vertices.Count]) * 0.5f;
                    Vector3 position = ToWorld(prism, midpoint, extrusion);
                    float size = HandleUtility.GetHandleSize(position) * 0.035f;
                    if (!Handles.Button(position, Quaternion.identity, size, size,
                            Handles.RectangleHandleCap))
                        continue;

                    Undo.RecordObject(undoTarget, undoName);
                    prism.InsertVertex(i + 1, midpoint);
                    EditorUtility.SetDirty(undoTarget);
                    Handles.color = previousColor;
                    return true;
                }
            }

            Handles.color = previousColor;
            return changed;
        }

        private static void DrawShapePopup(Rect position, SerializedProperty volume,
            Bounds initialBounds, GUIContent label)
        {
            object value = volume.managedReferenceValue;
            int current = value switch
            {
                PolygonPrismStreamingVolume => 1,
                BoxStreamingVolume => 0,
                _ => 0
            };
            GUIContent[] options = StandardShapeOptions;

            if (value != null && value is not PolygonPrismStreamingVolume && value is not BoxStreamingVolume)
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
            int selected = EditorGUI.Popup(position,
                label ?? new GUIContent("Region Shape"), current, options);
            if (!EditorGUI.EndChangeCheck() || selected == current)
                return;

            Bounds bounds = value is StreamingVolume currentVolume
                ? currentVolume.BroadphaseBounds
                : initialBounds;
            volume.managedReferenceValue = selected switch
            {
                0 => new BoxStreamingVolume(bounds),
                1 => new PolygonPrismStreamingVolume(bounds, PolygonPrismPlane.XZ),
                _ => value
            };
            volume.isExpanded = selected == 1;
        }

        private static bool DrawExtrusionHandles(PolygonPrismStreamingVolume prism,
            Object undoTarget, string undoName)
        {
            if (prism.Vertices.Count == 0)
                return false;

            Bounds bounds = prism.BroadphaseBounds;
            Vector3 axis = prism.ProjectionPlane == PolygonPrismPlane.XY
                ? Vector3.forward
                : Vector3.up;
            Vector3 minimum = bounds.center;
            Vector3 maximum = bounds.center;
            if (prism.ProjectionPlane == PolygonPrismPlane.XY)
            {
                minimum.z = prism.MinExtrusion;
                maximum.z = prism.MaxExtrusion;
            }
            else
            {
                minimum.y = prism.MinExtrusion;
                maximum.y = prism.MaxExtrusion;
            }

            Color previousColor = Handles.color;
            Handles.color = Color.cyan;
            Handles.DrawDottedLine(minimum, maximum, 3f);
            bool changed = false;

            EditorGUI.BeginChangeCheck();
            Vector3 movedMinimum = Handles.Slider(
                minimum, axis, HandleUtility.GetHandleSize(minimum) * 0.08f,
                Handles.ConeHandleCap, 0f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(undoTarget, undoName);
                float value = prism.ProjectionPlane == PolygonPrismPlane.XY
                    ? movedMinimum.z
                    : movedMinimum.y;
                prism.MinExtrusion = Mathf.Min(value, prism.MaxExtrusion - 0.01f);
                EditorUtility.SetDirty(undoTarget);
                changed = true;
            }

            EditorGUI.BeginChangeCheck();
            Vector3 movedMaximum = Handles.Slider(
                maximum, axis, HandleUtility.GetHandleSize(maximum) * 0.08f,
                Handles.ConeHandleCap, 0f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(undoTarget, undoName);
                float value = prism.ProjectionPlane == PolygonPrismPlane.XY
                    ? movedMaximum.z
                    : movedMaximum.y;
                prism.MaxExtrusion = Mathf.Max(value, prism.MinExtrusion + 0.01f);
                EditorUtility.SetDirty(undoTarget);
                changed = true;
            }

            string axisName = prism.ProjectionPlane == PolygonPrismPlane.XY ? "Z" : "Y";
            Handles.Label(minimum, $"Min {axisName}  {prism.MinExtrusion:0.##}");
            Handles.Label(maximum, $"Max {axisName}  {prism.MaxExtrusion:0.##}");
            Handles.color = previousColor;
            return changed;
        }

        private static Vector3 ToWorld(PolygonPrismStreamingVolume prism, Vector2 vertex, float extrusion)
        {
            return prism.ProjectionPlane == PolygonPrismPlane.XY
                ? new Vector3(vertex.x, vertex.y, extrusion)
                : new Vector3(vertex.x, extrusion, vertex.y);
        }

        private static Vector2 FromWorld(PolygonPrismStreamingVolume prism, Vector3 point)
        {
            return prism.ProjectionPlane == PolygonPrismPlane.XY
                ? new Vector2(point.x, point.y)
                : new Vector2(point.x, point.z);
        }
    }
}
