using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public partial class RegionManager
{
    public partial class Region
    {
        public SceneAsset sceneAsset;

        public void OnValidate()
        {
            if (sceneAsset != null)
                RegionName = sceneAsset.name;
        }
    }

    [ContextMenu("Refresh Bounds From Scenes")]
    public void RefreshBounds()
    {
        foreach (var sceneRef in Scenes)
        {
            if (sceneRef.sceneAsset == null) continue;

            string scenePath = AssetDatabase.GetAssetPath(sceneRef.sceneAsset);
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            foreach (var go in scene.GetRootGameObjects())
            {
                var sb = go.GetComponentInChildren<RegionBound>();
                if (sb != null)
                {
                    sceneRef.cachedBounds = new Bounds(
                        sb.transform.TransformPoint(sb.Bounds.center),
                        sb.Bounds.size
                    );
                    break;
                }
            }

            EditorSceneManager.CloseScene(scene, true);
        }
    }

    public void ApplyBoundsToScene(Region region)
    {
        if (region.sceneAsset == null) return;

        string scenePath = AssetDatabase.GetAssetPath(region.sceneAsset);
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

        foreach (var go in scene.GetRootGameObjects())
        {
            var sb = go.GetComponentInChildren<RegionBound>();
            if (sb != null)
            {
                Undo.RecordObject(sb, "Update Scene Bound");
                sb.Bounds = new Bounds(
                    sb.transform.InverseTransformPoint(region.cachedBounds.center),
                    region.cachedBounds.size
                );
                EditorUtility.SetDirty(sb);
                break;
            }
        }

        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.CloseScene(scene, true);
    }

    private void OnValidate()
    {
        foreach (var sceneRef in Scenes)
        {
            sceneRef.OnValidate();
        }
    }
    
    private void OnDrawGizmos()
    {
        foreach (var s in Scenes)
        {
            if (s.sceneAsset == null) continue;

            bool isLoaded = _loadedSceneNames.Contains(s.sceneAsset.name);
            Color wireColor   = isLoaded ? Color.green : Color.cyan;
            Color fillColor   = wireColor; 
            fillColor.a       = 0.1f; 
            Gizmos.color = fillColor;
            Gizmos.DrawCube(s.cachedBounds.center, s.cachedBounds.size);
            Gizmos.color = wireColor;
            Gizmos.DrawWireCube(s.cachedBounds.center, s.cachedBounds.size);
        }
    }
}