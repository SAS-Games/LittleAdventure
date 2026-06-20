using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnityEngine.Object), true)]
[CanEditMultipleObjects]
public class LockedInspectorEditor : Editor
{
    private bool _isEditing;

    public override void OnInspectorGUI()
    {
        if (!HasLockedInspectorAttribute())
        {
            DrawDefaultInspector();
            return;
        }

        serializedObject.Update();

        DrawEditButton();
        DrawProperties();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEditButton()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            GUILayout.FlexibleSpace();
            bool newState = GUILayout.Toggle(_isEditing, "Edit", EditorStyles.miniButton, GUILayout.Width(60));
            if (newState != _isEditing)
                _isEditing = newState;
        }
    }

    private void DrawProperties()
    {
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (iterator.propertyPath == "m_Script")
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.PropertyField(iterator);
                continue;
            }

            bool editable = _isEditing || IsAlwaysEditable(iterator);

            using (new EditorGUI.DisabledScope(!editable))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
        }
    }

    private bool HasLockedInspectorAttribute()
    {
        return System.Attribute.IsDefined(target.GetType(), typeof(LockedInspectorAttribute));
    }

    private bool IsAlwaysEditable(SerializedProperty property)
    {
        var field = ReflectionUtility.GetFieldInfo(property);
        return field != null && System.Attribute.IsDefined(field, typeof(AlwaysEditableAttribute));
    }
}