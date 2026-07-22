using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SAS.Checkpoints
{
    public sealed class CheckpointManager : ICheckpointManager, IDisposable
    {
        private readonly Dictionary<string, Checkpoint> _checkpoints = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SpawnPointGroup> _spawnPointGroups = new(StringComparer.Ordinal);
        private readonly ICheckpointProgressService _progressService;
        private readonly SemaphoreSlim _activationLock = new(1, 1);

        private Checkpoint _activeCheckpoint;
        private SpawnPointGroup _defaultSpawnPointGroup;
        private string _activeCheckpointId;
        private bool _isDisposed;

        public event Action<Checkpoint, Checkpoint> ActiveCheckpointChanged;
        public Checkpoint ActiveCheckpoint => _activeCheckpoint;
        public string ActiveCheckpointId => _activeCheckpointId;

        public bool HasActiveCheckpoint => !string.IsNullOrWhiteSpace(_activeCheckpointId);

        public CheckpointManager(ICheckpointProgressService progressService)
        {
            _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        }

        public void RestoreFromProgress()
        {
            ThrowIfDisposed();

            ActiveCheckpointData savedData = _progressService.GetActiveCheckpoint();
            _activeCheckpointId = savedData?.CheckpointId;

            Checkpoint checkpoint = null;

            if (!string.IsNullOrWhiteSpace(_activeCheckpointId))
                _checkpoints.TryGetValue(_activeCheckpointId, out checkpoint);

            SetRuntimeActiveCheckpoint(checkpoint);
        }

        public void RegisterCheckpoint(Checkpoint checkpoint)
        {
            ThrowIfDisposed();

            if (checkpoint == null)
                return;

            if (string.IsNullOrWhiteSpace(checkpoint.Id))
            {
                Debug.LogError($"Cannot register checkpoint '{checkpoint.name}' " + "because its ID is empty.", checkpoint);
                return;
            }

            if (_checkpoints.TryGetValue(checkpoint.Id, out Checkpoint existing) && existing != null && existing != checkpoint)
            {
                Debug.LogError($"Duplicate checkpoint ID '{checkpoint.Id}'. Objects: '{existing.name}' and '{checkpoint.name}'.", checkpoint);
                return;
            }

            _checkpoints[checkpoint.Id] = checkpoint;

            if (_activeCheckpoint == null && string.Equals(_activeCheckpointId, checkpoint.Id, StringComparison.Ordinal))
                SetRuntimeActiveCheckpoint(checkpoint);
        }

        public void UnregisterCheckpoint(Checkpoint checkpoint)
        {
            if (_isDisposed)
                return;

            if (checkpoint == null || string.IsNullOrWhiteSpace(checkpoint.Id))
                return;

            if (_checkpoints.TryGetValue(checkpoint.Id, out Checkpoint registered) && registered == checkpoint)
                _checkpoints.Remove(checkpoint.Id);

            if (_activeCheckpoint == checkpoint)
                SetRuntimeActiveCheckpoint(null);
        }

        public void RegisterGroup(SpawnPointGroup group)
        {
            ThrowIfDisposed();

            if (group == null)
                return;

            string groupId = group.SpawnPointGroupId;

            if (string.IsNullOrWhiteSpace(groupId))
            {
                Debug.LogError($"Cannot register spawn-point group " + $"'{group.name}' because its ID is empty.", group);
                return;
            }

            if (_spawnPointGroups.TryGetValue(groupId, out SpawnPointGroup existing) && existing != null && existing != group)
            {
                Debug.LogError($"Duplicate spawn-point group ID '{groupId}'. Objects: '{existing.name}' and '{group.name}'.", group);
                return;
            }

            _spawnPointGroups[groupId] = group;

            if (!group.IsDefault)
                return;

            if (_defaultSpawnPointGroup == null)
            {
                _defaultSpawnPointGroup = group;
                return;
            }

            if (_defaultSpawnPointGroup != group)
                Debug.LogError($"Multiple default spawn-point groups are loaded. " + $"Keeping '{_defaultSpawnPointGroup.name}' and " + $"ignoring '{group.name}' as the default.", group);
        }

        public void UnregisterGroup(SpawnPointGroup group)
        {
            if (_isDisposed)
                return;

            if (group == null || string.IsNullOrWhiteSpace(group.SpawnPointGroupId))
                return;

            if (_spawnPointGroups.TryGetValue(group.SpawnPointGroupId, out SpawnPointGroup registered) && registered == group)
                _spawnPointGroups.Remove(group.SpawnPointGroupId);

            if (_defaultSpawnPointGroup == group)
                _defaultSpawnPointGroup = FindRegisteredDefaultGroup();
        }

        public bool IsActive(Checkpoint checkpoint)
        {
            ThrowIfDisposed();

            if (checkpoint == null)
                return false;

            return _activeCheckpoint == checkpoint;
        }

        public bool CanActivate(Checkpoint checkpoint)
        {
            ThrowIfDisposed();

            if (checkpoint == null || !checkpoint.IsValid)
                return false;

            if (!_checkpoints.TryGetValue(checkpoint.Id, out Checkpoint registered) || registered != checkpoint)
                return false;

            if (IsActive(checkpoint))
                return false;

            if (_activeCheckpoint == null)
                return true;

            if (checkpoint.AllowBackwardActivation)
                return true;

            return checkpoint.Order >= _activeCheckpoint.Order;
        }

        public async Task<bool> ActivateAsync(Checkpoint checkpoint)
        {
            ThrowIfDisposed();

            if (checkpoint == null)
                return false;

            await _activationLock.WaitAsync();

            try
            {
                if (!CanActivate(checkpoint))
                    return false;

                ActiveCheckpointData checkpointData =
                    checkpoint.CreateProgressData();

                bool saved = await _progressService.ActivateCheckpointAsync(checkpointData);
                if (!saved)
                    return false;

                _activeCheckpointId = checkpoint.Id;
                SetRuntimeActiveCheckpoint(checkpoint);

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Checkpoint '{checkpoint.name}' activation failed.\n{exception}", checkpoint);
                return false;
            }
            finally
            {
                _activationLock.Release();
            }
        }

        public bool TryGetCheckpoint(string checkpointId, out Checkpoint checkpoint)
        {
            ThrowIfDisposed();

            checkpoint = null;

            if (string.IsNullOrWhiteSpace(checkpointId))
                return false;

            if (!_checkpoints.TryGetValue(checkpointId, out checkpoint))
                return false;

            if (checkpoint != null)
                return true;

            _checkpoints.Remove(checkpointId);
            checkpoint = null;
            return false;
        }

        public bool TryGetSpawnPointGroup(string spawnPointGroupId, out SpawnPointGroup group)
        {
            ThrowIfDisposed();

            group = null;

            if (string.IsNullOrWhiteSpace(spawnPointGroupId))
                return false;

            if (!_spawnPointGroups.TryGetValue(spawnPointGroupId, out group))
                return false;

            if (group != null)
                return true;

            _spawnPointGroups.Remove(spawnPointGroupId);
            group = null;
            return false;
        }

        public bool TryGetActiveSpawnPointGroup(out SpawnPointGroup group)
        {
            ThrowIfDisposed();

            group = null;

            if (_activeCheckpoint != null && _activeCheckpoint.SpawnPointGroup != null && _activeCheckpoint.SpawnPointGroup.isActiveAndEnabled)
            {
                group = _activeCheckpoint.SpawnPointGroup;
                return true;
            }

            if (!_progressService.IsInitialized)
                return false;

            ActiveCheckpointData savedData = _progressService.GetActiveCheckpoint();

            if (savedData == null || string.IsNullOrWhiteSpace(savedData.SpawnPointGroupId))
                return false;

            return TryGetSpawnPointGroup(savedData.SpawnPointGroupId, out group);
        }

        public bool TryGetSpawnPoint(int playerId, out SpawnPoint spawnPoint)
        {
            ThrowIfDisposed();
            spawnPoint = null;

            if (!TryGetActiveSpawnPointGroup(out SpawnPointGroup group) && !TryGetDefaultSpawnPointGroup(out group))
                return false;

            if (group.TryGetAvailableByPlayerId(playerId, out spawnPoint))
                return true;

            // The deterministic occupied point is an explicit fallback.
            return group.TryGetByPlayerId(playerId, out spawnPoint);
        }

        public bool TryGetDefaultSpawnPointGroup(out SpawnPointGroup group)
        {
            ThrowIfDisposed();

            if (_defaultSpawnPointGroup != null)
            {
                group = _defaultSpawnPointGroup;
                return true;
            }

            _defaultSpawnPointGroup = FindRegisteredDefaultGroup();
            group = _defaultSpawnPointGroup;
            return group != null;
        }

        private SpawnPointGroup FindRegisteredDefaultGroup()
        {
            foreach (SpawnPointGroup group in _spawnPointGroups.Values)
            {
                if (group != null && group.IsDefault)
                    return group;
            }

            return null;
        }

        private void SetRuntimeActiveCheckpoint(Checkpoint checkpoint)
        {
            Checkpoint previousCheckpoint = _activeCheckpoint;

            if (previousCheckpoint == checkpoint)
                return;

            _activeCheckpoint = checkpoint;
            RaiseActiveCheckpointChanged(previousCheckpoint, checkpoint);
        }

        private void RaiseActiveCheckpointChanged(Checkpoint previousCheckpoint, Checkpoint checkpoint)
        {
            Action<Checkpoint, Checkpoint> callbacks = ActiveCheckpointChanged;

            if (callbacks == null)
                return;

            foreach (Action<Checkpoint, Checkpoint> callback in callbacks.GetInvocationList())
            {
                try
                {
                    callback(previousCheckpoint, checkpoint);
                }
                catch (Exception exception)
                {
                    Debug.LogError("An active-checkpoint event subscriber failed.\n" + exception);
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(CheckpointManager));
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _activationLock.Dispose();

            _checkpoints.Clear();
            _spawnPointGroups.Clear();
            _defaultSpawnPointGroup = null;
            ActiveCheckpointChanged = null;

            _isDisposed = true;
        }
    }
}
