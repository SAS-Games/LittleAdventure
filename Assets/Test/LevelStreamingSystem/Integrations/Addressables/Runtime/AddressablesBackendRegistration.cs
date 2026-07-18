using UnityEngine;
using UnityEngine.Scripting;

namespace LevelStreaming.AddressablesIntegration
{
    [Preserve]
    internal static class AddressablesBackendRegistration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            RegionStreamingBackendRegistry.Register(
                RegionManager.RegionType.Prefab,
                regionManager => new PrefabStreamingLoader(regionManager));
            RegionStreamingBackendRegistry.Register(
                RegionManager.RegionType.AddressableScene,
                regionManager => new AddressableSceneStreamingLoader(regionManager));
        }
    }
}
