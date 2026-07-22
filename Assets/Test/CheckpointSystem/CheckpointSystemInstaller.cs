using System;
using System.Threading.Tasks;
using SAS.Core.TagSystem;
using UnityEngine;

namespace SAS.Checkpoints
{
    public interface ICheckpointSystemInstaller : IBindable
    {
    }

    public sealed class CheckpointSystemInstaller : ICheckpointSystemInstaller, IInitializable, IDestroyable
    {
        private readonly IContextBinder _contextBinder;

        [Inject(optional: true)] private ISaveSystem _saveSystem;
        [Inject(optional: true)] private IUserModel _userModel;
        [Inject(optional: true)] private IPlayerSetupModel _playerSetupModel;

        private CheckpointProgressService _checkpointProgressService;
        private CheckpointManager _checkpointManager;
        private CheckpointRespawnService _checkpointRespawnService;
        private CheckpointSceneRespawner _checkpointSceneRespawner;
        private Task _checkpointInitializationTask;

        public CheckpointSystemInstaller(IContextBinder context)
        {
            _contextBinder = context ?? throw new ArgumentNullException(nameof(context));

            if (context is not Component contextComponent)
                throw new ArgumentException("Checkpoint installation requires a component context.", nameof(context));

            contextComponent.Initialize(this);
        }

        void IInitializable.OnCreated(IContextBinder contextBinder)
        {
            _saveSystem ??= new JsonFileSaveSystem(Application.persistentDataPath);
            _userModel ??= new DummyUserModel();

            _checkpointProgressService = new CheckpointProgressService(_saveSystem);
            _checkpointManager = new CheckpointManager(_checkpointProgressService);
            _checkpointRespawnService = new CheckpointRespawnService(_checkpointManager, _checkpointProgressService);

            BindCheckpointServices();

            _checkpointInitializationTask = InitializeCheckpointSystemAsync();

            if (_playerSetupModel != null)
                _checkpointSceneRespawner = new CheckpointSceneRespawner(_checkpointRespawnService, _playerSetupModel, _checkpointInitializationTask);

            ObserveCheckpointInitializationFailuresAsync(_checkpointInitializationTask);
        }

        private void BindCheckpointServices()
        {
            _contextBinder.Add(typeof(ICheckpointProgressService), _checkpointProgressService, default);
            _contextBinder.Add(typeof(ICheckpointManager), _checkpointManager, default);
            _contextBinder.Add(typeof(ICheckpointRespawnService), _checkpointRespawnService, default);
        }

        private async Task InitializeCheckpointSystemAsync()
        {
            await _checkpointProgressService.InitializeAsync(_userModel.GetActiveUserId());
            _checkpointManager.RestoreFromProgress();
        }

        private static async void ObserveCheckpointInitializationFailuresAsync(Task checkpointInitializationTask)
        {
            try
            {
                await checkpointInitializationTask;
            }
            catch (Exception exception)
            {
                Debug.LogError("Checkpoint system initialization failed.\n" + exception);
            }
        }

        void IDestroyable.OnDestroyed(IContextBinder contextBinder)
        {
            _checkpointSceneRespawner?.Dispose();
            _checkpointManager?.Dispose();
            _checkpointProgressService?.Dispose();
        }
    }
}
