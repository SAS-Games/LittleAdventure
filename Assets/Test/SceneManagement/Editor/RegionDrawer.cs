using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RegionManager.Region))]
public class RegionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = 4; // Type, UnloadStrategy, RegionName
        var type = (RegionManager.RegionType)property.FindPropertyRelative("<Type>k__BackingField").enumValueIndex;

        if (type == RegionManager.RegionType.Scene)
            lines++; // SceneRef
        else if (type == RegionManager.RegionType.Prefab)
            lines++; // PrefabRef

        // Bounds is multi-line (2 Vector3s) → use proper height
        var boundsProp = property.FindPropertyRelative("<CachedBounds>k__BackingField");
        float boundsHeight = EditorGUI.GetPropertyHeight(boundsProp, true);

        // Normal line heights
        float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        return lineHeight * (lines - 1) + boundsHeight; // replace 1 line with full bounds height
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var typeProp = property.FindPropertyRelative("<Type>k__BackingField");
        var sceneProp = property.FindPropertyRelative("<SceneRef>k__BackingField");
        var prefabProp = property.FindPropertyRelative("<PrefabRef>k__BackingField");
        var boundsProp = property.FindPropertyRelative("<CachedBounds>k__BackingField");
        var unloadProp = property.FindPropertyRelative("<UnloadStrategy>k__BackingField");
        var nameProp = property.FindPropertyRelative("<RegionName>k__BackingField");

        var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // Type
        EditorGUI.PropertyField(rect, typeProp);
        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // Scene or Prefab field
        var type = (RegionManager.RegionType)typeProp.enumValueIndex;
        if (type == RegionManager.RegionType.Scene)
        {
            EditorGUI.PropertyField(rect, sceneProp);
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
        else if (type == RegionManager.RegionType.Prefab)
        {
            EditorGUI.PropertyField(rect, prefabProp);
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        // CachedBounds (multi-line with center + extents)
        float boundsHeight = EditorGUI.GetPropertyHeight(boundsProp, true);
        var boundsRect = new Rect(rect.x, rect.y, rect.width, boundsHeight);
        EditorGUI.PropertyField(boundsRect, boundsProp, true);
        rect.y += boundsHeight + EditorGUIUtility.standardVerticalSpacing;

        // UnloadStrategy
        EditorGUI.PropertyField(rect, unloadProp);
        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // RegionName (read-only)
        GUI.enabled = false;
        EditorGUI.PropertyField(rect, nameProp);
        GUI.enabled = true;

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
