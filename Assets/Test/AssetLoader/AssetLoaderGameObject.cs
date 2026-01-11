using SAS.Core.TagSystem;
using UnityEngine;

namespace SAS.AssetLoader
{
    public interface IAssetLoaderGameObject : IAssetLoaderT<GameObject>
    {
    }

    public class AssetLoaderGameObject : AssetLoaderT<GameObject>, IAssetLoaderGameObject
    {
        public AssetLoaderGameObject(IContextBinder _) : base(_)
        { 
        }
    }
}
