#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelStreaming.Editor
{
    public static class RegionAuthoringUtility
    {
        public static string GetDefaultRegionName(RegionManager.Region region)
        {
            if (region == null)
                return string.Empty;

            switch (region.Type)
            {
                case RegionManager.RegionType.Scene when region.SceneRef?.SceneAsset != null:
                    return region.SceneRef.SceneAsset.name;

                case RegionManager.RegionType.Prefab when region.PrefabRef != null:
                {
                    string path = AssetDatabase.GUIDToAssetPath(region.PrefabRef.AssetGUID);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    return prefab != null ? prefab.name : string.Empty;
                }

                case RegionManager.RegionType.AddressableScene when region.AddressableSceneRef != null:
                {
                    string path = AssetDatabase.GUIDToAssetPath(region.AddressableSceneRef.AssetGUID);
                    var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                    return scene != null ? scene.name : string.Empty;
                }

                default:
                    return string.Empty;
            }
        }

        public static RegionManager FindTargetManager(bool showDialog = true)
        {
            if (Selection.activeGameObject != null)
            {
                var selectedManager = Selection.activeGameObject.GetComponentInParent<RegionManager>();
                if (selectedManager != null)
                    return selectedManager;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            var managers = new List<RegionManager>();
            if (activeScene.IsValid() && activeScene.isLoaded)
            {
                foreach (var root in activeScene.GetRootGameObjects())
                    managers.AddRange(root.GetComponentsInChildren<RegionManager>(true));
            }

            if (managers.Count == 1)
                return managers[0];

            if (showDialog)
            {
                string message = managers.Count == 0
                    ? "No RegionManager exists in the active scene."
                    : "More than one RegionManager exists in the active scene. Select the manager to author.";
                EditorUtility.DisplayDialog("Streaming Region Manager", message, "OK");
            }

            return null;
        }
    }
}
#endif
