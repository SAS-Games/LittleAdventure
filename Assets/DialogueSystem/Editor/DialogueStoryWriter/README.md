# Dialogue Story Writer

Unity editor tool for writing Ink dialogue from inside Unity.

Open it from:

`Tools > Dialogue > Story Writer`

You can create a draft from:

`Assets > Create > Dialogue > Story Draft`

You can also right-click an `.ink` file and choose:

`Import Ink Into Dialogue Story Writer`

## Edit Mode Player

The **Edit Mode Player** lets you test the generated Ink directly inside the Story Writer window without entering Unity Play Mode.

How to use it:

1. Open `Tools > Dialogue > Story Writer`.
2. Assign or create a `DialogueStoryDraft`.
3. Write your dialogue, choices, tags, sections, and diverts.
4. Scroll to **Edit Mode Player**.
5. Press **Play**.
6. Read the generated dialogue output in the player history.
7. Press **Continue** when the story can continue.
8. Click any choice button to test branching.
9. Press **Restart** to replay from the beginning.
10. Press **Stop** to clear the preview.

What it does:

- Builds the draft into Ink in memory.
- Compiles that Ink in edit mode.
- Shows dialogue text.
- Shows tags for each line.
- Shows choices as buttons.
- Follows knots, stitches, diverts, gathers, conditions, and variables.
- Loads included files such as `LA_common.ink`.

What it does not do:

- It does not use the scene `DialogueHandler`.
- It does not play your real speaker UI, portraits, layout animation, or audio.
- It does not run real gameplay effects.

External methods are bound to dummy preview handlers, so calls like this will not break editor preview:

```ink
~ GrantItem("shrine_spark", 1)
```

For real gameplay, those methods still need to be registered in your runtime `InkExternalMethodRegistry`.

## Draft

A `DialogueStoryDraft` is the editable Unity asset that stores the designer-friendly version of the story.

It generates an `.ink` file when you press:

- **Save Ink**
- **Save + Compile**

The draft is the source designers should edit when possible. The `.ink` output is generated from it.

## Output Settings

### Ink File Name

Name of the generated `.ink` file.

Example:

```text
LA_Introduction
```

Generates:

```text
LA_Introduction.ink
```

### Output Folder

Folder where the generated `.ink` file is saved.

Default folder:

```text
Assets/DialogueSystem/DialougeAssets
```

### Include Common Ink

Adds `INCLUDE` lines at the top of the Ink file.

Use this for shared variables and constants, for example:

```ink
INCLUDE LA_common.ink
```

Your `LA_common.ink` currently contains globals such as:

```ink
VAR isCoop = ""
VAR dashButton = "R1"
VAR attackButton = "X"
VAR Player1_name = "Player1"
```

### Include Files

Use multiple includes when a story needs more than one shared Ink file.

Example output:

```ink
INCLUDE LA_common.ink
INCLUDE LA_items.ink
```

### Global Tags

Tags written before the story starts.

Use these for metadata about the whole story.

Example:

```ink
# story:introduction
# chapter:tutorial
```

### Write Start Divert

Writes the first divert before the first section.

Example:

```ink
-> introduction
```

This tells Ink where the story should begin.

### Start Target

The knot or stitch to start from.

Usually this is the first knot, for example:

```text
introduction
```

### Compile On Save

When enabled, **Save Ink** also asks Ink Unity Integration to compile the generated Ink into JSON.

### Auto End Line-Only Sections

If a section has only lines and no terminal entry, the generator adds:

```ink
-> END
```

This prevents Ink warnings like “loose end exists where the flow runs out”.

## Sections

Sections are Ink navigation blocks.

The editor supports three section types:

- **Knot**
- **Stitch**
- **Function**

### Knot

A knot is a major story section.

Editor section type:

```text
Knot
```

Generated Ink:

```ink
=== introduction ===
```

Use knots for major scenes, branches, or reusable story destinations.

### Stitch

A stitch is a smaller section inside the current knot.

Important rule:

```text
A stitch belongs to the nearest knot above it.
```

That means this is valid:

```ink
=== shrine_hub ===
= inspect_relic
```

This is invalid because there is no parent knot:

```ink
= orphan_stitch
```

This is also invalid as a parent relationship because functions are not knots:

```ink
=== function AddPower(value) ===
= orphan_stitch
```

Editor section type:

```text
Stitch
```

Generated Ink:

```ink
= inspect_relic
```

Use stitches for small branches that belong to the current knot.

Editor safeguards:

- The sidebar indents stitches under their parent knot.
- The `+ Stitch` button is enabled only when the selected section has a valid parent knot.
- New stitches are inserted inside the selected knot block, before the next knot or function.
- Save and Edit Mode Player are blocked if a stitch has no parent knot.

Example:

