using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

public partial class RegionManager
{
    public partial class Region
    {
        public void OnValidate()
        {
            switch (Type)
            {
                case RegionType.Scene:
                    if (SceneRef != null && SceneRef.SceneAsset != null)
                        RegionName = SceneRef.SceneAsset.name;
                    else
                        RegionName = string.Empty;
                    break;

                case RegionType.Prefab:
                    if (PrefabRef != null)
                    {
                        RegionName = PrefabRef.editorAsset != null 
                            ? PrefabRef.editorAsset.name 
                            : PrefabRef.RuntimeKey.ToString();
                    }
                    else
                        RegionName = string.Empty;
                    break;
            }
        }
    }

    public void ApplyBounds(Region region)
    {
        switch (region.Type)
        {
            case RegionType.Scene:
                if (region.SceneRef == null) return;

                string scenePath = AssetDatabase.GetAssetPath(region.SceneRef.SceneAsset);
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                foreach (var go in scene.GetRootGameObjects())
                {
                    var sb = go.GetComponentInChildren<RegionBound>();
                    if (sb != null)
                    {
                        Undo.RecordObject(sb, "Update Scene Bound");
                        sb.Bounds = new Bounds(
                            sb.transform.InverseTransformPoint(region.CachedBounds.center),
                            region.CachedBounds.size
                        );
                        EditorUtility.SetDirty(sb);
                        break;
                    }
                }

                EditorSceneManager.SaveScene(scene);
                EditorSceneManager.CloseScene(scene, true);
                break;

            case RegionType.Prefab:
                if (region.PrefabRef == null) return;

                string prefabPath = AssetDatabase.GUIDToAssetPath(region.PrefabRef.AssetGUID);
                if (string.IsNullOrEmpty(prefabPath)) return;

                var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

                var pb = prefabRoot.GetComponentInChildren<RegionBound>();
                if (pb != null)
                {
                    Undo.RecordObject(pb, "Update Prefab Bound");
                    pb.Bounds = new Bounds(
                        pb.transform.InverseTransformPoint(region.CachedBounds.center),
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
            switch (region.Type)
            {
                case RegionType.Scene:
                    if (region.SceneRef == null) continue;
                    string scenePath = AssetDatabase.GetAssetPath(region.SceneRef.SceneAsset);
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                    foreach (var go in scene.GetRootGameObjects())
                    {
                        var sb = go.GetComponentInChildren<RegionBound>();
                        if (sb != null)
                        {
                            region.CachedBounds = new Bounds(
                                sb.transform.TransformPoint(sb.Bounds.center),
                                sb.Bounds.size
                            );
                            break;
                        }
                    }

                    EditorSceneManager.CloseScene(scene, true);
                    break;

                case RegionType.Prefab:
                    if (region.PrefabRef == null) continue;
                    string prefabPath = AssetDatabase.GUIDToAssetPath(region.PrefabRef.AssetGUID);
                    var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

                    var pb = prefabRoot.GetComponentInChildren<RegionBound>();
                    if (pb != null)
                    {
                        region.CachedBounds = new Bounds(
                            pb.transform.TransformPoint(pb.Bounds.center),
                            pb.Bounds.size
                        );
                    }

                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                    break;
            }
        }
    }

    private void OnValidate()
    {
        foreach (var region in Regions)
        {
            region.OnValidate();
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var region in Regions)
        {
            if (region == null) continue;

            bool isLoaded = false;
            //todo: need to implement this funcionality
            // switch (region.regionType)
            // {
            //     case RegionType.Scene:
            //         if (region.sceneRef == null) continue;
            //         isLoaded = _loadedSceneNames.Contains(region.sceneRef.SceneAsset.name);
            //         break;
            //
            //     case RegionType.Prefab:
            //         if (region.prefabAddress == null) continue;
            //         // isLoaded = _activePrefabInstances.ContainsKey(region.RegionName);
            //         break;
            // }

            Color wireColor = isLoaded ? Color.green : Color.cyan;
            Color fillColor = wireColor;
            fillColor.a = 0.1f;

            Gizmos.color = fillColor;
            Gizmos.DrawCube(region.CachedBounds.center, region.CachedBounds.size);

            Gizmos.color = wireColor;
            Gizmos.DrawWireCube(region.CachedBounds.center, region.CachedBounds.size);
        }
    }
}