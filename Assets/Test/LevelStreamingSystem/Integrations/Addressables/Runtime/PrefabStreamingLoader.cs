using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace LevelStreaming
{
    public sealed class PrefabStreamingLoader : IStreamingLoader<RegionManager.Region>
    {
        private const string KeyPrefix = "prefab:";
        private readonly RegionManager _regionManager;

        public PrefabStreamingLoader(RegionManager regionManager)
        {
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        }

        public async Task LoadAsync(RegionManager.Region region)
        {
            if (region == null)
                throw new ArgumentNullException(nameof(region));
            if (region.PrefabRef == null || !region.PrefabRef.RuntimeKeyIsValid())
                throw new InvalidOperationException($"Region '{region.RegionName}' has no valid Addressable prefab reference.");

            object runtimeKey = region.PrefabRef.RuntimeKey;
            string key = KeyPrefix + runtimeKey;
            AsyncOperationHandle<GameObject> handle = default;
            bool acquired = false;
            GameObject instance = null;
            var meta = _regionManager.GetOrCreateMeta(region);

            try
            {
                GameObject prefab = await _regionManager.Registry.Acquire<GameObject>(
                    key,
                    async () =>
                    {
                        handle = Addressables.LoadAssetAsync<GameObject>(runtimeKey);
                        try
                        {
                            GameObject loadedPrefab = await handle.Task;
                            if (handle.Status != AsyncOperationStatus.Succeeded || loadedPrefab == null)
                            {
                                throw handle.OperationException ??
                                      new InvalidOperationException($"Addressable prefab '{runtimeKey}' returned no asset.");
                            }

                            return loadedPrefab;
                        }
                        catch
                        {
                            if (handle.IsValid())
                                Addressables.Release(handle);
                            throw;
                        }
                    },
                    () =>
                    {
                        if (handle.IsValid())
                            Addressables.Release(handle);
                        return Task.CompletedTask;
                    });

                acquired = true;
                meta.RegistryKey = key;
                instance = Object.Instantiate(prefab);
                instance.name = $"{region.RegionName}_Instance";

                var regionBound = instance.GetComponentInChildren<RegionBound>(true);
                if (regionBound != null)
                {
                    Bounds currentWorldBounds = BoundsTransformUtility.Transform(
                        regionBound.Bounds,
                        regionBound.transform.localToWorldMatrix);
                    instance.transform.position += region.CachedBounds.center - currentWorldBounds.center;
                }
                else
                {
                    instance.transform.position = region.CachedBounds.center;
                }

                meta.Instance = instance;
                meta.LoadedType = RegionManager.RegionType.Prefab;
            }
            catch (Exception loadException)
            {
                // If setup failed after the shared asset was acquired, roll back this
                // region's reference so the registry cannot leak it.
                if (instance != null)
                    Object.Destroy(instance);
                if (meta.Instance == instance)
                    meta.Instance = null;
                if (acquired)
                {
                    try
                    {
                        await _regionManager.Registry.Release(key);
                    }
                    catch (Exception cleanupException)
                    {
                        throw new AggregateException(
                            $"Prefab region '{region.RegionName}' failed to load and its registry rollback also failed.",
                            loadException,
                            cleanupException);
                    }
                    finally
                    {
                        meta.RegistryKey = null;
                        meta.LoadedType = null;
                    }
                }

                throw;
            }
        }

        public async Task UnloadAsync(RegionManager.Region region)
        {
            if (region == null)
                throw new ArgumentNullException(nameof(region));
            var meta = _regionManager.GetOrCreateMeta(region);
            if (string.IsNullOrWhiteSpace(meta.RegistryKey))
                throw new InvalidOperationException($"Region '{region.RegionName}' has no acquired prefab key.");

            GameObject instance = meta.Instance;
            bool wasActive = instance != null && instance.activeSelf;
            if (instance != null)
                instance.SetActive(false);

            try
            {
                await _regionManager.Registry.Release(meta.RegistryKey);
            }
            catch
            {
                if (instance != null)
                    instance.SetActive(wasActive);
                throw;
            }

            if (instance != null)
                Object.Destroy(instance);
            meta.Instance = null;
            meta.RegistryKey = null;
            meta.LoadedType = null;
        }

        public bool IsLoading(RegionManager.Region region) => _regionManager.IsRegionLoading(region);
        public bool IsLoaded(RegionManager.Region region) => _regionManager.IsRegionLoaded(region);
    }
}
