using SAS.Core.TagSystem;
using UnityEngine;

namespace SAS.AssetLoader
{
    interface IAssetLoaderSprite : IAssetLoaderT<Sprite>
    {
    }

    public class AssetLoaderSprite : AssetLoaderT<Sprite>, IAssetLoaderSprite
    {
        public AssetLoaderSprite(IContextBinder _) : base(_)
        {
        }
    }
}
