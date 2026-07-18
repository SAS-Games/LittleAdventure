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
            var typeProp = property.FindPropertyRelative("type");
            SerializedProperty sourceProp = GetSourceProperty(property,
                (RegionManager.RegionType)typeProp.enumValueIndex);

            float line = EditorGUIUtility.singleLineHeight;
            float space = EditorGUIUtility.standardVerticalSpacing;

            float height = 0;

            // Foldout
            height += line + space;

            // Name + Type
            height += (line + space) * 2;

            // Scene, Prefab, or Addressable Scene
            height += EditorGUI.GetPropertyHeight(sourceProp, true) + space;

            // Bounds
            height += EditorGUI.GetPropertyHeight(boundsProp, true) + space;

            // Fit/apply buttons
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
            var addressableSceneProp = property.FindPropertyRelative("addressableSceneRef");
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

            SerializedProperty sourceProp = type switch
            {
                RegionManager.RegionType.Scene => sceneProp,
                RegionManager.RegionType.Prefab => prefabProp,
                RegionManager.RegionType.AddressableScene => addressableSceneProp,
                _ => null
            };

            if (sourceProp != null)
            {
                float sourceHeight = EditorGUI.GetPropertyHeight(sourceProp, true);
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, sourceHeight), sourceProp, true);
                rect.y += sourceHeight + space;
            }

            // Cached Bounds
            float boundsHeight = EditorGUI.GetPropertyHeight(boundsProp, true);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, boundsHeight), boundsProp, true);
            rect.y += boundsHeight + space;
            float buttonGap = 4f;
            float buttonWidth = (rect.width - buttonGap) * 0.5f;
            Rect refreshButton = new Rect(rect.x, rect.y, buttonWidth, line);
            Rect applyButton = new Rect(rect.x + buttonWidth + buttonGap, rect.y, buttonWidth, line);

            if (GUI.Button(refreshButton, "Fit From Asset"))
            {
                property.serializedObject.ApplyModifiedProperties();
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

                property.serializedObject.Update();
            }

            if (GUI.Button(applyButton, "Apply To Asset"))
            {
                property.serializedObject.ApplyModifiedProperties();
                var manager = property.serializedObject.targetObject as RegionManager;
                int index = GetRegionIndex(property);
                if (manager != null && index >= 0 && index < manager.Regions.Count &&
                    EditorUtility.DisplayDialog(
                        "Apply Streaming Bounds",
                        "This writes the cached bounds into the source scene or prefab.",
                        "Apply",
                        "Cancel"))
                    manager.ApplyBounds(manager.Regions[index]);
                property.serializedObject.Update();
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

        private static SerializedProperty GetSourceProperty(SerializedProperty property,
            RegionManager.RegionType type)
        {
            return type switch
            {
                RegionManager.RegionType.Scene => property.FindPropertyRelative("sceneRef"),
                RegionManager.RegionType.Prefab => property.FindPropertyRelative("prefabRef"),
                RegionManager.RegionType.AddressableScene => property.FindPropertyRelative("addressableSceneRef"),
                _ => property.FindPropertyRelative("sceneRef")
            };
        }
    }
}