```ink
=== shrine_hub ===
* [Inspect the relic] -> inspect_relic

= inspect_relic
The relic is warm.
-> shrine_hub
```

In this example, `inspect_relic` is a stitch under `shrine_hub`.

### Function

A function is an Ink section that can return a value.

Editor section type:

```text
Function
```

Function Signature field:

```text
AddShrinePower(current, amount)
```

Generated Ink:

```ink
=== function AddShrinePower(current, amount) ===
~ return current + amount
```

Call it from Raw Ink:

```ink
~ shrine_power = AddShrinePower(shrine_power, 2)
```

Function sections are not shown in the divert target dropdown, because they are called like functions instead of visited like story knots.

Function sections also do not receive an automatic `-> END`.

## Entry Types

Each section contains entries. Entries become Ink lines, choices, diverts, tags, or raw Ink.

## Line

Writes normal dialogue text.

Editor fields:

- Text
- Speaker tag
- Localization tag
- Layout tag
- Audio tag
- Custom tags

Generated Ink:

```ink
Welcome back. #speaker:npc #speaker_name:KAIROS #animation:Talk #locale:welcome_back #layout:right #audio:default
```

Use this for normal dialogue.

## Tag

Writes a standalone Ink tag line.

Generated Ink:

```ink
# CLEAR
```

Use standalone tags for non-dialogue instructions, such as clearing an image, changing a scene state, or sending metadata to tag processors.

## Choice

Writes an Ink choice.

Basic generated Ink:

```ink
* [Ask about the shard] -> about_shard
```

Choices can have:

- Target
- Target Is Tunnel
- Choice Body
- Choice depth
- Sticky choice
- Fallback choice
- Suppress choice text
- Choice condition
- Localization tag
- Custom tags

### Target

Where the choice goes.

Generated Ink:

```ink
* [Ask about the shard] -> about_shard
```

### Target Is Tunnel

When enabled, the choice target is written as a tunnel call.

Generated Ink:

```ink
* [Run through the alarm route] -> trigger_alarm ->
```

Use this when the target knot should behave like a subroutine and return to the next line after it reaches `->->`.

### Choice Body

Choice Body lets a choice contain child entries.

Use it when selecting a choice should run dialogue, tags, raw Ink, variable assignments, diverts, tunnels, or nested choices before leaving the branch.

Editor example:

- Choice Text: `Drop down silently behind the left guard`
- Choice Body:
- Raw Ink: `~ player_stealth += 2`
- Line: `You land like a cat. The guard continues walking.`
- Divert: `guard_interaction`

Generated Ink:

```ink
+ [Drop down silently behind the left guard]
    ~ player_stealth += 2
    You land like a cat. The guard continues walking.
    -> guard_interaction
```

If a choice has body entries and a Target, the Target is written after the body:

```ink
+ [Just run for the door]
    -> trigger_alarm ->
    -> vault_door_entry
```

For simple choices that immediately go somewhere, leave Choice Body empty and use Target.

### Choice Depth

Controls how many `*` markers are used.

Depth 1:

```ink
* [Open the door] -> open_door
```

Depth 2:

```ink
** [Nested choice] -> nested_choice
```

Use this for nested choices. When you add a Choice inside another choice's body, the editor starts it at the next choice depth automatically.

### Sticky Choice

Uses `+` instead of `*`.

Generated Ink:

```ink
+ [Repeat the warning] -> warning_repeat
```

Normal `*` choices are usually consumed after selection. Sticky `+` choices can remain available depending on the flow.

### Fallback Choice

Writes a choice with no visible text.

Generated Ink:

```ink
* -> fallback_no_choice
```

Use this as a fallback branch when no other visible choice is valid.

### Suppress Choice Text

Writes choice text inside square brackets.

Generated Ink:

```ink
* [What is Localization? #locale:what_is_localization] -> what_is_localization
```

This is useful because Ink shows the text as a choice but does not print it again as normal story output after the choice is selected.

For designer workflow, this is the recommended choice style.

### Choice Localization

Choice localization tags are written inside the square brackets when choice text is suppressed.

Recommended:

```ink
* [Setting Up Localization in Unity #locale:setting_up_localization] -> setup_localization
```

In the editor, the designer should only fill:

- Choice Text: `Setting Up Localization in Unity`
- Local Key: `setting_up_localization`
- Target: `setup_localization`
- Suppress Choice Text: enabled

The designer should not manually type the `#locale:` tag into the choice text.

### Choice Condition

Controls whether the choice is visible.

Generated Ink:

```ink
* {has_relic_key} [Use the relic key #locale:use_relic_key] -> key_gate
```

This choice only appears when `has_relic_key` is true.

Use this for locked choices, quest-state choices, inventory checks, co-op checks, and tutorial gates.

## Gather

Writes an Ink gather using `-`.

