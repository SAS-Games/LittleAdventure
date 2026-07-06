using LevelStreaming;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class StreamingHierarchyMenu
{
    private const string CreateMenuPath = "GameObject/Streaming/Create Streaming Level";
    private const string AddExistingMenuPath = "GameObject/Streaming/Add Existing Streaming Level";

  
    [MenuItem(CreateMenuPath, false, 0)]
    private static void CreateStreamingLevel(MenuCommand command)
    {
        RegionManager regionManager =
            Object.FindFirstObjectByType<RegionManager>();

        if (regionManager == null)
        {
            EditorUtility.DisplayDialog("Missing RegionManager", "No RegionManager found in the active scene.", "OK");
            return;
        }

        string scenePath = EditorUtility.SaveFilePanelInProject("Create Streaming Scene", "StreamingLevel", "unity", "Select location for new streaming level");

        if (string.IsNullOrEmpty(scenePath))
            return;

        // Create additive empty scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

        if (!EditorSceneManager.SaveScene(newScene, scenePath))
        {
            Debug.LogError("[Streaming] Failed to save streaming scene.");
            return;
        }

        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

        if (sceneAsset == null)
        {
            Debug.LogError("[Streaming] Could not load SceneAsset.");
            return;
        }

        RegisterScene(regionManager, sceneAsset);
    }

    [MenuItem(CreateMenuPath, true)]
    private static bool ValidateCreateStreamingLevel()
    {
        return Object.FindFirstObjectByType<RegionManager>() != null;
    }

    [MenuItem(AddExistingMenuPath, false, 1)]
    private static void AddExistingStreamingLevel(MenuCommand command)
    {
        RegionManager regionManager = Object.FindFirstObjectByType<RegionManager>();

        if (regionManager == null)
        {
            EditorUtility.DisplayDialog("Missing RegionManager", "No RegionManager found in the active scene.", "OK");
            return;
        }

        string absolutePath = EditorUtility.OpenFilePanel("Select Streaming Scene", Application.dataPath, "unity");

        if (string.IsNullOrEmpty(absolutePath))
            return;

        if (!absolutePath.StartsWith(Application.dataPath))
        {
            Debug.LogError("[Streaming] Scene must be inside Assets folder.");
            return;
        }

        string scenePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);

        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

        if (sceneAsset == null)
        {
            Debug.LogError("[Streaming] Invalid SceneAsset.");
            return;
        }

        RegisterScene(regionManager, sceneAsset);
    }

    [MenuItem(AddExistingMenuPath, true)]
    private static bool ValidateAddExistingStreamingLevel()
    {
        return Object.FindFirstObjectByType<RegionManager>() != null;
    }

    private static void RegisterScene(RegionManager regionManager, SceneAsset sceneAsset)
    {
        string scenePath = AssetDatabase.GetAssetPath(sceneAsset);

        SerializedObject so = new SerializedObject(regionManager);
        SerializedProperty regionsProp = so.FindProperty("regions");

        if (regionsProp == null)
        {
            Debug.LogError("[Streaming] Could not find 'regions' field.");
            return;
        }

        so.Update();

        for (int i = 0; i < regionsProp.arraySize; i++)
        {
            var regionProp = regionsProp.GetArrayElementAtIndex(i);

            var sceneRefProp = regionProp.FindPropertyRelative("sceneRef");
            if (sceneRefProp == null)
                continue;

            var scenePathProp = sceneRefProp.FindPropertyRelative("scenePath");
            if (scenePathProp == null)
                continue;

            var existingPath = scenePathProp.stringValue;

            if (existingPath == scenePath)
            {
                Debug.LogWarning($"Scene '{scenePath}' already used by another region. " + "This is allowed but ensure bounds differ.");
            }
        }

        // Add new region entry
        regionsProp.arraySize++;

        SerializedProperty newRegion = regionsProp.GetArrayElementAtIndex(regionsProp.arraySize - 1);
        newRegion.FindPropertyRelative("type").enumValueIndex = (int)RegionManager.RegionType.Scene;
        newRegion.FindPropertyRelative("regionName").stringValue = Path.GetFileNameWithoutExtension(scenePath);

        var defaultStrategy = FindDefaultUnloadStrategy();
        if (defaultStrategy != null)
            newRegion.FindPropertyRelative("unloadStrategy").objectReferenceValue = defaultStrategy;

        so.ApplyModifiedProperties();
        
        var region = regionManager.Regions[regionManager.Regions.Count - 1];

#if UNITY_EDITOR
        region.SceneRef.SceneAsset = sceneAsset;
#endif

        EditorUtility.SetDirty(regionManager);

        Debug.Log($"[Streaming] Registered region: {scenePath}");
    }
    
    private static UnloadStrategy FindDefaultUnloadStrategy()
    {
        string[] guids = AssetDatabase.FindAssets("t:BoundsIntersectionUnloadStrategy");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.Contains("LevelStreaming"))
                continue;

            var strategy = AssetDatabase.LoadAssetAtPath<UnloadStrategy>(path);

            if (strategy != null)
                return strategy;
        }

        Debug.LogWarning("[Streaming] Default UnloadStrategy not found.");
        return null;
    }
}