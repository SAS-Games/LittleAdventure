# World Streaming Authoring Tool

This folder contains Phase 1 of the non-destructive world-streaming authoring
workflow. The large map remains the source of truth. No Phase 1 command moves,
copies, deletes, or saves source-scene objects.

## Existing runtime architecture

The current runtime remains authoritative:

- `RegionManager` owns serialized regions. A region can reference a Build
  Settings scene, an Addressable scene, or an Addressable prefab. It stores a
  cached world bound, stable region name, portals, and an unload strategy.
- `RegionStreamingController` queries an `IStreamingVolumeProvider`, asks the
  selected `RegionSelectionStrategySO` for intersecting regions, and schedules
  asynchronous loads and unloads.
- `SceneStreamingLoader` opens Build Settings scenes additively by scene path.
  Optional Addressables backends register prefab and scene loaders without
  changing the built-in scene path.
- `SharedStreamingRegistry` reference-counts physically shared assets.
- Activation is bounds-based. Unloading is delegated to each region's
  `UnloadStrategy`. Portals can mark neighboring regions as desired, but there
  is no generated neighbor manifest or generic neighbor preloading yet.
- The persistent scene owns `RegionManager`, `RegionStreamingController`, the
  bounds provider, and other always-loaded systems.

`WorldStreamingManifest` is a generated metadata schema for later phases. It
does not introduce a second runtime controller. Phase 4 will generate the
existing `RegionManager.Region` configuration from the same authoring data.

## Phase 1 architecture

```text
WorldAuthoring/
  Runtime/
    SAS.WorldStreaming.Runtime.asmdef
    WorldStreamingManifest.cs
    StreamingSceneRecord.cs
    SharedSceneRecord.cs
  Editor/
    SAS.WorldStreaming.Editor.asmdef
    Model/
      WorldStreamingProfile.cs
      StreamingRegionDefinition.cs
      SharedSceneDefinition.cs
      WorldStreamingAuthoringDatabase.cs
      WorldObjectAuthoringRecord.cs
      WorldObjectReference.cs
    Services/
      IWorldRegionProvider.cs
      UniformGridRegionProvider.cs
    SceneView/
      WorldStreamingSceneViewDrawer.cs
    Validation/
      WorldStreamingProfileValidator.cs
    WorldStreamingAuthoringWindow.cs
  Tests/Editor/
    SAS.WorldStreaming.Editor.Tests.asmdef
    UniformGridRegionProviderTests.cs
```

The profile stores source/output configuration, grid or manual regions,
persistent classification defaults, shared-scene definitions, and the defaults
needed by Phase 2. Region IDs are GUIDs. Regenerating a grid preserves the ID of
every existing coordinate, even when the grid origin changes.

The authoring database already reserves stable `GlobalObjectId`, original
hierarchy/transform, assignment, calculated-bounds, and generated-output fields.
Phase 2 will populate these records; generated scene content will never become
the source of truth.

## Data flow

```text
Large source scene
  -> WorldStreamingProfile
  -> grid/manual region preview
  -> Phase 2 scan and assignment database
  -> Phase 3 validation and dry run
  -> Phase 4 non-destructive generated scene copies
  -> existing RegionManager configuration + runtime manifest
```

## Designer workflow for Phase 1

1. Open `Tools > SAS > World Streaming Authoring`.
2. Create or select a `WorldStreamingProfile`.
3. Assign the large source authoring scene and an output folder under `Assets`.
4. Configure the uniform grid:
   - World Origin is the minimum grid corner.
   - Cell Size controls X/Z cell dimensions.
   - In XZ mode, Minimum/Maximum World Y provide one shared height range.
   - Load Bounds Padding expands preview load bounds without changing ownership
     bounds.
5. Click **Generate Grid Preview**.
6. Open the source scene and enable **Edit Layout In Scene View** to move the
   grid origin.
7. Switch to Manual Regions to add, select, resize, duplicate, delete, and frame
   hand-authored bounds.
8. Create the authoring database before Phase 2 analysis.

## Known production risks

- Cross-scene serialized and UnityEvent references can break after partitioning.
  Phase 3 must block generation on unsafe references by default.
- Prefab instance roots must remain intact. Phase 2 will default hierarchy
  grouping to `PrefabInstanceRoot`.
- A Terrain is treated as one object in the initial implementation. Automatic
  `TerrainData` splitting is intentionally deferred.
- Lighting data, lightmaps, NavMesh data, occlusion data, static batching,
  reflection/light probes, global volumes, and Terrain neighbors are
  scene-bound workflows and require warnings or rebaking after generation.
- Generated copies require source `GlobalObjectId` markers so regeneration can
  replace only generated content and preserve unmarked designer content.

## Planned phases

1. Foundation: profile/database, manifest schema, grid/manual regions, window,
   Scene-view preview, and grid tests. **Implemented here.**
2. Analysis: source scanning, bounds providers, hierarchy grouping, persistent
   and streaming classification, cross-region reporting, manual overrides.
3. Validation: modular validators, cross-scene reference analysis, dry-run
   preview, blocking errors.
4. Generation: non-destructive copy strategy, persistent/streaming scenes,
   existing `RegionManager` metadata, manifest, progress/cancellation cleanup.
5. Regeneration: replace generated objects only, stale-object cleanup, conflict
   reporting, preserve unmarked objects.
6. Reference resolvers and production polish: reports, complexity estimates,
   filters, build integration, and the remaining EditMode tests.