Generated Ink:

```ink
- The branches return to one path.
```

Use gathers to bring choice branches back together.

Gather depth controls how many `-` markers are written:

```ink
-
--
---
```

## Divert

Moves the story to another knot or stitch.

Generated Ink:

```ink
-> shrine_hub
```

Use diverts for navigation.

Special terminal diverts:

```ink
-> END
-> DONE
```

## Tunnel Divert

Calls another knot as a tunnel, then returns to the next line when that knot reaches `->->`.

Generated Ink:

```ink
-> trigger_alarm ->
```

Example:

```ink
+ [Just run for the door]
    -> trigger_alarm ->
    -> vault_door_entry
```

Use this for reusable story beats such as alarms, rewards, checks, or short cutscenes that should return to the caller.

## Tunnel Return

Returns from a tunnel call.

Generated Ink:

```ink
->->
```

Use this at the end of a knot that is called with `-> target ->`.

## Conditional Divert

Branches to one target if a condition is true, and optionally another target otherwise.

Editor fields:

- Condition
- If True
- Else

Generated Ink:

```ink
{ isCoop:
    -> coop_intro
- else:
    -> solo_intro
}
```

Use this for story-level branching.

## Conditional Tunnel Divert

Conditionally calls a tunnel target.

Generated Ink:

```ink
{ player_stealth < 4:
    -> trigger_alarm ->
}
```

Use this when a condition should run a reusable tunnel and then come back to the current flow.

## Conditional Line

Writes a line only when a condition is true.

Generated Ink:

```ink
{ inspected_relic:
    KAIROS nods toward the relic. #locale:inspected_relic_line
}
```

Use this for small conditional flavor lines.

## Raw Ink

Writes Ink exactly as typed.

Use Raw Ink for advanced Ink that the editor does not model directly.

Examples:

### Variables

```ink
VAR shrine_power = 0
VAR has_relic_key = false
```

### Variable Assignment

```ink
~ has_relic_key = true
~ shrine_power = shrine_power + 1
```

### Function Call

```ink
~ shrine_power = AddShrinePower(shrine_power, 2)
```

### External Method Call

```ink
EXTERNAL GrantItem(id, count)
~ GrantItem("shrine_spark", 1)
```

External methods must be registered in Unity runtime code through `InkExternalMethodRegistry`.

### Multiline Raw Ink

```ink
{ shrine_power >= 3:
    The shrine burns bright across several raw Ink lines.
    Its light remembers every offering you made.
- else:
    The shrine glows softly across several raw Ink lines.
    It is waiting for more power.
}
```

Use this for complex Ink blocks, temporary variables, advanced conditions, functions, tunnels, lists, or anything the editor UI does not expose yet.

## End

Writes:

```ink
-> END
```

Use this when the story should fully end.

## Done

Writes:

```ink
-> DONE
```

Use this when the current flow is done and should return to the caller in Ink flow.

## Tags

Tags are metadata attached to lines or choices.

The runtime parses every line or choice exactly once into an immutable `DialogueLineContext`. The canonical fields are:

- `id`
- `locale`
- `speaker`, `speaker_name`, `portrait`, and `animation`
- `listener`, `listener_name`, `listener_portrait`, and `listener_animation`
- `layout`
- `audio`

Every parsed tag is also kept in the case-insensitive, multi-value `DialogueLineContext.Tags` bag. This includes arbitrary custom tags and no-value tags such as:

```ink
#CLEAR
```

Runtime code can read them with:

```csharp
if (lineContext.HasTag("CLEAR"))
{
    // Clear a portrait, image, or other presentation state.
}

if (lineContext.TryGetTagValue("quest", out var questId))
{
    // React to #quest:some_id.
}
```

Canonical metadata is parsed as a complete set, so tag order does not matter. Duplicate scalar fields produce a warning and the final value wins. Invalid identifiers and incomplete participant definitions produce errors; `DialogueHandler` rejects lines with metadata errors by default.

## Canonical Inky Metadata

The runtime accepts the fields written by the customized Inky metadata inspector:

```ink
# id:dialogue.leave.01
# locale:dialogue.leave.01
# speaker:alice
# speaker_name:Alice
# portrait:happy
# animation:TalkHappy
# listener:bob
# listener_portrait:concerned
# listener_animation:Listen
# audio:alice_leave_01
Alice: We should leave.
```

`speaker` is the active voice. `listener` is an optional second participant, so no separate dialogue mode is required:

- Narration has no participant tags.
- Monologue uses only `speaker` and optional portrait/animation fields.
- Dialogue uses `speaker` plus an optional `listener`.

The speaker presenter activates every participant for which it has a configured `SpeakerView`. Portrait and animation values remain stable lookup keys; they are not asset paths.

Runtime code can query roles without knowing how many participants a line has:

