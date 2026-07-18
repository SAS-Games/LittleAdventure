using LevelStreaming;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using LevelStreaming.Editor;

public static class StreamingHierarchyMenu
{
    private const string CreateMenuPath = "GameObject/Streaming/Create Streaming Level";
    private const string AddExistingMenuPath = "GameObject/Streaming/Add Existing Streaming Level";

  
    [MenuItem(CreateMenuPath, false, 0)]
    private static void CreateStreamingLevel(MenuCommand command)
    {
        RegionManager regionManager = RegionAuthoringUtility.FindTargetManager();

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
        var regionRoot = new GameObject("Region");
        SceneManager.MoveGameObjectToScene(regionRoot, newScene);
        regionRoot.AddComponent<RegionBound>();

        if (!EditorSceneManager.SaveScene(newScene, scenePath))
        {
            Debug.LogError("[Streaming] Failed to save streaming scene.");
            EditorSceneManager.CloseScene(newScene, true);
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
        return RegionAuthoringUtility.FindTargetManager(showDialog: false) != null;
    }

    [MenuItem(AddExistingMenuPath, false, 1)]
    private static void AddExistingStreamingLevel(MenuCommand command)
    {
        RegionManager regionManager = RegionAuthoringUtility.FindTargetManager();

        if (regionManager == null)
        {
            EditorUtility.DisplayDialog("Missing RegionManager", "No RegionManager found in the active scene.", "OK");
            return;
        }

        string absolutePath = EditorUtility.OpenFilePanel("Select Streaming Scene", Application.dataPath, "unity");

        if (string.IsNullOrEmpty(absolutePath))
            return;

        string scenePath = FileUtil.GetProjectRelativePath(absolutePath);
        if (string.IsNullOrWhiteSpace(scenePath) || !scenePath.StartsWith("Assets/"))
        {
            Debug.LogError("[Streaming] Scene must be inside Assets folder.");
            return;
        }

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
        return RegionAuthoringUtility.FindTargetManager(showDialog: false) != null;
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
                Debug.LogWarning(
                    $"Scene '{scenePath}' is already used by another region. Runtime loading will be shared; " +
                    "apply source bounds for these regions individually.");
            }
        }

        // Add a fully initialized region entry. Unity may duplicate the previous array
        // element when growing serialized arrays, so every relevant field is overwritten.
        int newIndex = regionsProp.arraySize;
        regionsProp.arraySize = newIndex + 1;

        SerializedProperty newRegion = regionsProp.GetArrayElementAtIndex(newIndex);
        newRegion.FindPropertyRelative("type").enumValueIndex = (int)RegionManager.RegionType.Scene;
        newRegion.FindPropertyRelative("regionName").stringValue =
            GenerateUniqueRegionName(regionManager, Path.GetFileNameWithoutExtension(scenePath));
        newRegion.FindPropertyRelative("cachedBounds").boundsValue =
            new Bounds(Vector3.zero, Vector3.one * 2f);
        newRegion.FindPropertyRelative("portals").arraySize = 0;

        SerializedProperty newSceneRef = newRegion.FindPropertyRelative("sceneRef");
        newSceneRef.FindPropertyRelative("sceneAsset").objectReferenceValue = sceneAsset;
        newSceneRef.FindPropertyRelative("scenePath").stringValue = scenePath;

        ClearAssetReference(newRegion.FindPropertyRelative("prefabRef"));
        ClearAssetReference(newRegion.FindPropertyRelative("addressableSceneRef"));

        var defaultStrategy = FindDefaultUnloadStrategy();
        newRegion.FindPropertyRelative("unloadStrategy").objectReferenceValue = defaultStrategy;

        so.ApplyModifiedProperties();
        SceneBuildSettingsUtility.EnsureEnabled(scenePath);
        EditorUtility.SetDirty(regionManager);
        EditorSceneManager.MarkSceneDirty(regionManager.gameObject.scene);

        Debug.Log($"[Streaming] Registered region: {scenePath}");
    }

    private static string GenerateUniqueRegionName(RegionManager manager, string baseName)
    {
        string candidate = string.IsNullOrWhiteSpace(baseName) ? "Region" : baseName;
        var existing = new HashSet<string>();
        foreach (var region in manager.Regions)
        {
            if (region != null && !string.IsNullOrWhiteSpace(region.RegionName))
                existing.Add(region.RegionName);
        }

        if (!existing.Contains(candidate))
            return candidate;

        int suffix = 2;
        while (existing.Contains($"{candidate}_{suffix}"))
            suffix++;
        return $"{candidate}_{suffix}";
    }

    private static void ClearAssetReference(SerializedProperty property)
    {
        if (property == null)
            return;

        SerializedProperty guid = property.FindPropertyRelative("m_AssetGUID");
        if (guid != null)
            guid.stringValue = string.Empty;
        SerializedProperty subObject = property.FindPropertyRelative("m_SubObjectName");
        if (subObject != null)
            subObject.stringValue = string.Empty;
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
