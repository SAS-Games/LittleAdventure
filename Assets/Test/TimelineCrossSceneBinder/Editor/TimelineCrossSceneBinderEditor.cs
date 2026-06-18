using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using Debug = SAS.Debug;

[CustomEditor(typeof(TimelineCrossSceneBinder))]
public class TimelineCrossSceneBinderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var binder = (TimelineCrossSceneBinder)target;
        var serialized = serializedObject;
        serialized.Update();

        // Draw everything except the bindings list
        var prop = serialized.FindProperty("m_Director");
        var bindingsProp = serialized.FindProperty("m_Bindings");

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(prop);
            EditorGUILayout.PropertyField(bindingsProp, new GUIContent("Bindings List (auto-generated)"),
                includeChildren: true);
        }

        serialized.ApplyModifiedProperties();
        EditorGUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Populate From Timeline", GUILayout.Height(28)))
                PopulateFromTimeline(binder);

            if (GUILayout.Button("Restore Timeline Bindings", GUILayout.Height(28)))
                RestoreTimelineBindings(binder);
        }
    }

    private void PopulateFromTimeline(TimelineCrossSceneBinder binder)
    {
        var director = binder.Director;
        if (director == null || director.playableAsset == null)
        {
            Debug.LogWarning("PlayableDirector or Timeline Asset not assigned.");
            return;
        }

        if (director.playableAsset is not TimelineAsset timeline)
        {
            Debug.LogWarning("PlayableDirector's asset is not a TimelineAsset.");
            return;
        }

        var bindings = binder.Bindings;
        bindings.Clear();

        foreach (var track in timeline.GetOutputTracks())
        {
            var info = new TimelineCrossSceneBinder.TrackBindingInfo
            {
                timelineTrack = track
            };

            var boundObj = director.GetGenericBinding(track);
            if (boundObj is Component comp)
                boundObj = comp.gameObject;

            if (boundObj is GameObject go)
            {
                if (!go.TryGetComponent<ObjectGuid>(out var objectGuid))
                    objectGuid = go.AddComponent<ObjectGuid>();
                info.guidReference = new GuidReference(objectGuid);
            }

            bindings.Add(info);
        }

        Debug.Log($"Populated {bindings.Count} bindings from Timeline '{timeline.name}'");
        EditorUtility.SetDirty(binder);
    }

    private void RestoreTimelineBindings(TimelineCrossSceneBinder binder)
    {
        var director = binder.Director;
        var bindings = binder.Bindings;

        if (director == null || bindings == null || bindings.Count == 0)
        {
            Debug.LogWarning("PlayableDirector or binding list not set.");
            return;
        }

        HashSet<string> requiredScenes = new();
        foreach (var info in bindings)
        {
            if (info.guidReference.CachedScene)
                requiredScenes.Add(info.guidReference.CachedScene.name);
        }

        foreach (var sceneName in requiredScenes)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                var scenePath = GetScenePath(sceneName);
                if (string.IsNullOrEmpty(scenePath))
                {
                    Debug.LogWarning($"Scene '{sceneName}' not found.");
                    continue;
                }

                Debug.Log($"Opening scene '{sceneName}' additively...");
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }
        }
        binder.BindAll();
        EditorUtility.SetDirty(director);
        Debug.Log("All missing Timeline bindings have been restored.");
    }

    private static string GetScenePath(string sceneName)
    {
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            var scene = EditorBuildSettings.scenes[i];
            if (scene.path.Contains(sceneName))
                return scene.path;
        }

        // Fallback: try AssetDatabase
        var guid = AssetDatabase.FindAssets(sceneName + " t:Scene");
        if (guid.Length > 0)
            return AssetDatabase.GUIDToAssetPath(guid[0]);

        return string.Empty;
    }
}