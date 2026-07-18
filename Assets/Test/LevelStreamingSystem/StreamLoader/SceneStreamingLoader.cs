using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelStreaming
{
    public sealed class SceneStreamingLoader : IStreamingLoader<RegionManager.Region>
    {
        private const string KeyPrefix = "scene:";
        private readonly RegionManager _regionManager;

        public SceneStreamingLoader(RegionManager regionManager)
        {
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        }

        public async Task LoadAsync(RegionManager.Region region)
        {
            if (region == null)
                throw new ArgumentNullException(nameof(region));
            if (region.SceneRef == null || string.IsNullOrWhiteSpace(region.SceneRef.ScenePath))
                throw new InvalidOperationException($"Region '{region.RegionName}' has no scene reference.");

            string path = region.SceneRef.ScenePath;
            string key = KeyPrefix + path;
            bool loadedByRegistry = false;

            await _regionManager.Registry.Acquire(
                key,
                async () =>
                {
                    try
                    {
                        var scene = SceneManager.GetSceneByPath(path);
                        if (scene.IsValid() && scene.isLoaded)
                            return;

                        if (!Application.CanStreamedLevelBeLoaded(path))
                        {
                            throw new InvalidOperationException(
                                $"Scene '{path}' cannot be streamed. Add it to Build Settings or use an Addressable scene backend.");
                        }

                        var operation = SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
                        if (operation == null)
                            throw new InvalidOperationException($"Unity did not create a load operation for scene '{path}'.");

                        await operation.ToTask();
                        loadedByRegistry = true;

                        scene = SceneManager.GetSceneByPath(path);
                        if (!scene.IsValid() || !scene.isLoaded)
                            throw new InvalidOperationException($"Scene '{path}' completed loading but is not available.");
                    }
                    catch
                    {
                        // The registry removes failed entries. If Unity loaded the scene
                        // before a later validation failed, undo that partial acquisition.
                        var partialScene = SceneManager.GetSceneByPath(path);
                        if (loadedByRegistry && partialScene.IsValid() && partialScene.isLoaded)
                        {
                            var cleanup = SceneManager.UnloadSceneAsync(partialScene);
                            if (cleanup != null)
                                await cleanup.ToTask();
                        }

                        loadedByRegistry = false;
                        throw;
                    }
                },
                async () =>
                {
                    // A scene that was already open before this registry acquired it is
                    // observed, not owned, and must not be unloaded by this system.
                    if (!loadedByRegistry)
                        return;

                    var scene = SceneManager.GetSceneByPath(path);
                    if (!scene.IsValid() || !scene.isLoaded)
                        return;

                    var operation = SceneManager.UnloadSceneAsync(scene);
                    if (operation == null)
                        throw new InvalidOperationException($"Unity did not create an unload operation for scene '{path}'.");

                    await operation.ToTask();
                });

            var loadedScene = SceneManager.GetSceneByPath(path);
            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                await _regionManager.Registry.Release(key);
                throw new InvalidOperationException($"Scene '{path}' is not loaded after acquisition.");
            }

            var meta = _regionManager.GetOrCreateMeta(region);
            meta.RegistryKey = key;
            meta.LoadedScene = loadedScene;
            meta.LoadedType = RegionManager.RegionType.Scene;
        }

        public async Task UnloadAsync(RegionManager.Region region)
        {
            if (region == null)
                throw new ArgumentNullException(nameof(region));

            var meta = _regionManager.GetOrCreateMeta(region);
            if (string.IsNullOrWhiteSpace(meta.RegistryKey))
                throw new InvalidOperationException($"Region '{region.RegionName}' has no acquired scene key.");

            await _regionManager.Registry.Release(meta.RegistryKey);
            meta.RegistryKey = null;
            meta.LoadedType = null;
        }

        public bool IsLoading(RegionManager.Region region) => _regionManager.IsRegionLoading(region);
        public bool IsLoaded(RegionManager.Region region) => _regionManager.IsRegionLoaded(region);
    }
}
