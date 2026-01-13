using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public sealed class AlwaysEditableAttribute : Attribute
{
}

public abstract class LockedInspectorEditor<T> : Editor where T : MonoBehaviour
{
    private bool _isEditing;
    private int _instanceId;

    protected bool IsEditing => _isEditing;

    protected virtual void OnEnable()
    {
        _instanceId = target.GetInstanceID();
        _isEditing = false;
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    protected virtual void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

        if (_isEditing)
            SetEditMode(false);
    }

    private void OnSelectionChanged()
    {
        if (Selection.activeInstanceID != _instanceId)
        {
            SetEditMode(false);
            Repaint();
        }
    }

    public sealed override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawEditButton();
        DrawInspectorBody();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEditButton()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            GUILayout.FlexibleSpace();

            bool newState = GUILayout.Toggle(_isEditing, "Edit", EditorStyles.miniButton, GUILayout.Width(60));

            if (newState != _isEditing)
                SetEditMode(newState);
        }
    }

    private void SetEditMode(bool enabled)
    {
        if (_isEditing == enabled)
            return;

        _isEditing = enabled;
        OnEditModeChanged(_isEditing);
    }

    private void DrawInspectorBody()
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
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(iterator, true);

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    OnPropertyValueChanged(iterator);
                }
            }
        }
    }

    private bool IsAlwaysEditable(SerializedProperty property)
    {
        FieldInfo field = GetFieldInfo(property);
        return field != null && Attribute.IsDefined(field, typeof(AlwaysEditableAttribute));
    }

    private static FieldInfo GetFieldInfo(SerializedProperty property)
    {
        Type type = property.serializedObject.targetObject.GetType();
        FieldInfo field = null;

        foreach (string part in property.propertyPath.Split('.'))
        {
            field = type.GetField(part, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field == null)
                return null;

            type = field.FieldType;
        }

        return field;
    }

    protected virtual void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode && _isEditing)
        {
            SetEditMode(false);
        }
    }


    protected virtual void OnPropertyValueChanged(SerializedProperty property) { }

    protected virtual void OnEditModeChanged(bool isEditing) { }
}
