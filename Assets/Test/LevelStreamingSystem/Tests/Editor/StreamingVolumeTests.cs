using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace LevelStreaming.Tests
{
    public sealed class StreamingVolumeTests
    {
        // Test-only geometry with empty space inside its envelope. This catches any
        // runtime path that mistakenly treats an envelope overlap as an exact result.
        [Serializable]
        private sealed class SplitVolume : StreamingVolume
        {
            private readonly BoxStreamingVolume left = Box(-5f, 2f);
            private readonly BoxStreamingVolume right = Box(5f, 2f);
            public override Bounds BroadphaseBounds => new(Vector3.zero, new Vector3(12f, 2f, 2f));
            public override bool Contains(Vector3 point) => left.Contains(point) || right.Contains(point);
            public override bool TryIntersects(IStreamingVolume other, out bool intersects)
            {
                intersects = false;
                if (other is not BoxStreamingVolume box) return false;
                intersects = left.Bounds.Intersects(box.Bounds) || right.Bounds.Intersects(box.Bounds);
                return true;
            }
        }

        private static BoxStreamingVolume Box(float x, float size = 1f) =>
            new(new Bounds(new Vector3(x, 0f, 0f), Vector3.one * size));

        private sealed class VolumeProvider : IStreamingVolumeProvider
        {
            public IStreamingVolume Query;
            public bool TryGetVolumes(out StreamingVolumeSnapshot snapshot)
            {
                snapshot = new StreamingVolumeSnapshot(Query, Query, Query);
                return true;
            }
        }

        [Test]
        public void ExactIntersection_RejectsEnvelopeOverlapAndDispatchesInBothOrders()
        {
            var split = new SplitVolume();
            Assert.That(split.BroadphaseBounds.Intersects(Box(0f).Bounds), Is.True);
            Assert.That(StreamingVolumeIntersection.Intersects(split, Box(0f)), Is.False);
            Assert.That(StreamingVolumeIntersection.Intersects(Box(0f), split), Is.False);
            Assert.That(StreamingVolumeIntersection.Intersects(split, Box(-5f)), Is.True);
            Assert.That(StreamingVolumeIntersection.Intersects(Box(5f), split), Is.True);
        }

        [Test]
        public void UnsupportedOverlappingPairs_ReportMissingExactImplementation()
        {
            Assert.Throws<NotSupportedException>(() =>
                StreamingVolumeIntersection.Intersects(new SplitVolume(), new SplitVolume()));
        }

        [Test]
        public void LegacyRegion_TracksCachedBoxAndRetainsAnchorWhenVolumeChanges()
        {
            var region = new RegionManager.Region { CachedBounds = Box(100f).Bounds };
            Assert.That(region.Intersects(Box(100f)), Is.True);
            region.CachedBounds = Box(200f).Bounds;
            Assert.That(region.Intersects(Box(100f)), Is.False);
            Assert.That(region.Intersects(Box(200f)), Is.True);
            region.SetVolume(new SplitVolume());
            Assert.That(region.Origin.x, Is.EqualTo(200f));
            Assert.That(region.BroadphaseBounds.center, Is.EqualTo(Vector3.zero));
        }

        [TestCase(typeof(BruteForceRegionSelection))]
        [TestCase(typeof(GridRegionSelection))]
        [TestCase(typeof(GridRegionSelection2D))]
        [TestCase(typeof(QuadtreeRegionSelection))]
        public void Selectors_UseRegionEnvelopeForIndexAndExactGeometryForResults(Type strategyType)
        {
            var strategy = (RegionSelectionStrategySO)ScriptableObject.CreateInstance(strategyType);
            try
            {
                var region = new RegionManager.Region { CachedBounds = Box(1000f).Bounds };
                region.SetVolume(new SplitVolume());
                strategy.Initialize(new[] { region });
                Assert.That(strategy.GetNearbyRegions(Box(0f)), Is.Empty);
                Assert.That(strategy.GetNearbyRegions(Box(-5f)), Is.EquivalentTo(new[] { region }));

                var gap = new RegionManager.Region { CachedBounds = Box(0f).Bounds };
                var occupied = new RegionManager.Region { CachedBounds = Box(5f).Bounds };
                strategy.Initialize(new[] { gap, occupied });
                Assert.That(strategy.GetNearbyRegions(new SplitVolume()), Is.EquivalentTo(new[] { occupied }));
            }
            finally { UnityEngine.Object.DestroyImmediate(strategy); }
        }

        [Test]
        public void Serialization_RoundTripsCustomVolumeAndLegacyFallback()
        {
            var region = new RegionManager.Region { CachedBounds = Box(100f).Bounds };
            region.SetVolume(Box(5f));
            region.Portals.Add(new RegionManager.Portal { WorldVolume = Box(20f) });
            var restored = JsonUtility.FromJson<RegionManager.Region>(JsonUtility.ToJson(region));
            Assert.That(restored.HasCustomVolume, Is.True);
            Assert.That(restored.Volume, Is.TypeOf<BoxStreamingVolume>());
            Assert.That(restored.Intersects(Box(5f)), Is.True);
            Assert.That(restored.Origin.x, Is.EqualTo(100f));
            Assert.That(restored.Portals[0].WorldVolume.BroadphaseBounds, Is.EqualTo(Box(20f).Bounds));

            region.SetVolume(null);
            restored = JsonUtility.FromJson<RegionManager.Region>(JsonUtility.ToJson(region));
            Assert.That(restored.HasCustomVolume, Is.False);
            Assert.That(restored.Intersects(Box(100f)), Is.True);
        }

        [TestCase(typeof(GridRegionSelection))]
        [TestCase(typeof(GridRegionSelection2D))]
        public void GridOverflowFallback_StillUsesExactGeometry(Type strategyType)
        {
            var strategy = (RegionSelectionStrategySO)ScriptableObject.CreateInstance(strategyType);
            try
            {
                var region = new RegionManager.Region();
                region.SetVolume(new SplitVolume());
                strategy.Initialize(new[] { region });
                // A long, thin box overlaps the envelope, but neither occupied part.
                var query = new BoxStreamingVolume(new Bounds(Vector3.zero, new Vector3(1f, 1f, 100000000f)));
                Assert.That(strategy.GetNearbyRegions(query), Is.Empty);
            }
            finally { UnityEngine.Object.DestroyImmediate(strategy); }
        }

        [Test]
        public void BoundsUnload_UsesExactRegionAndObserverGeometry()
        {
            var strategy = ScriptableObject.CreateInstance<BoundsIntersectionUnloadStrategy>();
            try
            {
                var region = new RegionManager.Region();
                region.SetVolume(new SplitVolume());
                Assert.That(strategy.ShouldUnload(Box(0f), null, region), Is.True);
                Assert.That(strategy.ShouldUnload(Box(5f), null, region), Is.False);
                region.SetVolume(null);
                region.CachedBounds = Box(0f).Bounds;
                Assert.That(strategy.ShouldUnload(new SplitVolume(), null, region), Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(strategy); }
        }

        [Test]
        public void Controller_ActivationUsesExactGeometry()
        {
            var owner = new GameObject("Volume controller test");
            owner.SetActive(false); // Do not start loaders or run Awake during this unit test.
            try
            {
                var manager = owner.AddComponent<RegionManager>();
                var controller = owner.AddComponent<RegionStreamingController>();
                var region = new RegionManager.Region();
                region.SetVolume(new SplitVolume());
                manager.MarkRegionLoaded(region);
                typeof(RegionStreamingController).GetField("_regionManager", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(controller, manager);
                var loaded = (HashSet<RegionManager.Region>)typeof(RegionStreamingController)
                    .GetField("_loadedRegions", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller);
                loaded.Add(region);
                var activate = typeof(RegionStreamingController).GetMethod("HandleActivation", BindingFlags.Instance | BindingFlags.NonPublic);
                activate.Invoke(controller, new object[] { Box(0f) });
                Assert.That(controller.ActiveRegions, Is.Empty);
                activate.Invoke(controller, new object[] { Box(5f) });
                Assert.That(controller.ActiveRegions, Does.Contain(region));
                activate.Invoke(controller, new object[] { Box(0f) });
                Assert.That(controller.ActiveRegions, Is.Empty);

                var provider = new VolumeProvider { Query = Box(0f) };
                controller.SetStreamingVolumeProvider(provider);
                var complete = typeof(RegionStreamingController).GetMethod("OnLoadComplete", BindingFlags.Instance | BindingFlags.NonPublic);
                complete.Invoke(controller, new object[] { region });
                Assert.That(controller.ActiveRegions, Is.Empty);
                provider.Query = Box(-5f);
                complete.Invoke(controller, new object[] { region });
                Assert.That(controller.ActiveRegions, Does.Contain(region));
            }
            finally { UnityEngine.Object.DestroyImmediate(owner); }
        }

        [Test]
        public void Controller_DesiredRegionsAndPortalProtectionUseExactGeometry()
        {
            var owner = new GameObject("Volume desired-region test");
            owner.SetActive(false);
            var selector = ScriptableObject.CreateInstance<BruteForceRegionSelection>();
            var unload = ScriptableObject.CreateInstance<PortalAwareBoundsUnloadStrategy>();
            try
            {
                var manager = owner.AddComponent<RegionManager>();
                var controller = owner.AddComponent<RegionStreamingController>();
                var region = new RegionManager.Region();
                region.SetVolume(new SplitVolume());
                selector.Initialize(new[] { region });
                typeof(RegionManager).GetField("_runtimeRegionSelectionStrategy", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(manager, selector);
                typeof(RegionStreamingController).GetField("_regionManager", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(controller, manager);
                var desired = typeof(RegionStreamingController).GetMethod("UpdateDesiredRegions", BindingFlags.Instance | BindingFlags.NonPublic);
                desired.Invoke(controller, new object[] { Box(0f) });
                Assert.That(controller.DesiredRegions, Is.Empty);
                desired.Invoke(controller, new object[] { Box(-5f) });
                Assert.That(controller.DesiredRegions, Does.Contain(region));

                var target = new RegionManager.Region { CachedBounds = Box(100f).Bounds };
                var source = new RegionManager.Region { CachedBounds = Box(200f).Bounds };
                var portal = new RegionManager.Portal { WorldVolume = new SplitVolume() };
                // Empty names are sufficient for the unload link comparison in this focused test.
                typeof(RegionManager.Portal).GetField("<TargetRegionName>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(portal, target.RegionName);
                source.Portals.Add(portal);
                source.RebuildPortalWorldBounds();
                manager.loadedRegions.Add(source);
                Assert.That(unload.ShouldUnload(Box(0f), manager, target), Is.True);
                Assert.That(unload.ShouldUnload(Box(5f), manager, target), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
                if (selector != null) UnityEngine.Object.DestroyImmediate(selector);
                UnityEngine.Object.DestroyImmediate(unload);
            }
        }

    }
}
