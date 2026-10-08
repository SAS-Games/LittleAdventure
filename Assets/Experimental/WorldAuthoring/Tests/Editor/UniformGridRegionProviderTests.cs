using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace SAS.WorldStreaming.Editor.Tests
{
    public sealed class UniformGridRegionProviderTests
    {
        private WorldStreamingProfile m_Profile;
        private UniformGridRegionProvider m_Provider;

        [SetUp]
        public void SetUp()
        {
            m_Profile = ScriptableObject.CreateInstance<WorldStreamingProfile>();
            m_Provider = new UniformGridRegionProvider();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Profile);
        }

        [Test]
        public void Generate_XZGrid_CreatesExpectedBoundsAndPadding()
        {
            m_Profile.ConfigureUniformGrid(
                Vector3.zero,
                new Vector3(10f, 1f, 20f),
                new Vector3Int(2, 7, 2),
                true,
                -10f,
                50f,
                5f);

            IReadOnlyList<StreamingRegionDefinition> regions = m_Provider.Generate(m_Profile);

            Assert.That(regions, Has.Count.EqualTo(4));
            Assert.That(regions[0].GridCoordinate, Is.EqualTo(Vector3Int.zero));
            Assert.That(regions[0].Bounds.center, Is.EqualTo(new Vector3(5f, 20f, 10f)));
            Assert.That(regions[0].Bounds.size, Is.EqualTo(new Vector3(10f, 60f, 20f)));
            Assert.That(regions[0].LoadPadding, Is.EqualTo(new Vector3(5f, 0f, 5f)));
            Assert.That(regions[0].SceneName, Is.EqualTo("Region_0_0"));
        }

        [Test]
        public void Generate_RegeneratedCoordinates_PreserveStableRegionIds()
        {
            m_Profile.ConfigureUniformGrid(
                Vector3.zero,
                new Vector3(100f, 100f, 100f),
                new Vector3Int(2, 1, 1),
                true,
                0f,
                100f);

            IReadOnlyList<StreamingRegionDefinition> first = m_Provider.Generate(m_Profile);
            m_Profile.ReplaceRegions(first);
            string firstId = first[0].RegionId;
            string secondId = first[1].RegionId;

            m_Profile.SetWorldOrigin(new Vector3(25f, 0f, 0f));
            IReadOnlyList<StreamingRegionDefinition> regenerated = m_Provider.Generate(m_Profile);

            Assert.That(regenerated[0].RegionId, Is.EqualTo(firstId));
            Assert.That(regenerated[1].RegionId, Is.EqualTo(secondId));
            Assert.That(regenerated[0].Bounds.center.x, Is.EqualTo(75f));
        }

        [Test]
        public void Generate_ExcessiveGrid_ThrowsBeforeAllocatingRegions()
        {
            m_Profile.ConfigureUniformGrid(
                Vector3.zero,
                Vector3.one,
                new Vector3Int(101, 1, 100),
                true,
                0f,
                1f);

            Assert.Throws<System.InvalidOperationException>(() => m_Provider.Generate(m_Profile));
        }
    }
}
