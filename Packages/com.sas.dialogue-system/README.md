# SAS Dialogue System

An Ink-powered Unity dialogue runtime. Ink remains the narrative source of truth; ordinary Ink tags are parsed into stable runtime metadata for speakers, listeners, portraits, animation, audio, layout, localization, and project-defined fields.

## Requirements

- Unity 2022.3 or newer
- [Ink Unity Integration](https://github.com/inkle/ink-unity-integration) 1.2.1 or newer
- SAS Core 1.0.0 or newer
- Unity Input System, Localization, and uGUI

Ink Unity Integration and SAS Core are Git packages in the SAS projects. Install those direct dependencies in the consuming project's `Packages/manifest.json` before installing this package from Git.

## Quick start

1. Add the package to the project.
2. In Package Manager, import **Basic Dialogue** from the Samples tab.
3. Open `Assets/Samples/SAS Dialogue System/0.1.0/Basic Dialogue/Basic Dialogue.unity`.
4. Enter Play Mode.
5. Press Space to reveal or advance text. Use the mouse or UI navigation to choose a response.

The sample uses direct component discovery and does not require a project-specific context binder.

## Runtime setup

1. Add a `DialogueHandler` and its presenter components to a Canvas, or start from the sample prefab.
2. Create a **Dialogue > Metadata Profile** asset and assign it to the handler.
3. Add `DialogueTrigger` to a scene object, assign compiled Ink JSON, and optionally assign a per-story metadata profile.
4. Call `DialogueTrigger.ShowDialogue()` from proximity, interaction, quest, or other game-owned code.
5. Subscribe to `IDialogueHandler` events or use `DialogueEventListener` for game reactions.

`DialogueTrigger` supports SAS Core injection, an explicit handler reference, and scene lookup as a final fallback.

## Metadata contract

| Runtime value | Ink tag |
| --- | --- |
| Line ID | `id` |
| Localization key | `locale` |
| Layout animation | `layout` |
| Audio profile | `audio` |
| Active speaker | `speaker` |
| Speaker name | `speaker_name` |
| Speaker portrait | `portrait` |
| Speaker animation | `animation` |
| Primary listener | `listener` |
| Listener name | `listener_name` |
| Listener portrait | `listener_portrait` |
| Listener animation | `listener_animation` |

Additional roles use `participant.<role>`, with optional `.name`, `.portrait`, and `.animation` fields. Unmapped `key:value` tags remain available through `DialogueLineContext.TryGetTagValue`. A `DialogueMetadataProfile` can map project-owned names such as `actor`, `face`, or `loc_key` onto the same runtime semantics.

## Customized Inky workflow

The customized Inky editor adds a metadata inspector but still writes standard Ink tags and uses the official compiler. It is an authoring companion, not a runtime dependency.

For regular dialogue, place the contiguous metadata block immediately above the line:

```ink
# id:guide.welcome
# speaker:guide
# speaker_name:Guide
# portrait:guide_happy
Welcome to the sample.
```

For choices, keep metadata inside the visible choice text so Ink exposes it through `Choice.tags` before selection:

```ink
* [Ask a question # id:choice.ask # analytics_event:sample.ask] -> answer
```

The optional `story.metadata.json` sidecar configures fields, labels, contexts, and suggestions in customized Inky. Unity runtime behavior is controlled by `DialogueMetadataProfile`, so the tag names in both tools should match.

## Package boundary

The package owns reusable session logic, metadata parsing, UI/presenter components, trigger/listener components, and tests. Proximity detection, player state changes, device-specific story variables, quest behavior, and game character animation adapters belong to the consuming game.