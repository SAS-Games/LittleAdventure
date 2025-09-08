using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public partial class SceneBoundsManager
{
    public partial class SceneRef
    {
        public SceneAsset sceneAsset;

        public void OnValidate()
        {
            if (sceneAsset != null)
                sceneName = sceneAsset.name;
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
                var sb = go.GetComponentInChildren<SceneBound>();
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

    public void ApplyBoundsToScene(SceneRef sceneRef)
    {
        if (sceneRef.sceneAsset == null) return;

        string scenePath = AssetDatabase.GetAssetPath(sceneRef.sceneAsset);
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

        foreach (var go in scene.GetRootGameObjects())
        {
            var sb = go.GetComponentInChildren<SceneBound>();
            if (sb != null)
            {
                Undo.RecordObject(sb, "Update Scene Bound");
                sb.Bounds = new Bounds(
                    sb.transform.InverseTransformPoint(sceneRef.cachedBounds.center),
                    sceneRef.cachedBounds.size
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
}