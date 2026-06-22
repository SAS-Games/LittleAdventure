using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FlowFieldSurface))]
public class FlowFieldSurfaceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        FlowFieldSurface surface =
            (FlowFieldSurface)target;

        if (GUILayout.Button("Bake"))
        {
            surface.Bake();
        }
    }
}