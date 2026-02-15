using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LevelStreaming
{
    [Serializable]
    public class SceneReference
    {
        [SerializeField] private string scenePath; // stored in build
#if UNITY_EDITOR
        [SerializeField] private SceneAsset sceneAsset; // editor-only
#endif

        public string ScenePath => scenePath;

#if UNITY_EDITOR
        public SceneAsset SceneAsset
        {
            get => sceneAsset;
            set
            {
                sceneAsset = value;
                scenePath = sceneAsset != null ? AssetDatabase.GetAssetPath(sceneAsset) : string.Empty;
            }
        }
#endif
    }
}