using SAS.Utilities.TagSystem;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SAS.AssetLoader
{
    public interface IAssetLoaderT<T> : IAssetLoader<T>
    {
        Task<T> LoadAsync(AssetReference assetReference);
    }

    public abstract class AssetLoaderT<T> : IAssetLoaderT<T>
    {
        async public Task<T> LoadAsync(AssetReference assetReference)
        {
            AsyncOperationHandle<T> asyncOperationHandle = Addressables.LoadAssetAsync<T>(assetReference);
            await asyncOperationHandle.Task;
            return GetAsset(asyncOperationHandle);
        }

        async Task<T> IAssetLoader<T>.LoadAsync(string path)
        {
            AsyncOperationHandle<T> asyncOperationHandle = Addressables.LoadAssetAsync<T>(path);
            await asyncOperationHandle.Task;
            return GetAsset(asyncOperationHandle);
        }

        private T GetAsset(AsyncOperationHandle<T> asyncOperationHandle)
        {
            T asset = default(T);
            if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
                asset = asyncOperationHandle.Result;  
            return asset;
        }

        public AssetLoaderT(IContextBinder _) { }
    }
}
