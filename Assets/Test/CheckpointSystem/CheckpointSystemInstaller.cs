using System;
using System.Threading.Tasks;
using SAS.Core.TagSystem;
using UnityEngine;

namespace SAS.Checkpoints
{
    public interface ICheckpointSystemInstaller : IBindable
    {
    }

    /// <summary>
    /// Installs only checkpoint-owned services. A game can bind implementations
    /// of the checkpoint dependency interfaces before this installer is created.
    /// </summary>
    public sealed class CheckpointSystemInstaller : ICheckpointSystemInstaller, IInitializable, IDestroyable, IDisposable
    {
        private readonly IContextBinder _contextBinder;

        [Inject(optional: true)] private ICheckpointProgressStore _progressStore;
        [Inject(optional: true)] private ICheckpointUserIdProvider _userIdProvider;
        [Inject(optional: true)] private ICheckpointPlayerProvider _playerProvider;
        [Inject(optional: true)] private ICheckpointSceneLoadNotifier _sceneLoadNotifier;

        private ICheckpointManager _checkpointManager;
        private ICheckpointProgressService _checkpointProgressService;
        private ICheckpointRespawnService _checkpointRespawnService;
        private CheckpointSceneRespawner _checkpointSceneRespawner;
        private Task _checkpointInitializationTask = Task.CompletedTask;
        private bool _isInstalled;
        private bool _isDisposed;

        public CheckpointSystemInstaller(IContextBinder context)
        {
            _contextBinder = context ?? throw new ArgumentNullException(nameof(context));

            if (context is not Component contextComponent)
                throw new ArgumentException("Checkpoint installation requires a component context.", nameof(context));

            contextComponent.Initialize(this);
        }

        private CheckpointSystemInstaller(IContextBinder context, ICheckpointProgressStore progressStore, ICheckpointUserIdProvider userIdProvider, ICheckpointPlayerProvider playerProvider, ICheckpointSceneLoadNotifier sceneLoadNotifier)
        {
            _contextBinder = context ?? throw new ArgumentNullException(nameof(context));
            _progressStore = progressStore;
            _userIdProvider = userIdProvider;
            _playerProvider = playerProvider;
            _sceneLoadNotifier = sceneLoadNotifier;
        }

        /// <summary>
        /// Composition-root entry point for games that construct their adapters
        /// in code instead of registering them in a Binder asset.
        /// </summary>
        public static CheckpointSystemInstaller Install(IContextBinder context, ICheckpointProgressStore progressStore, ICheckpointUserIdProvider userIdProvider, ICheckpointPlayerProvider playerProvider = null, ICheckpointSceneLoadNotifier sceneLoadNotifier = null)
        {
            CheckpointSystemInstaller installer = new(
                context,
                progressStore,
                userIdProvider,
                playerProvider,
                sceneLoadNotifier);

            installer.InstallCore();
            return installer;
        }

        void IInitializable.OnCreated(IContextBinder contextBinder)
        {
            InstallCore();
        }

        private void InstallCore()
        {
            if (_isInstalled)
                return;

            _progressStore ??= new JsonFileCheckpointProgressStore(Application.persistentDataPath);
            _userIdProvider ??= new FixedCheckpointUserIdProvider();

            ValidateOptionalRespawnDependencies();

            _checkpointProgressService = new CheckpointProgressService(_progressStore);
            _checkpointManager = new CheckpointManager(_checkpointProgressService);
            _checkpointRespawnService = new CheckpointRespawnService(_checkpointManager, _checkpointProgressService);

            BindCheckpointServices();

            _checkpointInitializationTask = InitializeCheckpointSystemAsync();

            if (_playerProvider != null && _sceneLoadNotifier != null)
            {
                _checkpointSceneRespawner = new CheckpointSceneRespawner(
                    _checkpointRespawnService,
                    _playerProvider,
                    _sceneLoadNotifier,
                    _checkpointInitializationTask);
            }

            _isInstalled = true;
            ObserveCheckpointInitializationFailuresAsync(_checkpointInitializationTask);
        }

        private void ValidateOptionalRespawnDependencies()
        {
            if ((_playerProvider == null) == (_sceneLoadNotifier == null))
                return;

            Debug.LogWarning(
                "Automatic checkpoint respawning requires both " +
                $"{nameof(ICheckpointPlayerProvider)} and " +
                $"{nameof(ICheckpointSceneLoadNotifier)}. " +
                "Automatic scene-load respawning is disabled.");
        }

        private void BindCheckpointServices()
        {
            _contextBinder.Add(typeof(ICheckpointProgressStore), _progressStore, default);
            _contextBinder.Add(typeof(ICheckpointUserIdProvider), _userIdProvider, default);

            if (_playerProvider != null)
                _contextBinder.Add(typeof(ICheckpointPlayerProvider), _playerProvider, default);

            if (_sceneLoadNotifier != null)
                _contextBinder.Add(typeof(ICheckpointSceneLoadNotifier), _sceneLoadNotifier, default);

            _contextBinder.Add(typeof(ICheckpointProgressService), _checkpointProgressService, default);
            _contextBinder.Add(typeof(ICheckpointManager), _checkpointManager, default);
            _contextBinder.Add(typeof(ICheckpointRespawnService), _checkpointRespawnService, default);
        }

        private async Task InitializeCheckpointSystemAsync()
        {
            await _checkpointProgressService.InitializeAsync(_userIdProvider.GetActiveUserId());
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
            Dispose();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _checkpointSceneRespawner?.Dispose();
            (_checkpointManager as IDisposable)?.Dispose();
            (_checkpointProgressService as IDisposable)?.Dispose();
            _isDisposed = true;
        }
    }
}
