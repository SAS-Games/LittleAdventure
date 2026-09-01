using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SAS.Core.TagSystem;
using SAS.SceneManagement;
using UnityEngine;

namespace SAS.Checkpoints.LittleAdventure
{
    /// <summary>
    /// LittleAdventure's composition root. This is the only checkpoint file
    /// that knows about the game's save, user, player, and scene APIs.
    /// </summary>
    public sealed class CheckpointSystemInstaller : ICheckpointSystemInstaller, IInitializable, IDestroyable
    {
        private readonly IContextBinder _contextBinder;

        [Inject(optional: true)] private ISaveSystem _saveSystem;
        [Inject(optional: true)] private IUserModel _userModel;
        [Inject(optional: true)] private IPlayerSetupModel _playerSetupModel;

        private global::SAS.Checkpoints.CheckpointSystemInstaller _coreInstaller;
        private CheckpointSceneLoadNotifier _sceneLoadNotifier;

        public CheckpointSystemInstaller(IContextBinder context)
        {
            _contextBinder = context ?? throw new ArgumentNullException(nameof(context));

            if (context is not Component contextComponent)
                throw new ArgumentException("Checkpoint installation requires a component context.", nameof(context));

            contextComponent.Initialize(this);
        }

        void IInitializable.OnCreated(IContextBinder contextBinder)
        {
            ICheckpointProgressStore progressStore = _saveSystem != null
                ? new CheckpointProgressStore(_saveSystem)
                : new JsonFileCheckpointProgressStore(Application.persistentDataPath);

            ICheckpointUserIdProvider userIdProvider = _userModel != null
                ? new CheckpointUserIdProvider(_userModel)
                : new FixedCheckpointUserIdProvider();

            ICheckpointPlayerProvider playerProvider = null;

            if (_playerSetupModel != null)
            {
                playerProvider = new CheckpointPlayerProvider(_playerSetupModel);
                _sceneLoadNotifier = new CheckpointSceneLoadNotifier();
            }

            _coreInstaller = global::SAS.Checkpoints.CheckpointSystemInstaller.Install(
                _contextBinder,
                progressStore,
                userIdProvider,
                playerProvider,
                _sceneLoadNotifier);
        }

        void IDestroyable.OnDestroyed(IContextBinder contextBinder)
        {
            _coreInstaller?.Dispose();
            _sceneLoadNotifier?.Dispose();
        }
    }

    public sealed class CheckpointProgressStore : ICheckpointProgressStore
    {
        private const string DirectoryName = "Progress";
        private const string FileName = "CheckpointProgress";

        private readonly ISaveSystem _saveSystem;

        public CheckpointProgressStore(ISaveSystem saveSystem)
        {
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
        }

        public Task<CheckpointProgressData> LoadAsync(int userId)
        {
            return _saveSystem.Load<CheckpointProgressData>(
                userId,
                DirectoryName,
                FileName);
        }

        public Task<bool> SaveAsync(int userId, CheckpointProgressData data)
        {
            return _saveSystem.Save(
                userId,
                DirectoryName,
                FileName,
                data);
        }
    }

    public sealed class CheckpointUserIdProvider : ICheckpointUserIdProvider
    {
        private readonly IUserModel _userModel;

        public CheckpointUserIdProvider(IUserModel userModel)
        {
            _userModel = userModel ?? throw new ArgumentNullException(nameof(userModel));
        }

        public int GetActiveUserId()
        {
            return _userModel.GetActiveUserId();
        }
    }

    public sealed class CheckpointPlayerProvider : ICheckpointPlayerProvider
    {
        private readonly IPlayerSetupModel _playerSetupModel;

        public CheckpointPlayerProvider(IPlayerSetupModel playerSetupModel)
        {
            _playerSetupModel = playerSetupModel ?? throw new ArgumentNullException(nameof(playerSetupModel));
        }

        public IEnumerable<CheckpointPlayer> GetPlayers()
        {
            IReadOnlyList<PlayerProfile> players = _playerSetupModel.Players;

            if (players == null)
                yield break;

            foreach (PlayerProfile player in players)
            {
                if (player != null)
                    yield return new CheckpointPlayer(player.Index, player.Character);
            }
        }
    }

    public sealed class CheckpointSceneLoadNotifier :
        ICheckpointSceneLoadNotifier,
        IDisposable
    {
        private readonly EventBinding<SceneGroupLoadedEvent> _sceneLoadedBinding;
        private bool _isDisposed;

        public event Action SceneLoaded;

        public CheckpointSceneLoadNotifier()
        {
            _sceneLoadedBinding = new EventBinding<SceneGroupLoadedEvent>(OnSceneGroupLoaded);
            EventBus<SceneGroupLoadedEvent>.Register(_sceneLoadedBinding);
        }

        private void OnSceneGroupLoaded(SceneGroupLoadedEvent sceneGroupLoadedEvent)
        {
            SceneLoaded?.Invoke();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            EventBus<SceneGroupLoadedEvent>.Deregister(_sceneLoadedBinding);
            SceneLoaded = null;
            _isDisposed = true;
        }
    }
}
