using UnityEditor;

[CustomEditor(typeof(ObjectGuid))]
public class ObjectGuidDrawer : Editor
{
    private ObjectGuid _objectGuidComp;

    public override void OnInspectorGUI()
    {
        if (_objectGuidComp == null)
            _objectGuidComp = (ObjectGuid)target;
        // Draw label
        EditorGUILayout.LabelField("Guid:", _objectGuidComp.GetGuid().ToString());
    }
}