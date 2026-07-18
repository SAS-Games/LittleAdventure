using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace LevelStreaming
{
    public sealed class AddressableSceneStreamingLoader : IStreamingLoader<RegionManager.Region>
    {
        private const string KeyPrefix = "addressable-scene:";
        private readonly RegionManager _regionManager;

        public AddressableSceneStreamingLoader(RegionManager regionManager)
        {
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        }

        public async Task LoadAsync(RegionManager.Region region)
        {
            if (region == null)
                throw new ArgumentNullException(nameof(region));
            if (region.AddressableSceneRef == null || !region.AddressableSceneRef.RuntimeKeyIsValid())
            {
                throw new InvalidOperationException(
                    $"Region '{region.RegionName}' has no valid Addressable scene reference.");
            }

            object runtimeKey = region.AddressableSceneRef.RuntimeKey;
            string key = KeyPrefix + runtimeKey;
            AsyncOperationHandle<SceneInstance> handle = default;

            SceneInstance instance = await _regionManager.Registry.Acquire<SceneInstance>(
                key,
                async () =>
                {
                    handle = Addressables.LoadSceneAsync(runtimeKey, LoadSceneMode.Additive, true);
                    try
                    {
                        SceneInstance loaded = await handle.Task;
                        if (handle.Status != AsyncOperationStatus.Succeeded ||
                            !loaded.Scene.IsValid() || !loaded.Scene.isLoaded)
                        {
                            throw handle.OperationException ??
                                  new InvalidOperationException(
                                      $"Addressable scene '{runtimeKey}' completed without a loaded scene.");
                        }

                        return loaded;
                    }
                    catch (Exception loadException)
                    {
                        try
                        {
                            await ReleaseFailedLoadAsync(handle, runtimeKey);
                        }
                        catch (Exception cleanupException)
                        {
                            throw new AggregateException(
                                $"Addressable scene '{runtimeKey}' failed to load and cleanup also failed.",
                                loadException,
                                cleanupException);
                        }

                        throw;
                    }
                },
                async () =>
                {
                    if (!handle.IsValid())
                        return;

                    // Keep the operation handle valid until its result has been checked.
                    // With auto-release enabled, reading Status after awaiting can access
                    // an already released handle.
                    var unloadHandle = Addressables.UnloadSceneAsync(handle, false);
                    try
                    {
                        await unloadHandle.Task;
                        if (unloadHandle.Status != AsyncOperationStatus.Succeeded)
                        {
                            throw unloadHandle.OperationException ??
                                  new InvalidOperationException(
                                      $"Addressable scene '{runtimeKey}' failed to unload.");
                        }
                    }
                    finally
                    {
                        if (unloadHandle.IsValid())
                            Addressables.Release(unloadHandle);
                    }
                });

            var meta = _regionManager.GetOrCreateMeta(region);
            meta.RegistryKey = key;
            meta.LoadedScene = instance.Scene;
            meta.LoadedType = RegionManager.RegionType.AddressableScene;
        }

        public async Task UnloadAsync(RegionManager.Region region)
        {
            if (region == null)
                throw new ArgumentNullException(nameof(region));

            var meta = _regionManager.GetOrCreateMeta(region);
            if (string.IsNullOrWhiteSpace(meta.RegistryKey))
            {
                throw new InvalidOperationException(
                    $"Region '{region.RegionName}' has no acquired Addressable scene key.");
            }

            await _regionManager.Registry.Release(meta.RegistryKey);
            meta.RegistryKey = null;
            meta.LoadedType = null;
        }

        public bool IsLoading(RegionManager.Region region) => _regionManager.IsRegionLoading(region);
        public bool IsLoaded(RegionManager.Region region) => _regionManager.IsRegionLoaded(region);

        private static async Task ReleaseFailedLoadAsync(
            AsyncOperationHandle<SceneInstance> handle,
            object runtimeKey)
        {
            if (!handle.IsValid())
                return;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(handle);
                return;
            }

            var unloadHandle = Addressables.UnloadSceneAsync(handle, false);
            try
            {
                await unloadHandle.Task;
                if (unloadHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw unloadHandle.OperationException ??
                          new InvalidOperationException(
                              $"Addressable scene '{runtimeKey}' failed cleanup after an invalid load result.");
                }
            }
            finally
            {
                if (unloadHandle.IsValid())
                    Addressables.Release(unloadHandle);
            }
        }
    }
}
