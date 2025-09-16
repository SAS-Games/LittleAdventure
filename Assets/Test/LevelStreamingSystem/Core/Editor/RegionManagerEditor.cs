using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LevelStreaming.Editor
{
    [CustomEditor(typeof(RegionManager))]
    public class RegionManagerEditor : UnityEditor.Editor
    {
        private BoxBoundsHandle regionHandle = new BoxBoundsHandle();
        private BoxBoundsHandle portalHandle = new BoxBoundsHandle();

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

                    DrawRegionBounds(manager, region);
                    DrawRegionPortals(manager, region);
                }
            }
        }

        private void DrawRegionBounds(RegionManager manager, RegionManager.Region region)
        {
            var bounds = region.CachedBounds;

            regionHandle.center = bounds.center;
            regionHandle.size = bounds.size;

            EditorGUI.BeginChangeCheck();
            regionHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(manager, "Modify Region Bound");
                region.CachedBounds = new Bounds(regionHandle.center, regionHandle.size);
                manager.ApplyBounds(region);
                EditorUtility.SetDirty(manager);
            }

            string label = region.Type switch
            {
                RegionManager.RegionType.Scene when region.SceneRef?.SceneAsset != null => region.SceneRef.SceneAsset
                    .name,
                RegionManager.RegionType.Prefab when region.PrefabRef != null => region.PrefabRef.RuntimeKey.ToString(),
                _ => region.RegionName
            };

            if (!string.IsNullOrEmpty(label))
            {
                Handles.Label(bounds.center + Vector3.up * bounds.extents.y, label, EditorStyles.boldLabel);
            }
        }

        private void DrawRegionPortals(RegionManager manager, RegionManager.Region region)
        {
            if (region.Portals == null || region.Portals.Count == 0) return;

            for (int i = 0; i < region.Portals.Count; i++)
            {
                var portal = region.Portals[i];
                var worldCenter = region.CachedBounds.center + portal.LocalBounds.center;

                portalHandle.center = worldCenter;
                portalHandle.size = portal.LocalBounds.size;

                EditorGUI.BeginChangeCheck();
                portalHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(manager, "Modify Portal Bound");
                    var newLocalCenter = portalHandle.center - region.CachedBounds.center;
                    portal.LocalBounds = new Bounds(newLocalCenter, portalHandle.size);
                    region.RebuildPortalWorldBounds();
                    EditorUtility.SetDirty(manager);
                }

                Handles.Label(worldCenter + Vector3.up * portal.LocalBounds.extents.y, $"Portal {i}",
                    EditorStyles.miniBoldLabel);
            }
        }
    }
}
