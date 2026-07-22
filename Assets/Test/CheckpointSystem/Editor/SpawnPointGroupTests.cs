using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SAS.Checkpoints.Tests
{
    public sealed class SpawnPointGroupTests
    {
        private GameObject _root;
        private SpawnPointGroup _group;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Spawn point tests");
            _root.SetActive(false);
            _group = _root.AddComponent<SpawnPointGroup>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void FirstAndRandomAvailable_IgnoreOccupiedAndNullPoints()
        {
            SpawnPoint occupied = CreatePoint("Occupied");
            SpawnPoint available = CreatePoint("Available");
            occupied.Assign(new GameObject("Player"));
            SetPoints(occupied, null, available);

            Assert.That(
                _group.TryGetFirstAvailable(out SpawnPoint first),
                Is.True);
            Assert.That(first, Is.SameAs(available));
            Assert.That(
                _group.TryGetRandomAvailable(out SpawnPoint random),
                Is.True);
            Assert.That(random, Is.SameAs(available));

            Object.DestroyImmediate(occupied.SpawnedObject);
        }

        [Test]
        public void ByPlayerId_IsDeterministicAndSupportsNegativeIds()
        {
            SpawnPoint zero = CreatePoint("Zero");
            SpawnPoint one = CreatePoint("One");
            SpawnPoint two = CreatePoint("Two");
            SetPoints(zero, one, two);

            _group.TryGetByPlayerId(4, out SpawnPoint positive);
            _group.TryGetByPlayerId(-1, out SpawnPoint negative);

            Assert.That(positive, Is.SameAs(one));
            Assert.That(negative, Is.SameAs(two));
        }

        [Test]
        public void AvailableByPlayerId_SearchesCircularly()
        {
            SpawnPoint zero = CreatePoint("Zero");
            SpawnPoint one = CreatePoint("One");
            SpawnPoint two = CreatePoint("Two");
            one.Assign(new GameObject("Player"));
            SetPoints(zero, one, two);

            Assert.That(
                _group.TryGetAvailableByPlayerId(1, out SpawnPoint result),
                Is.True);
            Assert.That(result, Is.SameAs(two));

            Object.DestroyImmediate(one.SpawnedObject);
        }

        [Test]
        public void AllOccupied_DoesNotReturnAnAvailablePoint()
        {
            SpawnPoint zero = CreatePoint("Zero");
            SpawnPoint one = CreatePoint("One");
            zero.Assign(new GameObject("Player 0"));
            one.Assign(new GameObject("Player 1"));
            SetPoints(zero, one);

            Assert.That(
                _group.TryGetFirstAvailable(out _),
                Is.False);
            Assert.That(
                _group.TryGetRandomAvailable(out _),
                Is.False);
            Assert.That(
                _group.TryGetAvailableByPlayerId(0, out _),
                Is.False);
            Assert.That(
                _group.TryGetFallback(out SpawnPoint fallback),
                Is.True);
            Assert.That(fallback, Is.SameAs(zero));

            Object.DestroyImmediate(zero.SpawnedObject);
            Object.DestroyImmediate(one.SpawnedObject);
        }

        [Test]
        public void EmptyAndNullArrays_ReturnFalse()
        {
            SetPoints();
            Assert.That(_group.TryGetFirstAvailable(out _), Is.False);
            Assert.That(_group.TryGetByPlayerId(0, out _), Is.False);

            SetPointsArray(null);
            Assert.That(_group.TryGetRandomAvailable(out _), Is.False);
            Assert.That(_group.TryGetFallback(out _), Is.False);
        }

        private SpawnPoint CreatePoint(string name)
        {
            GameObject target = new GameObject(name);
            target.transform.SetParent(_root.transform);
            return target.AddComponent<SpawnPoint>();
        }

        private void SetPoints(params SpawnPoint[] points)
        {
            SetPointsArray(points);
        }

        private void SetPointsArray(SpawnPoint[] points)
        {
            FieldInfo field = typeof(SpawnPointGroup).GetField(
                "m_SpawnPoints",
                BindingFlags.Instance | BindingFlags.NonPublic);

            field.SetValue(_group, points);
        }
    }
}
