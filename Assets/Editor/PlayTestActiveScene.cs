using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

[InitializeOnLoad]
public static class PlayLoadedScene
{
    private static readonly string[] prefabPaths =
    {
        "Assets/Editor/Prefabs/SceneTestPrerequsites.prefab",
        "Assets/Editor/Prefabs/TestBootstrapper.prefab",
        "Assets/Editor/Prefabs/TestSceneGroupLoader.prefab"
    };

    private static readonly List<GameObject> AddedPrefabs = new();
    
    static PlayLoadedScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("Tools/Play Active Scene &#r")] // Alt+Shift+R
    public static void PlaySceneWithPrefabs()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Already in Play Mode!");
            return;
        }

        AddPrefabsToScene();
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlaying)
                EditorApplication.EnterPlaymode();
        };
    }

    private static void AddPrefabsToScene()
    {
        AddedPrefabs.Clear();

        foreach (var path in prefabPaths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found: {path}");
                continue;
            }

            if (GameObject.Find(prefab.name) == null)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                instance.name = prefab.name;
                Undo.RegisterCreatedObjectUndo(instance, "Add Prefab Before Play");
                AddedPrefabs.Add(instance);
                Debug.Log($"Added prefab: {prefab.name}");
            }
        }

        //EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
            CleanupAddedPrefabs();
    }

    private static void CleanupAddedPrefabs()
    {
        foreach (var path in prefabPaths)
        {
            string prefabName = Path.GetFileNameWithoutExtension(path);
            GameObject go = GameObject.Find(prefabName);
            if (go != null)
            {
                Object.DestroyImmediate(go);
                Debug.Log($"Removed prefab after play: {prefabName}");
            }
        }
    }
}