```csharp
string activeSpeaker = lineContext.CurrentSpeakerId;
string listener = lineContext.ListenerId;

if (lineContext.TryGetParticipant("listener", out var participant))
{
    string characterId = participant.CharacterId;
    string portraitKey = participant.PortraitKey;
    string animationKey = participant.AnimationKey;
}
```

Generic roles use the explicit `participant.<role>` namespace so they cannot collide with ordinary custom tags:

```ink
# participant.interviewer:maya
# participant.interviewer.name:Detective Maya
# participant.interviewer.portrait:focused
# participant.interviewer.animation:Question
What did you see?
```

Only `participant.interviewer:maya` is required to create the role. Name, portrait, and animation are optional. Add an `interviewer` participant slot to `SpeakerPresenter` when that role needs its own `SpeakerView`.

Choice tags use the same parser because `ChoiceHandler` builds a `DialogueLineContext` from `Choice.tags`. This makes choice `id`, `locale`, analytics, and other custom metadata available before selection.

## Runtime flow

`DialogueSession` owns all Ink progression and exposes deterministic states: `Starting`, `PresentingLine`, `WaitingForAdvance`, `PresentingChoices`, `Exiting`, and `Faulted`. Unity components adapt input and presentation around it:

- Send player input to `DialogueHandler.RequestAdvance()`. It skips the active typewriter reveal or continues a fully presented line as appropriate.
- A line presenter calls `CompleteLinePresentation(lineContext)` exactly once after it has revealed or skipped the line.
- Choice selection is accepted only while the session is `PresentingChoices`, preventing double selection.
- Presentation components never call `Story.Continue()` directly.
- Choice-only knots transition directly to `PresentingChoices`; they do not create a fake empty line.

## Speaker Tag

Use the canonical fields:

```ink
# speaker:npc
# speaker_name:KAIROS
# portrait:default
# animation:Talk
```

The runtime intentionally has no legacy compound speaker parser. `speaker` is the character ID, `speaker_name` is an optional display override, and portrait/animation are optional presentation lookup keys.

## Localization Tag

Generated Ink:

```ink
#locale:introduction_welcome
```

Use this to replace displayed text with a localized string.

For choices, use the choice localization field instead of manually typing tags.

## Layout Tag

Generated Ink:

```ink
#layout:right
```

Use this to control dialogue layout animation or speaker position.

## Audio Tag

Generated Ink:

```ink
#audio:default
```

Use this to change typewriter or dialogue audio.

## Custom Tags

Generated Ink:

```ink
#quest:dialogue_writer_showcase
#emotion:teacher
```

Use custom tags for extra metadata consumed by gameplay systems. They do not need to be part of the canonical participant schema.

If a custom tag has no value, it writes only the key:

```ink
# CLEAR
```

No-value tags are valid runtime metadata. They are stored with an empty value and can be checked with `lineContext.HasTag("CLEAR")`.

## Importing Existing Ink

Right-click an `.ink` file and choose:

`Import Ink Into Dialogue Story Writer`

The importer handles:

- Includes
- Global tags
- Knots
- Stitches
- Lines
- Tags
- Choices
- Sticky choices
- Choice conditions
- Fallback choices
- Gathers
- Diverts
- `END`
- `DONE`
- Conditional diverts
- Tunnel diverts
- Tunnel returns
- Conditional tunnel diverts
- Simple conditional lines
- Raw Ink fallback for advanced Ink

Advanced Ink that cannot be represented cleanly becomes a Raw Ink entry so the content is preserved.

## Recommended Designer Workflow

1. Create or open a `DialogueStoryDraft`.
2. Add a knot for the main scene.
3. Add line entries for dialogue.
4. Add speaker and localization fields through the tag UI.
5. Add choices using the Choice entry.
6. For choices, enable **Suppress Choice Text** and fill the **Local Key** field.
7. Use conditions for locked or state-based choices.
8. Use diverts to move between knots and stitches.
9. Use stitches for small branches inside a main knot.
10. Use the Edit Mode Player to test flow.
11. Press **Save + Compile** when ready to generate Ink JSON.

## Feature Showcase Story

A sample story that exercises the editor features is here:

```text
Assets/DialogueSystem/DialougeAssets/DialogueStoryWriterFeatureShowcase.ink
```

It includes:

- Includes
- Global tags
- Knots
- Stitches
- Lines
- Standalone tags
- Speaker/local/layout/audio/custom tags
- Choices
- Choice localization
- Choice conditions
- Sticky choices
- Fallback choices
- Nested choices
- Gathers
- Diverts
- Conditional diverts
- Conditional lines
- Raw Ink
- Multiline Raw Ink
- Variables
- Variable assignment
- Internal Ink functions
- External Unity method calls
- `END`
- `DONE`
