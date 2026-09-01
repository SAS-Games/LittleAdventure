using System;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SAS.Checkpoints.Tests
{
    public sealed class CheckpointManagerTests
    {
        private InMemoryProgressService _progressService;
        private CheckpointManager _manager;
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _progressService = new InMemoryProgressService();
            _manager = new CheckpointManager(_progressService);
            _root = new GameObject("Checkpoint tests");
            _root.SetActive(false);
        }

        [TearDown]
        public void TearDown()
        {
            _manager.Dispose();
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void DuplicateCheckpointId_KeepsFirstRegistration()
        {
            Checkpoint first = CreateCheckpoint("CP_01", 1);
            Checkpoint duplicate = CreateCheckpoint("CP_01", 2);
            _manager.RegisterCheckpoint(first);

            LogAssert.Expect(
                LogType.Error,
                "Duplicate checkpoint ID 'CP_01'. Objects: " +
                "'CP_01' and 'CP_01'.");
            _manager.RegisterCheckpoint(duplicate);

            Assert.That(_manager.CanActivate(first), Is.True);
            Assert.That(_manager.CanActivate(duplicate), Is.False);
        }

        [Test]
        public void RestoreBeforeRegistration_ReconnectsWhenObjectRegisters()
        {
            _progressService.ActiveCheckpoint = CreateActiveData("CP_01");
            _manager.RestoreFromProgress();

            Checkpoint checkpoint = CreateCheckpoint("CP_01", 1);
            _manager.RegisterCheckpoint(checkpoint);

            Assert.That(_manager.IsActive(checkpoint), Is.True);
        }

        [Test]
        public async Task UnregisterActiveCheckpoint_PreservesIdAndClearsObject()
        {
            Checkpoint checkpoint = CreateCheckpoint("CP_01", 1);
            _manager.RegisterCheckpoint(checkpoint);
            await _manager.ActivateAsync(checkpoint);

            _manager.UnregisterCheckpoint(checkpoint);

            Assert.That(_manager.IsActive(checkpoint), Is.False);

            _manager.RegisterCheckpoint(checkpoint);

            Assert.That(_manager.IsActive(checkpoint), Is.True);
        }

        [Test]
        public void NoActiveCheckpoint_UsesDefaultSpawnPointGroup()
        {
            SpawnPointGroup group = CreateGroup("Default_Group");
            SetField(group, "<IsDefault>k__BackingField", true);

            SpawnPoint point = group.gameObject.AddComponent<SpawnPoint>();
            SetField(group, "m_SpawnPoints", new[] { point });

            _manager.RegisterGroup(group);

            Assert.That(
                _manager.TryGetSpawnPoint(0, out SpawnPoint result),
                Is.True);
            Assert.That(result, Is.SameAs(point));
        }

        [Test]
        public void DuplicateGroupId_KeepsFirstRegistration()
        {
            SpawnPointGroup first = CreateGroup("CP_01_Group");
            SpawnPointGroup duplicate = CreateGroup("CP_01_Group");
            SpawnPoint firstPoint = first.gameObject.AddComponent<SpawnPoint>();
            SpawnPoint duplicatePoint = duplicate.gameObject.AddComponent<SpawnPoint>();
            SetField(first, "m_SpawnPoints", new[] { firstPoint });
            SetField(duplicate, "m_SpawnPoints", new[] { duplicatePoint });
            _progressService.ActiveCheckpoint = CreateActiveData("CP_01");
            _manager.RegisterGroup(first);

            LogAssert.Expect(
                LogType.Error,
                "Duplicate spawn-point group ID 'CP_01_Group'. " +
                "Objects: 'CP_01_Group' and 'CP_01_Group'.");
            _manager.RegisterGroup(duplicate);

            Assert.That(
                _manager.TryGetSpawnPoint(0, out SpawnPoint result),
                Is.True);
            Assert.That(result, Is.SameAs(firstPoint));
        }

        [Test]
        public async Task BackwardActivation_RequiresExplicitPermission()
        {
            Checkpoint later = CreateCheckpoint("CP_02", 2);
            Checkpoint earlier = CreateCheckpoint("CP_01", 1);
            _manager.RegisterCheckpoint(later);
            _manager.RegisterCheckpoint(earlier);
            await _manager.ActivateAsync(later);

            Assert.That(_manager.CanActivate(earlier), Is.False);

            SetField(earlier, "m_AllowBackwardActivation", true);

            Assert.That(_manager.CanActivate(earlier), Is.True);
        }

        [Test]
        public async Task ActiveCheckpointEvent_ReportsPreviousAndCurrent()
        {
            Checkpoint first = CreateCheckpoint("CP_01", 1);
            Checkpoint second = CreateCheckpoint("CP_02", 2);
            _manager.RegisterCheckpoint(first);
            _manager.RegisterCheckpoint(second);
            await _manager.ActivateAsync(first);

            Checkpoint eventPrevious = null;
            Checkpoint eventCurrent = null;
            _manager.ActiveCheckpointChanged += (previous, current) =>
            {
                eventPrevious = previous;
                eventCurrent = current;
            };

            await _manager.ActivateAsync(second);

            Assert.That(eventPrevious, Is.SameAs(first));
            Assert.That(eventCurrent, Is.SameAs(second));
        }

        private Checkpoint CreateCheckpoint(
            string id,
            int order)
        {
            GameObject target = new GameObject(id);
            target.transform.SetParent(_root.transform);
            Checkpoint checkpoint = target.AddComponent<Checkpoint>();
            CheckpointDefinition definition = new CheckpointDefinition();
            SetField(definition, "m_Id", id);
            SetField(definition, "m_Order", order);
            SetField(checkpoint, "m_Definition", definition);
            return checkpoint;
        }

        private SpawnPointGroup CreateGroup(string id)
        {
            GameObject target = new GameObject(id);
            target.transform.SetParent(_root.transform);
            SpawnPointGroup group = target.AddComponent<SpawnPointGroup>();
            SetField(group, "<SpawnPointGroupId>k__BackingField", id);
            return group;
        }

        private static ActiveCheckpointData CreateActiveData(string id)
        {
            return new ActiveCheckpointData(
                id,
                "Arena",
                id + "_Group",
                Vector3.zero,
                Quaternion.identity);
        }

        private static void SetField(
            object target,
            string fieldName,
            object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private sealed class InMemoryProgressService :
            ICheckpointProgressService
        {
            public event Action Initialized
            {
                add { }
                remove { }
            }

            public event Action<string> CheckpointCompleted
            {
                add { }
                remove { }
            }

            public event Action ProgressReset
            {
                add { }
                remove { }
            }

            public bool IsInitialized => true;
            public ActiveCheckpointData ActiveCheckpoint;

            public Task InitializeAsync(int userId)
            {
                return Task.CompletedTask;
            }

            public bool IsCompleted(string checkpointId)
            {
                return false;
            }

            public ActiveCheckpointData GetActiveCheckpoint()
            {
                return Clone(ActiveCheckpoint);
            }

            public Task<bool> ActivateCheckpointAsync(
                ActiveCheckpointData checkpointData)
            {
                if (ActiveCheckpoint != null &&
                    string.Equals(
                        ActiveCheckpoint.CheckpointId,
                        checkpointData.CheckpointId,
                        StringComparison.Ordinal))
                {
                    return Task.FromResult(false);
                }

                ActiveCheckpoint = Clone(checkpointData);
                return Task.FromResult(true);
            }

            public Task ResetAsync()
            {
                ActiveCheckpoint = null;
                return Task.CompletedTask;
            }

            private static ActiveCheckpointData Clone(
                ActiveCheckpointData data)
            {
                if (data == null)
                    return null;

                return new ActiveCheckpointData(
                    data.CheckpointId,
                    data.SceneName,
                    data.SpawnPointGroupId,
                    data.FallbackPosition,
                    data.FallbackRotation);
            }
        }
    }
}
