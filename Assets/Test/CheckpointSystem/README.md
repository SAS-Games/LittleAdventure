# Checkpoint and respawn system

This folder is the single checkpoint implementation. The legacy root-level
`PlayerCheckpointManager` and `SpawnPointGroup` have been removed.

`CheckpointProgressService` owns saved completion history and the active
checkpoint record. `CheckpointManager` owns loaded checkpoints and spawn-point
groups. `CheckpointRespawnService` teleports a player to the active checkpoint,
or to the loaded default group when the player has not activated a checkpoint
yet.

`CheckpointSystemInstaller` creates, initializes, registers, and disposes the
checkpoint progress, manager, respawn, and optional scene-respawn services. The
core has no references to `ISaveSystem`, `IUserModel`, `IPlayerSetupModel`, or
`SceneGroupLoadedEvent`. It still uses the shared `SAS.Core.TagSystem` binder
for service registration and component injection, plus Unity APIs.

The portable contracts are:

- `ICheckpointSaveAdapter` for connecting checkpoint data to a save system.
- `ICheckpointUserIdProvider` for selecting a user or save slot.
- `ICheckpointPlayerProvider` for enumerating players to respawn.
- `ICheckpointSceneLoadNotifier` for notifying the core after a scene load.

All four adapters are optional. Without `ICheckpointSaveAdapter`, progress is
kept only for the lifetime of the checkpoint services and no save API is called.
Without `ICheckpointUserIdProvider`, user ID `0` is used. Automatic respawn after
scene loads is enabled only when both the player and scene-load adapters are
provided.

## Game integration

Copy this `CheckpointSystem` folder to reuse the core. Put adapters that know
about a particular game's models and events in a separate sibling folder. In
LittleAdventure that folder is `CheckpointSystem_Game`; its namespaced
`CheckpointSystemInstaller` adapts the existing save system,
user model, player setup model, and scene-group event.

The game installer uses the explicit composition-root overload:

```csharp
CheckpointSystemInstaller.Install(
    context,
    saveAdapter,         // optional; omit for session-only progress
    userIdProvider,      // optional; defaults to user ID 0
    playerProvider,       // optional as a pair
    sceneLoadNotifier);   // optional as a pair
```

A game only implements the checkpoint-owned interfaces for the features it
needs. Calling `CheckpointSystemInstaller.Install(context)` creates a working
session-only system. To persist progress, implement `ICheckpointSaveAdapter`
using the game's existing save service. If the game does not need automatic
respawning on scene loads, omit the last two adapters.

## Scene setup

Use this hierarchy as a starting point:

```text
Checkpoint_CP01
|-- Checkpoint
|-- Trigger
|   |-- Collider (Is Trigger)
|   `-- CheckpointTrigger
|-- ActiveVisual
|-- InactiveVisual
`-- SpawnPointGroup
    |-- SpawnPoint_Player0
    |-- SpawnPoint_Player1
    |-- SpawnPoint_Player2
    `-- SpawnPoint_Player3
```

1. Add `SAS.Checkpoints.Checkpoint` to the checkpoint root.
2. Give its definition a stable, globally unique ID and an increasing order.
3. Add `SAS.Checkpoints.SpawnPointGroup` on the root or a child. Give the group
   a globally unique ID and populate it with `SAS.Checkpoints.SpawnPoint`
   components. Empty arrays are collected automatically from children.
4. Enable `Is Default` on exactly one loaded group when players should spawn
   there before any checkpoint has been activated. The included
   `SpawnPointGroup.prefab` is configured as a default group.
5. Assign the group to `Checkpoint`. If it is on the checkpoint root or below
   it, the component resolves it automatically.
6. Add a trigger collider and `CheckpointTrigger`; assign the checkpoint and
   use the player tag expected by the project.
7. Optionally assign active/inactive visual objects and a fallback transform.

`CompleteOnlyOnce` prevents a completed checkpoint from activating again.
`AllowBackwardActivation` permits returning to an earlier checkpoint and also
permits that completed checkpoint to become active again.

## Respawning a player

Inject the respawn service into a death or retry handler:

```csharp
using SAS.Checkpoints;
using SAS.Core.TagSystem;
using UnityEngine;

public sealed class PlayerDeathHandler : MonoBehaviour
{
    [Inject] private ICheckpointRespawnService _checkpointRespawnService;

    public bool Respawn(int playerId, GameObject player)
    {
        return _checkpointRespawnService.TryRespawn(playerId, player);
    }
}
```

The system first chooses a deterministic unoccupied point for the player. If
all points are occupied, it uses that player's deterministic point. If the
saved checkpoint group is not currently loaded, it uses the saved fallback
position and rotation. On scene-group loads, this call is already made for all
active player profiles.

When a player permanently leaves a still-loaded group, call
`SpawnPoint.Release(player)` if the point must become immediately available.
Destroyed Unity objects are treated as unoccupied automatically.

## Progress queries and reset

Inject `ICheckpointProgressService` to query or clear progress:

```csharp
bool completed = progressService.IsCompleted("CP_01");
await progressService.ResetAsync();
```

Do not call progress methods before `IsInitialized` is true. Scene checkpoint
components can be enabled during initialization; saved visual/object state is
restored through the service's initialization notification.

## Completion-driven object state

Add `CheckpointCompletionState` to an always-loaded controller object when
scene objects should change according to one checkpoint's completion state.

1. Set `Checkpoint Id` to the exact checkpoint ID, for example `CP_01`.
2. Add objects that should appear after completion to `Enable When Completed`.
3. Add objects that should disappear after completion to
   `Disable When Completed`.
4. Keep `Apply Incomplete State` enabled to apply the inverse state before the
   checkpoint is completed and after progress is reset. Disable it when the
   prefab or scene's initial active states should be left untouched.
5. Use `Disable Self When Completed` only when the controller object itself
   should disappear. Prefer keeping the component on a separate, always-loaded
   controller so it can continue responding to progress changes.

The state is applied after saved progress initializes, immediately when the
matching checkpoint is completed, and immediately after progress is reset.
The checkpoint ID comparison is ordinal and case-sensitive.

## Save format

When a persistence adapter is supplied, it determines where checkpoint data is
stored. LittleAdventure's adapter uses `Progress/CheckpointProgress` for the
active user. Only `CheckpointProgressData.CurrentVersion` (currently version 2)
is accepted. There is no legacy migration; delete incompatible checkpoint
progress data when the schema version changes.

IDs are ordinal and case-sensitive. Keep checkpoint and group IDs stable after
shipping because saved data refers to them directly.
