# Weapon System

WeaponSystem is intentionally outside ActionGraph. The dependency direction is one-way:

`WeaponSystem -> ActionGraph`

ActionGraph contains only generic graph execution, configuration, conditions, and editor tooling. WeaponSystem owns weapon contracts, damage/projectile types, attack routing, weapon contexts, and all weapon-specific graph nodes.

## Folder layout

- `ActionGraph/Nodes` contains weapon-specific graph nodes and their data providers.
- `ActionGraph/Conditions` contains combo and weapon graph conditions.
- `ActionGraph/Context` contains `WeaponContext`, data selection, and node utilities.
- `StateMachine/Actions` contains the FSM-to-weapon handoff and cancellation actions.
- `Runtime/Controllers` contains primary/secondary attack routing and player input ownership.
- `Runtime/Components` contains executable weapon and projectile behaviours.
- `Runtime/Systems` contains inventory and weapon-generation systems.
- `Core/Contracts` contains weapon and damage interfaces.
- `Core/Data` contains shared weapon data contracts.
- `Content` contains prefabs and ScriptableObject assets used by the feature.
- `Configs` contains weapon graph assets.

## Runtime ownership

- `WeaponAttackController` selects the primary or secondary weapon and owns player input/FSM handoff.
- `AttackAction` starts the selected weapon only after its state machine enters the Attack state.
- `ComboWeapon` executes its assigned `ActionGraphAsset` and records buffered combo input. It does not register input or control a state machine.
- AI attacks are initiated by their FSM `AttackAction`; they do not use player input commands.

## Player setup

1. Add one `WeaponAttackController` to the player actor.
2. Assign a component implementing `IWeapon` to the Primary Weapon slot.
3. Optionally assign another `IWeapon` to the Secondary Weapon slot.
4. Keep weapon components such as `ComboWeapon` focused on executing their action graph.

Player attack input is ignored while `FSMCharacterController` is disabled. Disabling it during an active attack cancels the selected weapon.

## ActionGraph integration

Weapon-specific nodes remain in this feature folder and depend on the generic ActionGraph API. This lets ActionGraph move into a reusable package later without making that package depend on WeaponSystem.

Weapon nodes derive from `WeaponActionNode<T>`, which centralizes `WeaponContext` validation and attack-indexed data selection. Shared animation-exit waiting also lives in `WeaponNodeUtility`, so individual nodes only contain their weapon behavior.

ActionGraph execution uses Unity's pooled `Awaitable` API end to end instead of `System.Threading.Tasks.Task`. Immediate actions stay on the Unity main thread, frame and timer waits receive the graph cancellation token directly, and Parallel starts every child before awaiting all branches. Parallel rents its temporary Awaitable buffer from `ArrayPool`, avoiding a new managed array on each execution while remaining safe when executions overlap during cancellation.

SubGraph nodes compile every referenced graph once during initialization and reuse isolated runtime instances on later executions. Each invocation resets the cached graph to preserve the former build-per-call selector behavior, parent resets propagate through every cached variant, and recursive SubGraph references fail early with the asset path instead of constructing forever.

The ActionGraph window provides editor-only live debugging during Play Mode. `Live Debug` highlights running, completed, cancelled, and failed nodes, while the bounded trace panel records the latest 200 state changes and exception messages. `Clear Trace` resets both the panel and node highlights. All runtime observation wrappers and events are guarded by `UNITY_EDITOR`; player builds contain the original undecorated execution tree and no debug calls.

Every graph node displays a short description in the ActionGraph editor. Edit its multiline `Description` field to save a per-node override in the graph asset, or use `Reset to Default` to restore generated text. Weapon actions provide their defaults through `ActionNodeMenuAttribute`; flow and condition nodes receive contextual defaults from the editor, and third-party actions without custom text receive a readable fallback.

Every graph has a Sequence or Parallel root. The root is structural and never occupies the canvas; loading a graph immediately displays its children. A nested Sequence or Parallel node appears as a normal card without an extra `GROUP` label. Enter it with the chevron, by double-clicking its title, or by selecting it and pressing Enter. Its container is then hidden so only its direct children are shown. Sequence children are numbered by execution order and provide up/down controls to run earlier or later. Parallel children start together, so their list order has no execution-order meaning. Use `Add Node` while inside a Sequence or Parallel node, `← Parent` to move up one level, or a clickable breadcrumb to jump directly to any ancestor. These navigation changes affect only the editor presentation—the runtime graph and execution order are unchanged.

The Graph object field replaces the older `Use Selection` shortcut. `Create Root` appears only when the loaded graph has no root; normal graph assets already contain one. Closed Sequence and Parallel cards intentionally have no Contents port or add button—their children are managed from inside the node.

The current combo graph uses a reusable loop:

- set up weapon models
- begin the current attack
- play the indexed attack animation
- execute movement, hit detection, damage, effects, and combo-input waiting
- advance only when buffered input is accepted
- reset when the loop finishes

