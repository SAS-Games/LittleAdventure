using System;
using System.Collections.Generic;
using LevelStreaming;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StreamingPersistentSceneMenu
{
    private const string LoadAllContextMenuPath =
        "CONTEXT/RegionManager/Load All Streaming Scenes For Editing";

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

            // Persistent scenes normally inherit RegionManager from the
            // LS_StreamingManager prefab, so nested dependencies must be included.
            var dependencies = AssetDatabase.GetDependencies(scenePath, true);

            foreach (var dep in dependencies)
            {
                if (AssetDatabase.AssetPathToGUID(dep) == regionManagerGuid)
                {
                    result.Add(scenePath);
                    break;
                }
            }
        }

        result.Sort(StringComparer.OrdinalIgnoreCase);
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

        LoadAllStreamingScenesForEditing(regionManager);
        Debug.Log($"[Streaming] Loaded Persistent Scene: {persistentScenePath}");
    }

    /// <summary>
    /// Opens every scene-backed region additively for authoring while keeping the
    /// RegionManager's persistent scene active.
    /// </summary>
    public static void LoadAllStreamingScenesForEditing(RegionManager regionManager)
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[Streaming] Streaming scenes can only be opened for editing outside Play Mode.");
            return;
        }

        if (regionManager == null)
        {
            Debug.LogError("[Streaming] Select a persistent-scene object with a RegionManager.");
            return;
        }

        Scene persistentScene = regionManager.gameObject.scene;
        if (!persistentScene.IsValid() || !persistentScene.isLoaded)
        {
            Debug.LogError("[Streaming] The selected RegionManager must belong to a loaded scene.", regionManager);
            return;
        }

        int openedCount = 0;
        int alreadyLoadedCount = 0;
        int invalidCount = 0;
        var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var region in regionManager.Regions)
        {
            if (region == null || region.Type == RegionManager.RegionType.Prefab)
                continue;

            string path = GetScenePath(region);

            if (string.IsNullOrWhiteSpace(path) ||
                AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                invalidCount++;
                Debug.LogWarning(
                    $"[Streaming] Region '{region.RegionName}' has a missing scene asset.",
                    regionManager);
                continue;
            }

            path = path.Replace('\\', '/');
            if (!visitedPaths.Add(path))
                continue;

            Scene existing = EditorSceneManager.GetSceneByPath(path);
            if (existing.IsValid() && existing.isLoaded)
            {
                alreadyLoadedCount++;
                continue;
            }

            try
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                openedCount++;
            }
            catch (Exception exception)
            {
                invalidCount++;
                Debug.LogError(
                    $"[Streaming] Could not open '{path}': {exception.Message}",
                    regionManager);
            }
        }

        EditorSceneManager.SetActiveScene(persistentScene);
        Debug.Log(
            $"[Streaming] Edit-time scene load complete for '{persistentScene.name}': " +
            $"{openedCount} opened, {alreadyLoadedCount} already loaded, {invalidCount} invalid.",
            regionManager);
    }

    [MenuItem(LoadAllContextMenuPath, false, 1000)]
    private static void LoadAllStreamingScenesForEditing(MenuCommand command)
    {
        LoadAllStreamingScenesForEditing(command.context as RegionManager);
    }

    [MenuItem(LoadAllContextMenuPath, true)]
    private static bool ValidateLoadAllStreamingScenesForEditing(MenuCommand command)
    {
        if (Application.isPlaying || command.context is not RegionManager regionManager)
            return false;

        Scene scene = regionManager.gameObject.scene;
        return scene.IsValid() && scene.isLoaded;
    }

    private static string GetScenePath(RegionManager.Region region)
    {
        if (region.Type == RegionManager.RegionType.AddressableScene)
            return AssetDatabase.GUIDToAssetPath(region.AddressableSceneRef?.AssetGUID);

        if (region.Type != RegionManager.RegionType.Scene || region.SceneRef == null)
            return string.Empty;

        string assetPath = region.SceneRef.SceneAsset == null
            ? string.Empty
            : AssetDatabase.GetAssetPath(region.SceneRef.SceneAsset);

        return string.IsNullOrWhiteSpace(assetPath)
            ? region.SceneRef.ScenePath
            : assetPath;
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
        UnityEngine.Object.DestroyImmediate(go);

        return _cachedGuid;
    }
}
