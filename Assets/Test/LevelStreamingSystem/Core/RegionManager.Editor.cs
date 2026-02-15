#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LevelStreaming
{
    public partial class RegionManager
    {
        public partial class Region
        {
            public void OnValidate()
            {
                switch (Type)
                {
                    case RegionType.Scene:
                        if (SceneRef != null)
                        {
                            if (string.IsNullOrEmpty(regionName))
                            {
                                regionName = SceneRef.SceneAsset != null ? SceneRef.SceneAsset.name : string.Empty;
                            }
                        }
                        else
                            regionName = string.Empty;

                        break;

                    case RegionType.Prefab:
                        if (PrefabRef != null)
                        {
                            if (!string.IsNullOrEmpty(PrefabRef.AssetGUID))
                            {
                                // Resolve prefab by GUID
                                string path = AssetDatabase.GUIDToAssetPath(PrefabRef.AssetGUID);
                                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                                regionName = prefab != null ? prefab.name : PrefabRef.RuntimeKey.ToString();
                            }
                            else
                            {
                                // Fall back to editorAsset if GUID missing
                                regionName = PrefabRef.editorAsset != null ? PrefabRef.editorAsset.name : string.Empty;
                            }
                        }
                        else
                            regionName = string.Empty;

                        break;
                }

                if (CachedBounds.size == Vector3.zero)
                    CachedBounds = new Bounds(Vector3.zero, Vector3.one * 2);
            }
        }

        public void ApplyBounds(Region region)
        {
            switch (region.Type)
            {
                case RegionType.Scene:
                    if (region.SceneRef == null || region.SceneRef.SceneAsset == null) return;

                    string scenePath = AssetDatabase.GetAssetPath(region.SceneRef.SceneAsset);
                    var targetScene = EditorSceneManager.GetSceneByPath(scenePath);

                    bool wasOpen = targetScene.isLoaded;
                    if (!wasOpen)
                        targetScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                    foreach (var go in targetScene.GetRootGameObjects())
                    {
                        var sb = go.GetComponentInChildren<RegionBound>();
                        if (sb != null)
                        {
                            Undo.RecordObject(sb, "Update Scene Bound");
                            sb.Bounds = new Bounds(sb.transform.InverseTransformPoint(region.CachedBounds.center),
                                region.CachedBounds.size);
                            EditorUtility.SetDirty(sb);
                            break;
                        }
                    }

                    if (!wasOpen)
                    {
                        EditorSceneManager.SaveScene(targetScene);
                        EditorSceneManager.CloseScene(targetScene, true);
                    }
                    else
                        EditorSceneManager.MarkSceneDirty(targetScene);

                    break;

                case RegionType.Prefab:
                    if (region.PrefabRef == null || string.IsNullOrEmpty(region.PrefabRef.AssetGUID)) return;

                    string prefabPath = AssetDatabase.GUIDToAssetPath(region.PrefabRef.AssetGUID);
                    if (string.IsNullOrEmpty(prefabPath)) return;

                    var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                    var pb = prefabRoot.GetComponentInChildren<RegionBound>();
                    if (pb != null)
                    {
                        Undo.RecordObject(pb, "Update Prefab Bound");
                        pb.Bounds = new Bounds(pb.transform.InverseTransformPoint(region.CachedBounds.center),
                            region.CachedBounds.size
                        );
                        EditorUtility.SetDirty(pb);
                    }

                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                    break;
            }
        }

        [ContextMenu("Refresh Bounds From Assets")]
        public void RefreshBounds()
        {
            foreach (var region in Regions)
            {
                RefreshBounds(region);
            }
        }

        public void RefreshBounds(Region region)
        {
            if (region == null)
                return;

            region.OnValidate();

            switch (region.Type)
            {
                case RegionType.Scene:
                {
                    if (region.SceneRef == null ||
                        region.SceneRef.SceneAsset == null)
                        return;

                    string scenePath =
                        AssetDatabase.GetAssetPath(region.SceneRef.SceneAsset);

                    var targetScene =
                        EditorSceneManager.GetSceneByPath(scenePath);

                    bool wasOpen = targetScene.isLoaded;

                    if (!wasOpen)
                        targetScene = EditorSceneManager.OpenScene(
                            scenePath,
                            OpenSceneMode.Additive);

                    foreach (var go in targetScene.GetRootGameObjects())
                    {
                        var sb = go.GetComponentInChildren<RegionBound>();
                        if (sb != null)
                        {
                            region.CachedBounds =
                                new Bounds(
                                    sb.transform.TransformPoint(sb.Bounds.center),
                                    sb.Bounds.size);
                            break;
                        }
                    }

                    if (!wasOpen)
                        EditorSceneManager.CloseScene(targetScene, true);

                    break;
                }

                case RegionType.Prefab:
                {
                    if (region.PrefabRef == null ||
                        string.IsNullOrEmpty(region.PrefabRef.AssetGUID))
                        return;

                    string prefabPath =
                        AssetDatabase.GUIDToAssetPath(region.PrefabRef.AssetGUID);

                    var prefabRoot =
                        PrefabUtility.LoadPrefabContents(prefabPath);

                    var pb = prefabRoot.GetComponentInChildren<RegionBound>();

                    if (pb != null)
                    {
                        region.CachedBounds =
                            new Bounds(
                                pb.transform.TransformPoint(pb.Bounds.center),
                                pb.Bounds.size);
                    }

                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                    break;
                }
            }

            EditorUtility.SetDirty(this);
        }

        [MenuItem("Tools/Streaming/Apply Bounds For All Regions")]
        private static void ApplyBoundsForAllRegions()
        {
            var manager = Object.FindFirstObjectByType<RegionManager>();
            if (manager == null)
            {
                Debug.LogWarning("No RegionManager found in the scene.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(manager.gameObject, "Apply Bounds For All Regions");

            foreach (var region in manager.Regions)
            {
                if (region != null)
                {
                    region.OnValidate(); // ensure scene/prefab references are correct
                    manager.ApplyBounds(region);
                }
            }

            Debug.Log("Applied bounds for all regions.");
        }

        [MenuItem("Tools/Streaming/Refresh Bounds From Assets")]
        private static void RefreshBoundsMenu()
        {
            var manager = Object.FindFirstObjectByType<RegionManager>();
            if (manager == null)
            {
                Debug.LogWarning("No RegionManager found in the scene.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(manager.gameObject, "Refresh Bounds From Assets");

            manager.RefreshBounds();

            Debug.Log("Refreshed bounds for all regions.");
        }

        private void OnValidate()
        {
            if (Regions == null) return;

            foreach (var region in Regions)
            {
                region?.OnValidate();
            }
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Regions == null) return;

            foreach (var region in Regions)
            {
                if (region == null) continue;
                bool isLoaded = IsRegionLoaded(region);
                Color wireColor = isLoaded ? Color.green : Color.cyan;
                Color fillColor = wireColor;
                fillColor.a = 0.1f;

                var bounds = region.CachedBounds;

                // --- Region bounds ---
                Gizmos.color = fillColor;
                Gizmos.DrawCube(bounds.center, bounds.size);

                Gizmos.color = wireColor;
                Gizmos.DrawWireCube(bounds.center, bounds.size);

                string label = region.RegionName ?? region.Type.ToString();
                GUIStyle regionLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    normal = { textColor = Color.black },
                    alignment = TextAnchor.LowerCenter
                };
                Handles.Label(bounds.center + Vector3.up * bounds.extents.y, label, regionLabelStyle);

                // --- Portal bounds ---
                if (region.Portals != null)
                {
                    for (int i = 0; i < region.Portals.Count; i++)
                    {
                        var portal = region.Portals[i];
                        var worldCenter = bounds.center + portal.LocalBounds.center;

                        Color portalWire = Color.yellow;
                        Color portalFill = new Color(1f, 1f, 0f, 0.1f);

                        Gizmos.color = portalFill;
                        Gizmos.DrawCube(worldCenter, portal.LocalBounds.size);

                        Gizmos.color = portalWire;
                        Gizmos.DrawWireCube(worldCenter, portal.LocalBounds.size);

                        string pLabel = $"Portal {i}";
                        if (!string.IsNullOrEmpty(portal.TargetRegionName))
                            pLabel += $" → {portal.TargetRegionName}";

                        GUIStyle portalLabelStyle = new GUIStyle(EditorStyles.miniBoldLabel)
                        {
                            normal = { textColor = Color.black },
                            alignment = TextAnchor.MiddleCenter
                        };
                        Handles.Label(worldCenter + Vector3.up * portal.LocalBounds.extents.y, pLabel,
                            portalLabelStyle);
                    }
                }
            }
#endif
        }
    }
}
#endif