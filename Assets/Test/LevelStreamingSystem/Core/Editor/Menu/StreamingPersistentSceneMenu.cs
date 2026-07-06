using System.Collections.Generic;
using LevelStreaming;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class StreamingPersistentSceneMenu
{
    public static List<string> FindPersistentScenes()
    {
        List<string> result = new();

        string regionManagerGuid = GetRegionManagerScriptGuid();
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");

        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);

            // Ignore packages & read-only folders
            if (scenePath.StartsWith("Packages/") || !AssetDatabase.IsOpenForEdit(scenePath))
                continue;

            // FAST dependency check (no scene opening)
            var dependencies = AssetDatabase.GetDependencies(scenePath, false);

            foreach (var dep in dependencies)
            {
                if (AssetDatabase.AssetPathToGUID(dep) == regionManagerGuid)
                {
                    result.Add(scenePath);
                    break;
                }
            }
        }

        return result;
    }
    
    public static void LoadPersistentScene(string persistentScenePath)
    {
        // Open persistent scene
        EditorSceneManager.OpenScene(persistentScenePath, OpenSceneMode.Single);

        RegionManager regionManager = Object.FindObjectOfType<RegionManager>();

        if (regionManager == null)
        {
            Debug.LogError("[Streaming] RegionManager missing.");
            return;
        }

        // Load all streaming scenes
        foreach (var region in regionManager.Regions)
        {
            if (region.Type != RegionManager.RegionType.Scene)
                continue;

            var sceneRef = region.SceneRef;
            if (sceneRef == null)
                continue;

            string path = sceneRef.ScenePath;

            if (string.IsNullOrEmpty(path))
                continue;

            EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }

        Debug.Log($"[Streaming] Loaded Persistent Scene: {persistentScenePath}");
    }

    private static string _cachedGuid;

    private static string GetRegionManagerScriptGuid()
    {
        if (!string.IsNullOrEmpty(_cachedGuid))
            return _cachedGuid;

        var go = new GameObject("Temp_GUID");
        var rm = go.AddComponent<RegionManager>();

        MonoScript script = MonoScript.FromMonoBehaviour(rm);
        _cachedGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script));
        Object.DestroyImmediate(go);

        return _cachedGuid;
    }
}