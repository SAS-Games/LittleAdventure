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
        private FakeSaveSystem _saveSystem;
        private CheckpointProgressService _service;

        [SetUp]
        public void SetUp()
        {
            _saveSystem = new FakeSaveSystem();
            _service = new CheckpointProgressService(_saveSystem);
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
            _saveSystem.Data = new CheckpointProgressData
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
            _saveSystem.Data = new CheckpointProgressData
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
            Assert.That(await _service.CompleteAsync("CP_01"), Is.False);
        }

        [Test]
        public async Task Complete_AddsIdOnce()
        {
            await _service.InitializeAsync(7);

            Assert.That(await _service.CompleteAsync("CP_01"), Is.True);
            Assert.That(await _service.CompleteAsync("CP_01"), Is.False);
            Assert.That(_saveSystem.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Complete_RaisesEventAfterStateIsCommitted()
        {
            await _service.InitializeAsync(7);
            string completedId = null;
            bool wasCompletedDuringEvent = false;

            _service.CheckpointCompleted += checkpointId =>
            {
                completedId = checkpointId;
                wasCompletedDuringEvent =
                    _service.IsCompleted(checkpointId);
            };

            await _service.CompleteAsync("CP_01");

            Assert.That(completedId, Is.EqualTo("CP_01"));
            Assert.That(wasCompletedDuringEvent, Is.True);
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
            Assert.That(_saveSystem.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Activate_PreviouslyCompletedCheckpoint_CanBecomeActive()
        {
            await _service.InitializeAsync(7);
            await _service.CompleteAsync("CP_01");
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
            int saveCount = _saveSystem.SaveCount;

            Assert.That(
                await _service.ActivateCheckpointAsync(
                    CreateActiveData("CP_01")),
                Is.False);
            Assert.That(_saveSystem.SaveCount, Is.EqualTo(saveCount));
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
            _saveSystem.FailNextSave = true;

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

        private sealed class FakeSaveSystem : ISaveSystem
        {
            public CheckpointProgressData Data;
            public int SaveCount;
            public bool FailNextSave;

            public Task<T> Load<T>(
                int userId,
                string dirName,
                string fileName)
                where T : new()
            {
                if (typeof(T) == typeof(CheckpointProgressData) &&
                    Data != null)
                {
                    return Task.FromResult(
                        (T)(object)Clone(Data));
                }

                return Task.FromResult(new T());
            }

            public Task<bool> Save<T>(
                int userId,
                string dirName,
                string fileName,
                T data)
            {
                SaveCount++;

                if (FailNextSave)
                {
                    FailNextSave = false;
                    return Task.FromResult(false);
                }

                if (data is CheckpointProgressData checkpointData)
                {
                    Data = Clone(checkpointData);
                }

                return Task.FromResult(true);
            }

            public Task DeleteFile(
                int userId,
                string dir,
                string fileName)
            {
                Data = null;
                return Task.CompletedTask;
            }

            public Task DeleteDirectory(
                int userId,
                string dirName)
            {
                Data = null;
                return Task.CompletedTask;
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
