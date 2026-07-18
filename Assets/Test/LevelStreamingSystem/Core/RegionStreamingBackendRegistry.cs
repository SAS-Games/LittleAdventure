using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelStreaming
{
    /// <summary>
    /// Registration point for optional streaming implementations such as Addressables.
    /// The core assembly never references those packages directly.
    /// </summary>
    public static class RegionStreamingBackendRegistry
    {
        public delegate IStreamingLoader<RegionManager.Region> LoaderFactory(RegionManager regionManager);

        private static readonly Dictionary<RegionManager.RegionType, LoaderFactory> Factories = new();

        public static void Register(RegionManager.RegionType type, LoaderFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (type == RegionManager.RegionType.Scene)
                throw new ArgumentException("The built-in scene backend cannot be replaced.", nameof(type));

            Factories[type] = factory;
        }

        public static bool TryCreate(RegionManager.RegionType type, RegionManager regionManager,
            out IStreamingLoader<RegionManager.Region> loader)
        {
            loader = null;
            if (regionManager == null || !Factories.TryGetValue(type, out LoaderFactory factory))
                return false;

            loader = factory(regionManager);
            return loader != null;
        }

        public static bool IsRegistered(RegionManager.RegionType type) => Factories.ContainsKey(type);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Factories.Clear();
        }
    }
}
