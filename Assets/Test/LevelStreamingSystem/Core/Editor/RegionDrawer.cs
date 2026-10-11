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
            var volumeProp = property.FindPropertyRelative("volume");
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

            // Exact streaming shape
            height += PolygonPrismEditorUtility.GetShapeFieldHeight(volumeProp) + space;

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
            var volumeProp = property.FindPropertyRelative("volume");
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

            float shapeHeight = PolygonPrismEditorUtility.GetShapeFieldHeight(volumeProp);
            PolygonPrismEditorUtility.DrawShapeField(
                new Rect(rect.x, rect.y, rect.width, shapeHeight),
                volumeProp,
                boundsProp.boundsValue);
            rect.y += shapeHeight + space;

            // Portals
            float portalsHeight = EditorGUI.GetPropertyHeight(portalsProp, true);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, portalsHeight), portalsProp, true);
            rect.y += portalsHeight + space;

            // Unload Strategy
            EditorGUI.PropertyField(rect, unloadProp);

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
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

    [CustomPropertyDrawer(typeof(RegionManager.Portal))]
    internal sealed class PortalDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty target = RegionEditorCommands.FindPortalTargetProperty(property);
            SerializedProperty bounds = RegionEditorCommands.FindPortalBoundsProperty(property);
            SerializedProperty volume = RegionEditorCommands.FindPortalVolumeProperty(property);

            float height = line + spacing;
            height += EditorGUI.GetPropertyHeight(target, true) + spacing;
            height += PolygonPrismEditorUtility.GetShapeFieldHeight(volume);
            if (volume?.managedReferenceValue is PolygonPrismStreamingVolume)
                height += spacing + EditorGUIUtility.singleLineHeight * 2f;
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
            SerializedProperty target = RegionEditorCommands.FindPortalTargetProperty(property);
            SerializedProperty bounds = RegionEditorCommands.FindPortalBoundsProperty(property);
            SerializedProperty volume = RegionEditorCommands.FindPortalVolumeProperty(property);

            row.y += line + spacing;
            float targetHeight = EditorGUI.GetPropertyHeight(target, true);
            EditorGUI.PropertyField(new Rect(row.x, row.y, row.width, targetHeight),
                target, new GUIContent("Target Region Name"), true);
            row.y += targetHeight + spacing;

            Bounds initialWorldBounds = bounds.boundsValue;
            initialWorldBounds.center += FindRegionOrigin(property);
            float shapeHeight = PolygonPrismEditorUtility.GetShapeFieldHeight(volume);
            PolygonPrismEditorUtility.DrawShapeField(
                new Rect(row.x, row.y, row.width, shapeHeight),
                volume,
                initialWorldBounds,
                new GUIContent("Portal Shape"));
            row.y += shapeHeight;

            if (volume?.managedReferenceValue is PolygonPrismStreamingVolume)
            {
                row.y += spacing;
                EditorGUI.HelpBox(
                    new Rect(row.x, row.y, row.width, line * 2f),
                    "Portal prism coordinates are world-space.",
                    MessageType.Info);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        private static Vector3 FindRegionOrigin(SerializedProperty portal)
        {
            if (portal.serializedObject.targetObject is not RegionManager manager)
                return Vector3.zero;

            string path = portal.propertyPath;
            int regionToken = path.IndexOf("regions.Array.data[", System.StringComparison.Ordinal);
            if (regionToken < 0)
                return Vector3.zero;
            int start = regionToken + "regions.Array.data[".Length;
            int end = path.IndexOf(']', start);
            if (end <= start || !int.TryParse(path.Substring(start, end - start), out int index) ||
                index < 0 || index >= manager.Regions.Count || manager.Regions[index] == null)
                return Vector3.zero;
            return manager.Regions[index].Origin;
        }
    }
}
