using SAS.Core.TagSystem;
using UnityEngine;

namespace SAS.AssetLoader
{
    interface IAssetLoaderAudioClip : IAssetLoaderT<AudioClip>
    {
    }

    public class AssetLoaderAudioClip : AssetLoaderT<AudioClip>, IAssetLoaderAudioClip
    {
        public AssetLoaderAudioClip(IContextBinder _) : base(_)
        {
        }
    }
}
