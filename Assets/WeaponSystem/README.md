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

The current combo graph uses a reusable loop:

- set up weapon models
- begin the current attack
- play the indexed attack animation
- execute movement, hit detection, damage, effects, and combo-input waiting
- advance only when buffered input is accepted
- reset when the loop finishes

