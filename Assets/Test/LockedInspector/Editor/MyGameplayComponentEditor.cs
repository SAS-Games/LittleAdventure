using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MyGameplayComponent))]
public class MyGameplayComponentEditor: LockedInspectorEditor<MyGameplayComponent>
{
    private GameObject _helper;

    protected override void OnEditModeChanged(bool isEditing)
    {
        if (isEditing)
            CreateHelper();
        else
            DestroyHelper();
    }

    private void CreateHelper()
    {
        if (_helper != null)
            return;

        _helper = new GameObject("MyGameplayComponent (Edit Helper)");
        _helper.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;

        var targetComp = (MyGameplayComponent)target;
        _helper.transform.position = targetComp.transform.position;
    }

    private void DestroyHelper()
    {
        if (_helper == null)
            return;

        DestroyImmediate(_helper);
        _helper = null;
    }
}
