# ActionGraph Weapon Test

This folder is a dependency-light rewrite spike for the zipped `WeaponSystem`.

Create a starter config from:

`Assets > Create > Action Graph > Sample Weapon Graph`

The old package is component/data driven. This sample keeps the useful ideas but moves attack execution into an `ActionGraphAsset` graph:

1. `Weapon` owns an `ExecutionGraph` and exposes `Attack()`.
2. `WeaponInput` is a tiny test input binder.
3. Weapon action nodes configure animation, wait windows, movement, hitbox detection, damage, knockback, projectile spawning, and combo advancement.
4. `WeaponDamageReceiver` and `SimpleProjectile` are simple test components so the graph can be exercised without the old pool/FSM/event systems.

If the owner already has a blackboard used by the FSM, expose it through `IActionGraphBlackboard` or assign it to `ActionContext.Blackboard` before graph initialization. The generic blackboard nodes in `Runtime/Core` can then read and write the same data.

Suggested graph shape for one melee combo step:

`Sequence`

- `AnimationNode`
- `WaitSecondsNode`
- `MovementNode` optional
- `OverlapBoxNode`
- `DamageHitsNode`
- `KnockbackHitsNode`
- `WaitSecondsNode`
- `AdvanceComboNode`

For multi-step weapon data, put multiple values in each provider data array. Weapon nodes read the value at `WeaponContext.CurrentAttackIndex`, so Attack 1 uses `Data[0]`, Attack 2 uses `Data[1]`, and so on. Use `WeaponComboStepCondition` branches only when each combo step needs a very different graph shape.

## Sword combo spike

The combo weapon rewrite no longer needs `WeaponDataSO` at runtime. The editor generator can read an old `SAS.WeaponSystem.WeaponDataSO` once and bake its values into graph node data.

1. Select the old `WeaponDataSO` asset, for example `Assets/Test/ActionGraph/Test/WeaponSystemOld/Weapon Data.asset`.
2. Run `Tools > Action Graph > Create Sword Combo Graph From Selected Weapon Data`.
   You can also right-click the selected asset and use `Assets > Action Graph > Create Sword Combo Graph From Selected Weapon Data`.
3. Add `ComboWeapon` to a character weapon object.
4. Assign the generated `ActionGraphAsset`, an `Animator`, and a `Hit Origin`.
5. For player input, leave `Register Input` enabled and set `Attack Input Key` to `Attack`. For AI/enemies, disable `Register Input` and call `ComboWeapon.Attack()` or `ComboWeapon.SetAttackInput(true)` from the AI.

The generated graph is now compact:

`Sequence`

- setup weapon models once
- run a `Loop` up to the combo count
- inside the loop, begin the current attack, play `Attack{index}`, and run one reusable attack body
- each body node picks the correct value from its data array using the current attack index
- advance to the next attack only when combo input is accepted
- reset the combo when the loop stops

The graph now owns the combo weapon values directly: sword setup, hitbox windows, damage amounts, slash event name, movement, knockback, and combo timing are all editable inside the generated graph.

