#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(SceneBoundsManager))]
public class SceneBoundsManagerEditor : Editor
{
    private BoxBoundsHandle boundsHandle = new BoxBoundsHandle();

    private void OnEnable()
    {
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        // Draw for all active managers in the scene, not only the selected one
        var managers = Object.FindObjectsOfType<SceneBoundsManager>();
        foreach (var manager in managers)
        {
            if (manager.Scenes == null) continue;

            foreach (var sceneRef in manager.Scenes)
            {
                if (sceneRef.sceneAsset == null) continue;

                var bounds = sceneRef.cachedBounds;

                // Setup handle
                boundsHandle.center = bounds.center;
                boundsHandle.size = bounds.size;

                EditorGUI.BeginChangeCheck();
                boundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(manager, "Modify Scene Bound");
                    sceneRef.cachedBounds = new Bounds(boundsHandle.center, boundsHandle.size);

                    // Push changes back into additive scene
                    manager.ApplyBoundsToScene(sceneRef);
                    EditorUtility.SetDirty(manager);
                }

                // Draw label above bounds
                Handles.Label(bounds.center + Vector3.up * bounds.extents.y,
                    sceneRef.sceneAsset.name,
                    EditorStyles.boldLabel);
            }
        }
    }
}
#endif