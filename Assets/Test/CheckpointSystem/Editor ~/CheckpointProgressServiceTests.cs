using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace SAS.Checkpoints.Tests
{
    public sealed class CheckpointProgressServiceTests
    {
        private FakeSaveAdapter _saveAdapter;
        private CheckpointProgressService _service;

        [SetUp]
        public void SetUp()
        {
            _saveAdapter = new FakeSaveAdapter();
            _service = new CheckpointProgressService(_saveAdapter);
        }

        [TearDown]
        public void TearDown()
        {
            _service.Dispose();
        }

        [Test]
        public async Task Initialize_WithNoSave_CreatesCurrentData()
        {
            await _service.InitializeAsync(7);

            Assert.That(_service.IsInitialized, Is.True);
            Assert.That(_service.GetActiveCheckpoint(), Is.Null);
            Assert.That(_service.IsCompleted("CP_01"), Is.False);
        }

        [Test]
        public async Task NoStore_TracksProgressForCurrentServiceLifetime()
        {
            using CheckpointProgressService service = new();

            await service.InitializeAsync(7);
            await service.ActivateCheckpointAsync(CreateActiveData("CP_01"));

            Assert.That(service.IsCompleted("CP_01"), Is.True);
            Assert.That(
                service.GetActiveCheckpoint().CheckpointId,
                Is.EqualTo("CP_01"));
        }

        [Test]
        public async Task Initialize_RaisesInitializedAfterStateIsReady()
        {
            bool wasReadyDuringEvent = false;
            _service.Initialized += () =>
            {
                wasReadyDuringEvent = _service.IsInitialized;
            };

            await _service.InitializeAsync(7);

            Assert.That(wasReadyDuringEvent, Is.True);
        }

        [Test]
        public void Initialize_WithVersionOne_RejectsSave()
        {
            _saveAdapter.Data = new CheckpointProgressData
            {
                Version = 1,
                CompletedCheckpointIds = new List<string> { "CP_01" }
            };

            Assert.ThrowsAsync<NotSupportedException>(
                async () => await _service.InitializeAsync(7));
        }

        [Test]
        public async Task Initialize_SanitizesCompletedIds()
        {
            _saveAdapter.Data = new CheckpointProgressData
            {
                CompletedCheckpointIds = new List<string>
                {
                    null,
                    string.Empty,
                    "CP_01",
                    "CP_01",
                    "CP_02"
                }
            };

            await _service.InitializeAsync(7);

            Assert.That(_service.IsCompleted("CP_01"), Is.True);
            Assert.That(_service.IsCompleted("CP_02"), Is.True);
        }

        [Test]
        public async Task Activate_CompletesAndActivatesInOneSave()
        {
            await _service.InitializeAsync(7);

            ActiveCheckpointData data = CreateActiveData("CP_01");

            Assert.That(
                await _service.ActivateCheckpointAsync(data),
                Is.True);
            Assert.That(_service.IsCompleted("CP_01"), Is.True);
            Assert.That(
                _service.GetActiveCheckpoint().CheckpointId,
                Is.EqualTo("CP_01"));
            Assert.That(_saveAdapter.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Activate_PreviouslyCompletedCheckpoint_CanBecomeActive()
        {
            await _service.InitializeAsync(7);
            await _service.ActivateCheckpointAsync(CreateActiveData("CP_01"));
            await _service.ActivateCheckpointAsync(CreateActiveData("CP_02"));

            Assert.That(
                await _service.ActivateCheckpointAsync(
                    CreateActiveData("CP_01")),
                Is.True);
            Assert.That(
                _service.GetActiveCheckpoint().CheckpointId,
                Is.EqualTo("CP_01"));
        }

        [Test]
        public async Task Activate_RaisesCompletionEventOnlyForNewCompletion()
        {
            await _service.InitializeAsync(7);
            List<string> completedIds = new();
            _service.CheckpointCompleted += completedIds.Add;

            await _service.ActivateCheckpointAsync(CreateActiveData("CP_01"));
            await _service.ActivateCheckpointAsync(CreateActiveData("CP_02"));
            await _service.ActivateCheckpointAsync(CreateActiveData("CP_01"));

            Assert.That(
                completedIds,
                Is.EqualTo(new[] { "CP_01", "CP_02" }));
        }

        [Test]
        public async Task Activate_CurrentCheckpoint_ReturnsFalseWithoutSaving()
        {
            await _service.InitializeAsync(7);
            await _service.ActivateCheckpointAsync(CreateActiveData("CP_01"));
            int saveCount = _saveAdapter.SaveCount;

            Assert.That(
                await _service.ActivateCheckpointAsync(
                    CreateActiveData("CP_01")),
                Is.False);
            Assert.That(_saveAdapter.SaveCount, Is.EqualTo(saveCount));
        }

        [Test]
        public async Task Reset_ClearsCompletedAndActiveData()
        {
            await _service.InitializeAsync(7);
            await _service.ActivateCheckpointAsync(CreateActiveData("CP_01"));

            await _service.ResetAsync();

            Assert.That(_service.IsCompleted("CP_01"), Is.False);
            Assert.That(_service.GetActiveCheckpoint(), Is.Null);
        }

        [Test]
        public async Task Reset_RaisesEventAfterStateIsCleared()
        {
            await _service.InitializeAsync(7);
            await _service.ActivateCheckpointAsync(CreateActiveData("CP_01"));
            bool wasClearedDuringEvent = false;

            _service.ProgressReset += () =>
            {
                wasClearedDuringEvent =
                    !_service.IsCompleted("CP_01") &&
                    _service.GetActiveCheckpoint() == null;
            };

            await _service.ResetAsync();

            Assert.That(wasClearedDuringEvent, Is.True);
        }

        [Test]
        public async Task SaveFailure_LeavesPreviousStateUnchanged()
        {
            await _service.InitializeAsync(7);
            await _service.ActivateCheckpointAsync(CreateActiveData("CP_01"));
            _saveAdapter.FailNextSave = true;

            Assert.ThrowsAsync<IOException>(
                async () => await _service.ActivateCheckpointAsync(
                    CreateActiveData("CP_02")));

            Assert.That(_service.IsCompleted("CP_02"), Is.False);
            Assert.That(
                _service.GetActiveCheckpoint().CheckpointId,
                Is.EqualTo("CP_01"));
        }

        private static ActiveCheckpointData CreateActiveData(
            string checkpointId)
        {
            return new ActiveCheckpointData(
                checkpointId,
                "Arena",
                checkpointId + "_SpawnGroup",
                Vector3.one,
                Quaternion.identity);
        }

        private sealed class FakeSaveAdapter : ICheckpointSaveAdapter
        {
            public CheckpointProgressData Data;
            public int SaveCount;
            public bool FailNextSave;

            public Task<CheckpointProgressData> LoadAsync(int userId)
            {
                if (Data != null)
                    return Task.FromResult(Clone(Data));

                return Task.FromResult(new CheckpointProgressData());
            }

            public Task<bool> SaveAsync(
                int userId,
                CheckpointProgressData data)
            {
                SaveCount++;

                if (FailNextSave)
                {
                    FailNextSave = false;
                    return Task.FromResult(false);
                }

                Data = Clone(data);

                return Task.FromResult(true);
            }

            private static CheckpointProgressData Clone(
                CheckpointProgressData data)
            {
                ActiveCheckpointData active = data.ActiveCheckpoint;

                return new CheckpointProgressData
                {
                    Version = data.Version,
                    CompletedCheckpointIds =
                        new List<string>(data.CompletedCheckpointIds),
                    ActiveCheckpoint = active == null
                        ? null
                        : new ActiveCheckpointData(
                            active.CheckpointId,
                            active.SceneName,
                            active.SpawnPointGroupId,
                            active.FallbackPosition,
                            active.FallbackRotation)
                };
            }
        }
    }
}
