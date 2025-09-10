#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(RegionManager))]
public class RegionManagerEditor : Editor
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
        var managers = Object.FindObjectsOfType<RegionManager>();
        foreach (var manager in managers)
        {
            if (manager.Regions == null) continue;

            foreach (var region in manager.Regions)
            {
                if (region == null) continue;

                var bounds = region.CachedBounds;

                // Setup handle
                boundsHandle.center = bounds.center;
                boundsHandle.size = bounds.size;

                EditorGUI.BeginChangeCheck();
                boundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(manager, "Modify Region Bound");
                    region.CachedBounds = new Bounds(boundsHandle.center, boundsHandle.size);

                    manager.ApplyBounds(region);
                    EditorUtility.SetDirty(manager);
                }

                // Label (Scene or Prefab)
                string label = region.Type switch
                {
                    RegionManager.RegionType.Scene when region.SceneRef?.SceneAsset != null => region.SceneRef.SceneAsset.name,
                    RegionManager.RegionType.Prefab when region.PrefabRef != null           => region.PrefabRef.RuntimeKey.ToString(),
                    _ => region.RegionName
                };

                if (!string.IsNullOrEmpty(label))
                {
                    Handles.Label(bounds.center + Vector3.up * bounds.extents.y, label, EditorStyles.boldLabel);
                }
            }
        }
    }
}
#endif
