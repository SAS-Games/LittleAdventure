using LevelStreaming;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class StreamingHierarchyMenu
{
    private const string MenuPath = "GameObject/Streaming/Create Streaming Level";

    [MenuItem(MenuPath, false, 0)]
    private static void CreateStreamingLevel(MenuCommand command)
    {
        RegionManager regionManager = Object.FindObjectOfType<RegionManager>();

        if (regionManager == null)
        {
            EditorUtility.DisplayDialog("Missing RegionManager", "No RegionManager found in the active scene.", "OK");
            return;
        }

        string scenePath = EditorUtility.SaveFilePanelInProject("Create Streaming Scene", "StreamingLevel", "unity", "Select location for new streaming level");

        if (string.IsNullOrEmpty(scenePath))
            return;

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

        SerializedObject so = new SerializedObject(regionManager);
        SerializedProperty regionsProp = so.FindProperty("regions");

        if (regionsProp == null)
        {
            Debug.LogError("[Streaming] Could not find 'regions' field.");
            return;
        }

        so.Update();

        regionsProp.arraySize++;
        SerializedProperty newRegion = regionsProp.GetArrayElementAtIndex(regionsProp.arraySize - 1);
        newRegion.FindPropertyRelative("type").enumValueIndex = (int)RegionManager.RegionType.Scene;
        newRegion.FindPropertyRelative("regionName").stringValue = Path.GetFileNameWithoutExtension(scenePath);

        SerializedProperty sceneRefProp = newRegion.FindPropertyRelative("sceneRef");
        SerializedProperty sceneAssetProp = sceneRefProp.FindPropertyRelative("sceneAsset"); 
        sceneAssetProp.objectReferenceValue = sceneAsset;
        
        var defaultStrategy = FindDefaultUnloadStrategy();

        if (defaultStrategy != null) 
            newRegion.FindPropertyRelative("unloadStrategy").objectReferenceValue = defaultStrategy;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(regionManager);
        Debug.Log($"[Streaming] Created and registered region: {scenePath}");
    }

    [MenuItem(MenuPath, true)]
    private static bool ValidateCreateStreamingLevel()
    {
        return Object.FindFirstObjectByType<RegionManager>() != null;
    }
    
    
    private static UnloadStrategy FindDefaultUnloadStrategy()
    {
        string[] guids = AssetDatabase.FindAssets("t:BoundsIntersectionUnloadStrategy");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Optional: restrict to LevelStreaming folder
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