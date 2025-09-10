#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(RegionManager))]
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
        var managers = Object.FindObjectsOfType<RegionManager>();
        foreach (var manager in managers)
        {
            if (manager.Regions == null) continue;

            foreach (var region in manager.Regions)
            {
                if (region.sceneRef == null) continue;

                var bounds = region.cachedBounds;

                // Setup handle
                boundsHandle.center = bounds.center;
                boundsHandle.size = bounds.size;

                EditorGUI.BeginChangeCheck();
                boundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(manager, "Modify Scene Bound");
                    region.cachedBounds = new Bounds(boundsHandle.center, boundsHandle.size);

                    // Push changes back into additive scene
                    manager.ApplyBounds(region);
                    EditorUtility.SetDirty(manager);
                }

                // Draw label above bounds
                if (region.sceneRef?.SceneAsset != null)
                    Handles.Label(bounds.center + Vector3.up * bounds.extents.y, region.sceneRef.SceneAsset.name,
                        EditorStyles.boldLabel);
            }
        }
    }
}
#endif