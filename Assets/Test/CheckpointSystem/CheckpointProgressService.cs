using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SAS.Checkpoints
{
    public sealed class CheckpointProgressService : ICheckpointProgressService, IDisposable
    {
        private readonly ICheckpointProgressStore _progressStore;
        private readonly HashSet<string> _completedCheckpointIds = new(StringComparer.Ordinal);
        private readonly SemaphoreSlim _operationLock = new(1, 1);
        private readonly object _stateLock = new();

        private CheckpointProgressData _data;
        private int _userId;
        private bool _isInitialized;
        private bool _isDisposed;

        public event Action Initialized;
        public event Action<string> CheckpointCompleted;
        public event Action ProgressReset;

        public bool IsInitialized
        {
            get
            {
                lock (_stateLock)
                {
                    return _isInitialized;
                }
            }
        }

        public CheckpointProgressService(ICheckpointProgressStore progressStore)
        {
            _progressStore = progressStore ?? throw new ArgumentNullException(nameof(progressStore));
        }

        public async Task InitializeAsync(int userId)
        {
            ThrowIfDisposed();

            await _operationLock.WaitAsync();

            bool initialized = false;

            try
            {
                CheckpointProgressData loadedData = await _progressStore.LoadAsync(userId);

                loadedData ??= new CheckpointProgressData();
                ValidateVersion(loadedData);
                Sanitize(loadedData);

                lock (_stateLock)
                {
                    ThrowIfDisposed();
                    _userId = userId;
                    _data = loadedData;
                    RebuildCompletedCheckpointCache();
                    _isInitialized = true;
                }

                initialized = true;
            }
            finally
            {
                _operationLock.Release();
            }

            if (initialized)
                RaiseInitialized();
        }

        private void RaiseInitialized()
        {
            Action callbacks = Initialized;

            if (callbacks == null)
                return;

            foreach (Action callback in callbacks.GetInvocationList())
            {
                try
                {
                    callback();
                }
                catch (Exception exception)
                {
                    Debug.LogError("A checkpoint initialization subscriber failed.\n" + exception);
                }
            }
        }

        private void RaiseCheckpointCompleted(string checkpointId)
        {
            Action<string> callbacks = CheckpointCompleted;

            if (callbacks == null)
                return;

            foreach (Action<string> callback in callbacks.GetInvocationList())
            {
                try
                {
                    callback(checkpointId);
                }
                catch (Exception exception)
                {
                    Debug.LogError("A checkpoint completion subscriber failed.\n" + exception);
                }
            }
        }

        private void RaiseProgressReset()
        {
            Action callbacks = ProgressReset;

            if (callbacks == null)
                return;

            foreach (Action callback in callbacks.GetInvocationList())
            {
                try
                {
                    callback();
                }
                catch (Exception exception)
                {
                    Debug.LogError("A checkpoint reset subscriber failed.\n" + exception);
                }
            }
        }

        public bool IsCompleted(string checkpointId)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(checkpointId))
                return false;

            lock (_stateLock)
            {
                return _completedCheckpointIds.Contains(checkpointId);
            }
        }

        public ActiveCheckpointData GetActiveCheckpoint()
        {
            EnsureInitialized();

            lock (_stateLock)
            {
                return _data.ActiveCheckpoint?.Clone();
            }
        }

        public async Task<bool> ActivateCheckpointAsync(ActiveCheckpointData checkpointData)
        {
            EnsureInitialized();

            ValidateCheckpointData(checkpointData);

            await _operationLock.WaitAsync();

            bool checkpointWasCompleted = false;

            try
            {
                CheckpointProgressData candidate;

                lock (_stateLock)
                {
                    EnsureInitialized();

                    if (IsSameActiveCheckpoint(_data.ActiveCheckpoint, checkpointData))
                        return false;

                    candidate = _data.Clone();

                    if (!_completedCheckpointIds.Contains(checkpointData.CheckpointId))
                    {
                        candidate.CompletedCheckpointIds.Add(checkpointData.CheckpointId);
                        checkpointWasCompleted = true;
                    }

                    candidate.ActiveCheckpoint = checkpointData.Clone();
                }

                await SaveAsync(candidate);

                lock (_stateLock)
                {
                    Commit(candidate);
                }
            }
            finally
            {
                _operationLock.Release();
            }

            if (checkpointWasCompleted)
                RaiseCheckpointCompleted(checkpointData.CheckpointId);

            return true;
        }

        public async Task ResetAsync()
        {
            EnsureInitialized();

            await _operationLock.WaitAsync();

            try
            {
                CheckpointProgressData candidate = new CheckpointProgressData();

                lock (_stateLock)
                {
                    EnsureInitialized();
                }

                await SaveAsync(candidate);

                lock (_stateLock)
                {
                    Commit(candidate);
                }
            }
            finally
            {
                _operationLock.Release();
            }

            RaiseProgressReset();
        }

        private async Task SaveAsync(CheckpointProgressData data)
        {
            bool succeeded = await _progressStore.SaveAsync(_userId, data);
            if (!succeeded)
                throw new IOException("The save system rejected checkpoint progress data.");
        }

        private void RebuildCompletedCheckpointCache()
        {
            _completedCheckpointIds.Clear();

            foreach (string checkpointId in _data.CompletedCheckpointIds)
            {
                _completedCheckpointIds.Add(checkpointId);
            }
        }

        private void Commit(CheckpointProgressData data)
        {
            _data = data;
            RebuildCompletedCheckpointCache();
        }

        private static void ValidateVersion(CheckpointProgressData data)
        {
            if (data.Version == CheckpointProgressData.CurrentVersion)
                return;

            throw new NotSupportedException(
                $"Checkpoint progress version {data.Version} is not supported. Expected exactly {CheckpointProgressData.CurrentVersion}. Delete or replace the incompatible save data.");
        }

        private static void Sanitize(CheckpointProgressData data)
        {
            data.CompletedCheckpointIds ??= new List<string>();

            HashSet<string> ids = new(StringComparer.Ordinal);
            List<string> sanitizedIds = new();

            foreach (string checkpointId in data.CompletedCheckpointIds)
            {
                if (string.IsNullOrWhiteSpace(checkpointId) || !ids.Add(checkpointId))
                    continue;

                sanitizedIds.Add(checkpointId);
            }

            data.CompletedCheckpointIds = sanitizedIds;

            if (data.ActiveCheckpoint != null)
                ValidateCheckpointData(data.ActiveCheckpoint);
        }

        private static bool IsSameActiveCheckpoint(ActiveCheckpointData current, ActiveCheckpointData next)
        {
            if (current == null || next == null)
                return false;

            return string.Equals(current.CheckpointId, next.CheckpointId, StringComparison.Ordinal);
        }

        private static void ValidateCheckpointData(ActiveCheckpointData checkpointData)
        {
            if (checkpointData == null)
            {
                throw new ArgumentNullException(nameof(checkpointData));
            }

            ValidateCheckpointId(checkpointData.CheckpointId, nameof(checkpointData));
        }

        private static void ValidateCheckpointId(string checkpointId, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(checkpointId))
            {
                throw new ArgumentException("Checkpoint ID cannot be empty.", parameterName);
            }
        }

        private void EnsureInitialized()
        {
            ThrowIfDisposed();

            lock (_stateLock)
            {
                ThrowIfDisposed();

                if (!_isInitialized)
                    throw new InvalidOperationException($"{nameof(CheckpointProgressService)} " +
                                                        "has not been initialized.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(CheckpointProgressService));
        }

        public void Dispose()
        {
            lock (_stateLock)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;
                _isInitialized = false;
                _data = null;
                _completedCheckpointIds.Clear();
            }

            _operationLock.Dispose();
            Initialized = null;
            CheckpointCompleted = null;
            ProgressReset = null;
        }
    }
}
