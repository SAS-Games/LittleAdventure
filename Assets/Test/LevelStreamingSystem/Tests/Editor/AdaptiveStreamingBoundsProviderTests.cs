using NUnit.Framework;
using UnityEngine;

namespace LevelStreaming.Tests
{
    public sealed class AdaptiveStreamingBoundsProviderTests
    {
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
