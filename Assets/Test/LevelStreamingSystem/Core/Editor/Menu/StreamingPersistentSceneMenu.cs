using System.Collections.Generic;
using LevelStreaming;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            if (scenePath.StartsWith("Packages/"))
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
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene persistentScene = EditorSceneManager.OpenScene(persistentScenePath, OpenSceneMode.Single);

        RegionManager regionManager = null;
        foreach (var root in persistentScene.GetRootGameObjects())
        {
            foreach (var candidate in root.GetComponentsInChildren<RegionManager>(true))
            {
                if (regionManager != null)
                {
                    Debug.LogError("[Streaming] Persistent scene has more than one RegionManager.");
                    return;
                }

                regionManager = candidate;
            }
        }

        if (regionManager == null)
        {
            Debug.LogError("[Streaming] RegionManager missing.");
            return;
        }

        // Load all streaming scenes
        foreach (var region in regionManager.Regions)
        {
            if (region == null || region.Type == RegionManager.RegionType.Prefab)
                continue;

            string path = region.Type == RegionManager.RegionType.Scene
                ? region.SceneRef?.ScenePath
                : AssetDatabase.GUIDToAssetPath(region.AddressableSceneRef?.AssetGUID);

            if (string.IsNullOrEmpty(path) || AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                Debug.LogWarning($"[Streaming] Region '{region.RegionName}' has a missing scene asset.");
                continue;
            }

            Scene existing = EditorSceneManager.GetSceneByPath(path);
            if (existing.IsValid() && existing.isLoaded)
                continue;

            try
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[Streaming] Could not open '{path}': {exception.Message}");
            }
        }

        EditorSceneManager.SetActiveScene(persistentScene);
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
