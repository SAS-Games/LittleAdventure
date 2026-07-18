using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelStreaming
{
    /// <summary>
    /// Package-neutral reference to an asset by GUID.
    ///
    /// The serialized field names intentionally match Addressables' AssetReference
    /// layout so existing region data survives when the Addressables package is
    /// removed or reinstalled.
    /// </summary>
    [Serializable]
    public class StreamingAssetReference
    {
        [FormerlySerializedAs("m_assetGUID")]
        [SerializeField] private string m_AssetGUID = string.Empty;
        [SerializeField] private string m_SubObjectName;
        [SerializeField] private string m_SubObjectType;
#if UNITY_EDITOR
        [SerializeField] private string m_SubObjectGUID;
        [SerializeField] private bool m_EditorAssetChanged;
#endif

        public string AssetGUID => m_AssetGUID ?? string.Empty;

        public object RuntimeKey => string.IsNullOrEmpty(m_SubObjectName)
            ? AssetGUID
            : $"{AssetGUID}[{m_SubObjectName}]";

        public bool RuntimeKeyIsValid()
        {
            string key = RuntimeKey as string;
            if (string.IsNullOrWhiteSpace(key))
                return false;

            int subObjectIndex = key.IndexOf('[', StringComparison.Ordinal);
            string guid = subObjectIndex >= 0 ? key.Substring(0, subObjectIndex) : key;
            return Guid.TryParse(guid, out _);
        }
    }

    [Serializable]
    public sealed class StreamingPrefabReference : StreamingAssetReference
    {
    }

    [Serializable]
    public sealed class StreamingSceneReference : StreamingAssetReference
    {
    }
}
