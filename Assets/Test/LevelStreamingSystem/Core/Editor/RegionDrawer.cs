using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    [CustomPropertyDrawer(typeof(RegionManager.Region))]
    public class RegionDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            var boundsProp = property.FindPropertyRelative("cachedBounds");
            var portalsProp = property.FindPropertyRelative("portals");

            float line = EditorGUIUtility.singleLineHeight;
            float space = EditorGUIUtility.standardVerticalSpacing;

            float height = 0;

            // Foldout
            height += line + space;

            // Name + Type
            height += (line + space) * 2;

            // Scene or Prefab
            height += line + space;

            // Bounds
            height += EditorGUI.GetPropertyHeight(boundsProp, true) + space;

            // ✅ Refresh Button
            height += line + space;

            // Portals
            height += EditorGUI.GetPropertyHeight(portalsProp, true) + space;

            // UnloadStrategy
            height += line + space;

            return height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float line = EditorGUIUtility.singleLineHeight;
            float space = EditorGUIUtility.standardVerticalSpacing;

            Rect rect = new Rect(position.x, position.y, position.width, line);

            // Foldout
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            rect.y += line + space;

            var nameProp = property.FindPropertyRelative("regionName");
            var typeProp = property.FindPropertyRelative("type");
            var sceneProp = property.FindPropertyRelative("sceneRef");
            var prefabProp = property.FindPropertyRelative("prefabRef");
            var boundsProp = property.FindPropertyRelative("cachedBounds");
            var portalsProp = property.FindPropertyRelative("portals");
            var unloadProp = property.FindPropertyRelative("unloadStrategy");

            // Region Name
            EditorGUI.PropertyField(rect, nameProp);
            rect.y += line + space;

            // Type
            EditorGUI.PropertyField(rect, typeProp);
            rect.y += line + space;

            var type = (RegionManager.RegionType)typeProp.enumValueIndex;

            // Scene or Prefab
            if (type == RegionManager.RegionType.Scene)
            {
                EditorGUI.PropertyField(rect, sceneProp);
                rect.y += line + space;
            }
            else if (type == RegionManager.RegionType.Prefab)
            {
                EditorGUI.PropertyField(rect, prefabProp);
                rect.y += line + space;
            }

            // Cached Bounds
            float boundsHeight = EditorGUI.GetPropertyHeight(boundsProp, true);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, boundsHeight), boundsProp, true);
            rect.y += boundsHeight + space;
            Rect buttonRect = new Rect(rect.x, rect.y, rect.width, line);

            if (GUI.Button(buttonRect, "Refresh Bounds From Asset"))
            {
                var manager = property.serializedObject.targetObject as RegionManager;

                if (manager != null)
                {
                    Undo.RecordObject(manager, "Refresh Region Bounds");

                    // Find region index
                    int index = GetRegionIndex(property);

                    if (index >= 0 && index < manager.Regions.Count)
                    {
                        manager.RefreshBounds(manager.Regions[index]);
                    }

                    EditorUtility.SetDirty(manager);
                }
            }

            rect.y += line + space;
            // Portals
            float portalsHeight = EditorGUI.GetPropertyHeight(portalsProp, true);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, portalsHeight), portalsProp, true);
            rect.y += portalsHeight + space;

            // Unload Strategy
            EditorGUI.PropertyField(rect, unloadProp);

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
        
        private int GetRegionIndex(SerializedProperty property)
        {
            string path = property.propertyPath;

            int start = path.IndexOf('[') + 1;
            int end = path.IndexOf(']');

            if (start < 0 || end < 0)
                return -1;

            string indexStr = path.Substring(start, end - start);

            if (int.TryParse(indexStr, out int index))
                return index;

            return -1;
        }
    }
}