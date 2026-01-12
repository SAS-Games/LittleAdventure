using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
    }

    protected virtual void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;

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

        var alwaysEditable = GetAlwaysEditableProperties();

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (iterator.propertyPath == "m_Script")
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.PropertyField(iterator);
                continue;
            }

            bool editable = _isEditing || alwaysEditable.Contains(iterator.name);

            using (new EditorGUI.DisabledScope(!editable))
                EditorGUILayout.PropertyField(iterator, true);
        }
    }

    protected virtual HashSet<string> GetAlwaysEditableProperties()
    {
        return new HashSet<string>();
    }

    protected virtual void OnEditModeChanged(bool isEditing) { }
}
