using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace LevelStreaming.Tests
{
    public sealed class AdaptiveStreamingBoundsProviderTests
    {
        private static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        [Test]
        public void DefaultProvider_SamplesAllBoxesAtCurrentPositionAndReusesGeometry()
        {
            var owner = new GameObject("Default volume provider test");
            owner.SetActive(false);
            try
            {
                IStreamingVolumeProvider provider = owner.AddComponent<DefaultStreamingBoundsProvider>();
                owner.transform.position = new Vector3(1f, 2f, 3f);
                Assert.That(provider.TryGetVolumes(out var first), Is.True);
                Assert.That(first.IsValid, Is.True);
                Assert.That(first.Activate, Is.TypeOf<BoxStreamingVolume>());
                Assert.That(first.Activate.BroadphaseBounds, Is.EqualTo(new Bounds(owner.transform.position, new Vector3(10f, 5f, 10f))));
                Assert.That(first.Load.BroadphaseBounds, Is.EqualTo(new Bounds(owner.transform.position, new Vector3(20f, 10f, 20f))));
                Assert.That(first.Unload.BroadphaseBounds, Is.EqualTo(new Bounds(owner.transform.position, new Vector3(30f, 15f, 30f))));

                owner.transform.position = new Vector3(10f, 20f, 30f);
                Assert.That(provider.TryGetVolumes(out var next), Is.True);
                Assert.That(next.Activate.BroadphaseBounds.center, Is.EqualTo(owner.transform.position));
                Assert.That(next.Load.BroadphaseBounds.center, Is.EqualTo(owner.transform.position));
                Assert.That(next.Unload.BroadphaseBounds.center, Is.EqualTo(owner.transform.position));
                Assert.That(next.ObserverPosition, Is.EqualTo(owner.transform.position));
                Assert.That(next.Revision, Is.GreaterThan(first.Revision));
                Assert.That(next.Activate, Is.SameAs(first.Activate));
                Assert.That(next.Load, Is.SameAs(first.Load));
                Assert.That(next.Unload, Is.SameAs(first.Unload));
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [Test]
        public void CameraProvider_PreservesRotatedAabbAndForwardBias()
        {
            var owner = new GameObject("Camera volume provider test");
            owner.SetActive(false);
            try
            {
                owner.AddComponent<Camera>();
                IStreamingVolumeProvider provider = owner.AddComponent<CameraFrustumStreamingBoundsProvider>();
                owner.transform.position = new Vector3(1f, 2f, 3f);
                owner.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
                Assert.That(provider.TryGetVolumes(out var sample), Is.True);
                Vector3 center = owner.transform.position + owner.transform.forward * 15f;
                Assert.That(sample.Activate.BroadphaseBounds.center, Is.EqualTo(center));
                Assert.That(sample.Load.BroadphaseBounds.center, Is.EqualTo(center));
                Assert.That(sample.Unload.BroadphaseBounds.center, Is.EqualTo(center));

                // The existing implementation encloses all eight oriented-box corners.
                Vector3 extents = new Vector3(15f, 8f, 15f * 1.5f) * 0.5f;
                for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 corner = center + owner.transform.rotation * Vector3.Scale(extents, new Vector3(x, y, z));
                    Assert.That(sample.Activate.BroadphaseBounds.SqrDistance(corner), Is.LessThan(0.00001f));
                }
                Assert.That(sample.ObserverPosition, Is.EqualTo(owner.transform.position));
                Assert.That(sample.Activate, Is.TypeOf<BoxStreamingVolume>());
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [Test]
        public void AdaptiveProvider_PreservesPredictionZoomMetadataAndNesting()
        {
            var owner = new GameObject("Adaptive volume provider test");
            owner.SetActive(false);
            try
            {
                var provider = owner.AddComponent<AdaptiveStreamingBoundsProvider>();
                SetField(provider, "m_ViewMode", StreamingViewMode.TargetWithZoomMultiplier);
                SetField(provider, "m_ZoomInput", StreamingZoomInput.Manual);
                owner.transform.position = new Vector3(3f, 4f, 5f);
                provider.SetManualZoom(0.75f);
                provider.SetVelocity(new Vector3(10f, 20f, 0f));
                Assert.That(((IStreamingVolumeProvider)provider).TryGetVolumes(out var first), Is.True);
                Assert.That(first.IsValid, Is.True);
                Assert.That(first.ObserverPosition, Is.EqualTo(owner.transform.position));
                Assert.That(first.Velocity, Is.EqualTo(new Vector3(10f, 0f, 0f)));
                Assert.That(first.NormalizedZoom, Is.EqualTo(0.75f));
                Assert.That(first.Activate.BroadphaseBounds.center, Is.EqualTo(owner.transform.position + Vector3.right * 15f));
                Assert.That(first.Load.BroadphaseBounds.Contains(first.Activate.BroadphaseBounds.min), Is.True);
                Assert.That(first.Load.BroadphaseBounds.Contains(first.Activate.BroadphaseBounds.max), Is.True);
                Assert.That(first.Unload.BroadphaseBounds.Contains(first.Load.BroadphaseBounds.min), Is.True);
                Assert.That(first.Unload.BroadphaseBounds.Contains(first.Load.BroadphaseBounds.max), Is.True);

                owner.transform.position = new Vector3(30f, 40f, 50f);
                provider.ResetPrediction();
                Assert.That(provider.TryGetVolumes(out var next), Is.True);
                Assert.That(next.ObserverPosition, Is.EqualTo(owner.transform.position));
                Assert.That(next.Revision, Is.GreaterThan(first.Revision));
                Assert.That(next.Activate, Is.SameAs(first.Activate));
                Assert.That(next.Load, Is.SameAs(first.Load));
                Assert.That(next.Unload, Is.SameAs(first.Unload));
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [Test]
        public void AdaptiveProvider_MissingObserverReturnsInvalidSampleAndRecovers()
        {
            var owner = new GameObject("Unavailable adaptive volume provider test");
            owner.SetActive(false);
            try
            {
                var provider = owner.AddComponent<AdaptiveStreamingBoundsProvider>();
                SetField(provider, "m_UseSelfWhenTargetMissing", false);
                Assert.That(provider.TryGetVolumes(out var sample), Is.False);
                Assert.That(sample.IsValid, Is.False);
                provider.SetTarget(owner.transform);
                Assert.That(provider.TryGetVolumes(out sample), Is.True);
                Assert.That(sample.IsValid, Is.True);
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [Test]
        public void Normalize_AcceptsReversedRangesAndClamps()
        {
            Assert.That(AdaptiveStreamingBoundsMath.Normalize(5f, new Vector2(10f, 0f)), Is.EqualTo(0.5f));
            Assert.That(AdaptiveStreamingBoundsMath.Normalize(-1f, new Vector2(0f, 10f)), Is.Zero);
            Assert.That(AdaptiveStreamingBoundsMath.Normalize(11f, new Vector2(0f, 10f)), Is.EqualTo(1f));
        }

        [Test]
        public void CalculatePrediction_UsesDeadZoneAndMaximumDistance()
        {
            Vector3 belowDeadZone = AdaptiveStreamingBoundsMath.CalculatePrediction(
                new Vector3(0.1f, 0f, 0f), 2f, 0.25f, 100f);
            Vector3 clamped = AdaptiveStreamingBoundsMath.CalculatePrediction(
                new Vector3(100f, 0f, 0f), 2f, 0.25f, 25f);

            Assert.That(belowDeadZone, Is.EqualTo(Vector3.zero));
            Assert.That(clamped, Is.EqualTo(new Vector3(25f, 0f, 0f)));
        }

        [Test]
        public void ProjectToStreamingSpace_RemovesOnlyTheConfiguredAxis()
        {
            Vector3 value = new(1f, 2f, 3f);

            Assert.That(
                AdaptiveStreamingBoundsProvider.ProjectToStreamingSpace(value, StreamingSpace.Full3D),
                Is.EqualTo(value));
            Assert.That(
                AdaptiveStreamingBoundsProvider.ProjectToStreamingSpace(value, StreamingSpace.GroundPlaneXZ),
                Is.EqualTo(new Vector3(1f, 0f, 3f)));
            Assert.That(
                AdaptiveStreamingBoundsProvider.ProjectToStreamingSpace(value, StreamingSpace.GroundPlaneXY),
                Is.EqualTo(new Vector3(1f, 2f, 0f)));
        }

        [Test]
        public void ContractBounds_ExpandsImmediatelyButShrinksAtConfiguredRate()
        {
            Bounds current = new(Vector3.zero, Vector3.one * 10f);
            Bounds expanded = AdaptiveStreamingBoundsProvider.ContractBounds(
                current, new Bounds(Vector3.zero, Vector3.one * 20f), 2f, 0.5f);
            Bounds contracted = AdaptiveStreamingBoundsProvider.ContractBounds(
                current, new Bounds(Vector3.zero, Vector3.one * 4f), 2f, 0.5f);

            Assert.That(expanded.min, Is.EqualTo(Vector3.one * -10f));
            Assert.That(expanded.max, Is.EqualTo(Vector3.one * 10f));
            Assert.That(contracted.min, Is.EqualTo(Vector3.one * -4f));
            Assert.That(contracted.max, Is.EqualTo(Vector3.one * 4f));
        }

        [Test]
        public void Encapsulate_GuaranteesOuterBoundsContainInnerBounds()
        {
            Bounds outer = new(Vector3.zero, Vector3.one * 2f);
            Bounds inner = new(new Vector3(5f, -3f, 2f), new Vector3(4f, 6f, 8f));

            Bounds result = AdaptiveStreamingBoundsProvider.Encapsulate(outer, inner);

            Assert.That(result.Contains(inner.min), Is.True);
            Assert.That(result.Contains(inner.max), Is.True);
            Assert.That(result.Contains(outer.min), Is.True);
            Assert.That(result.Contains(outer.max), Is.True);
        }

        [Test]
        public void GroundFootprint_ProjectsCameraCornersAndUsesConfiguredHeight()
        {
            var cameraObject = new GameObject("Streaming bounds test camera");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.aspect = 1f;
                camera.fieldOfView = 60f;
                camera.transform.position = new Vector3(0f, 10f, -10f);
                camera.transform.rotation = Quaternion.LookRotation(Vector3.zero - camera.transform.position);

                bool success = AdaptiveStreamingBoundsMath.TryCreateGroundFootprint(
                    camera, 0f, 100f, 20f, out Bounds footprint);

                Assert.That(success, Is.True);
                Assert.That(footprint.center.y, Is.EqualTo(0f).Within(0.001f));
                Assert.That(footprint.size.y, Is.EqualTo(20f).Within(0.001f));
                Assert.That(footprint.size.x, Is.GreaterThan(0f));
                Assert.That(footprint.size.z, Is.GreaterThan(0f));
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
