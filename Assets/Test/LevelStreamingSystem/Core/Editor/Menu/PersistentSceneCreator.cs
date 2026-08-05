using LevelStreaming;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LevelStreaming.Editor;

public static class PersistentSceneCreator
{
    private const string DefaultSceneName = "PersistentScene";
    private const string PrefabSearchFilter = "LS_StreamingManager t:Prefab";

    [MenuItem("Tools/Streaming/Create Persistent Scene", priority = 0)]
    public static void CreatePersistentScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        GameObject prefab = LoadStreamingManagerPrefab();
        if (prefab == null)
        {
            Debug.LogError("[PersistentSceneCreator] Could not find unique StreamingManager prefab.");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Persistent Scene",
            DefaultSceneName,
            "unity",
            "Save the Persistent Scene");
        if (string.IsNullOrEmpty(path))
            return;

        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        Camera cam = Camera.main;

        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        if (cam.GetComponent<IStreamingBoundsProvider>() == null)
            Undo.AddComponent<AdaptiveStreamingBoundsProvider>(cam.gameObject);

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab, newScene) as GameObject;
        if (instance == null)
        {
            Debug.LogError("[PersistentSceneCreator] Could not instantiate the StreamingManager prefab.");
            return;
        }

        instance.name = "StreamingManager";
        EditorSceneManager.MarkSceneDirty(newScene);

        if (!EditorSceneManager.SaveScene(newScene, path))
        {
            Debug.LogError($"[PersistentSceneCreator] Could not save persistent scene '{path}'.");
            return;
        }

        SceneBuildSettingsUtility.EnsureEnabled(path);
        Debug.Log("[PersistentSceneCreator] Persistent Scene created successfully.");
    }

    private static GameObject LoadStreamingManagerPrefab()
    {
        string[] guids = AssetDatabase.FindAssets(PrefabSearchFilter);

        if (guids.Length == 0)
        {
            Debug.LogError("[PersistentSceneCreator] No StreamingManager prefab found in project.");
            return null;
        }

        if (guids.Length > 1)
        {
            Debug.LogError("[PersistentSceneCreator] Multiple StreamingManager prefabs found. Please ensure only one exists.");
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
}
