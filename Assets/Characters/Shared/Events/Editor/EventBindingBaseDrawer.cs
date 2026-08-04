using UnityEditor;
using UnityEngine;
using System;

[CustomPropertyDrawer(typeof(EventHook), true)]
public class EventBindingBaseDrawer : PropertyDrawer
{
    private static readonly Type[] DerivedTypes =
    {
        typeof(VoidEventHook),
        typeof(BoolEventHook),
        typeof(IntEventHook),
        typeof(Vector3EventHook)
    };

    private static readonly string[] TypeNames = Array.ConvertAll(DerivedTypes,
        t => t.Name.Replace("EventHook", ""));

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Foldout with label (keeps foldout state per property path)
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded, label, true);

        if (property.isExpanded)
        {
            // Type selection dropdown
            var dropdownRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2,
                position.width, EditorGUIUtility.singleLineHeight);
            var currentType = property.managedReferenceValue?.GetType();
            var currentIndex = currentType != null ? Array.IndexOf(DerivedTypes, currentType) : -1;

            var newIndex = EditorGUI.Popup(dropdownRect, "Payload Type", currentIndex, TypeNames);
            HandleTypeSelection(property, newIndex, currentIndex);

            if (property.managedReferenceValue != null)
            {
                var contentY = dropdownRect.y + EditorGUIUtility.singleLineHeight + 2;
                var contentRect = new Rect(position.x, contentY, position.width, 0);
                DrawChildProperties(contentRect, property);
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight; // Foldout

        if (!property.isExpanded)
            return height;

        height += EditorGUIUtility.singleLineHeight + 2; // Dropdown

        if (property.managedReferenceValue != null)
        {
            ForEachChildProperty(property, child =>
                height += EditorGUI.GetPropertyHeight(child, true) + 2);
        }

        return height;
    }

    private void HandleTypeSelection(SerializedProperty property, int newIndex, int currentIndex)
    {
        if (newIndex == currentIndex || newIndex < 0) return;

        property.serializedObject.Update();
        property.managedReferenceValue = Activator.CreateInstance(DerivedTypes[newIndex]);
        property.serializedObject.ApplyModifiedProperties();
    }

    private void DrawChildProperties(Rect position, SerializedProperty property)
    {
        ForEachChildProperty(property, child =>
        {
            position.height = EditorGUI.GetPropertyHeight(child);
            EditorGUI.PropertyField(position, child, true);
            position.y += position.height + 2;
        });
    }

    private static void ForEachChildProperty(SerializedProperty parent, Action<SerializedProperty> action)
    {
        var iterator = parent.Copy();
        var enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            if (!iterator.propertyPath.StartsWith(parent.propertyPath)) break;
            action(iterator);
            enterChildren = false;
        }
    }
}