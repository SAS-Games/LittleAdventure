# Streaming volume architecture

The runtime makes load, activation, unload, and portal decisions through
`IStreamingVolume`. The existing axis-aligned box is `BoxStreamingVolume`.
Irregular side-scroller sections use `PolygonStreamingVolume`, a concave-capable
world-space XY outline extruded between Back Z and Front Z. Irregular open-world
and building sections use `XZPolygonStreamingVolume`, a concave-capable XZ
footprint extruded between Bottom Y and Top Y.

## Contracts

- `BroadphaseBounds` is a conservative world-space AABB enclosing the entire
  volume. Grid/quadtree strategies use it to find candidates. An overlap of these
  envelopes is never sufficient to select, activate, or retain a region.
- `Contains(point)` describes exact point membership, including the boundary.
- `TryIntersects(other, out result)` returns `true` when the implementation knows
  how to test that shape pair exactly; `result` is the intersection answer.
  Return `false` only for an unsupported pair. Touching counts as intersection.
- `StreamingVolumeIntersection.Intersects` rejects disjoint envelopes, then
  asks both implementations in turn. Unsupported overlapping pairs throw
  `NotSupportedException`, rather than silently falling back to AABB overlap.
  Implementations must agree on symmetric results and must not recursively call
  the dispatcher with the same two volumes.

Implement `IStreamingVolume` for runtime query geometry. Derive a `[Serializable]`
class from `StreamingVolume` for region/portal geometry stored by Unity's
`SerializeReference`. New shapes implement their own intersection with boxes and
any other supported shape types; the streaming controller does not change.
Do not depend on a collider in an unloaded content scene. Bake persistent shape
data or otherwise make the geometry available before content loads.

## Polygon regions

Choose **Polygon Prism** from a region's **Region Shape** field. The cached bounds
remain the content placement anchor and fallback box; switching shape creates a
rectangle from those bounds as the initial polygon. Expand **Shape Data** to edit
the XY vertex list and depth range. With **Edit Shape In Scene View** enabled,
drag the yellow point handles to shape the outline.

Vertices may run clockwise or counter-clockwise, and concave outlines are supported.
The outline must contain at least three finite points, have non-zero area, and must
not cross itself. Boundary contact counts as intersection. Polygon/box and
polygon/polygon queries are exact in XY and also require their Z ranges to overlap.

## Provider and region extension points

Implement `IStreamingVolumeProvider.TryGetVolumes(out StreamingVolumeSnapshot)`
and register it with `controller.SetStreamingVolumeProvider(provider)`. Return one
atomic activation/load/unload sample, with activation contained by load and load
contained by unload. Geometry must stay stable throughout that streaming tick.

Use `region.SetVolume(serializableVolume)` to assign world-space region geometry.
The serialized field is a managed reference. `region.Volume` is the effective
geometry; `region.BroadphaseBounds` is the index envelope. Static indexes must be
reinitialized after changing region geometry at runtime, just as they previously
needed rebuilding after changing cached boxes.

Portals optionally expose `WorldVolume` for world-space custom geometry. Their
legacy local box is converted by `region.GetPortalVolume(index)`. Call
`RebuildPortalWorldBounds()` after changing legacy portal data or region placement.

## Existing assets and compatibility

Existing serialized `cachedBounds` and portal `LocalBounds` remain intact. A null
custom volume uses the cached box, with no scene/prefab migration. Setting a custom
volume to null returns to that legacy box. `Region.Origin` retains the existing
placement anchor from `cachedBounds.center`; assigning new geometry does not move
prefab instances or legacy portals to the geometry's envelope center.

All observer providers implement `IStreamingVolumeProvider` directly. The old
bounds-provider interfaces, bounds snapshot, registration method, and compatibility
adapter have been removed. Default, camera, and adaptive providers currently
produce `BoxStreamingVolume` (AABB) samples. Their existing component names, script
GUIDs, and serialized settings are preserved. The camera provider still encloses
its rotated box in an AABB; exact oriented-box/frustum geometry is a future shape.

The built-in providers reuse their three box objects. Returned snapshots are
valid for the current tick, not historical immutable copies. Adaptive snapshots
retain observer position, velocity, normalized zoom, revision, readiness, and
the existing prediction/hysteresis behavior. `ResetPrediction` remains an
adaptive-provider operation, and `IStreamingZoomSource` is a separate zoom input
contract.

Selection and unload base classes retain box-query convenience overloads for
callers. Custom subclasses must override the new `IStreamingVolume` signature.
`QuadtreeNode.Query` likewise now accepts a volume.

The existing box Apply/Refresh tools and Phase 1 WorldAuthoring grid/profile/manifest
schemas remain box-based. Apply/Refresh are disabled for polygon regions. Polygon
outlines have their own Scene-view handles, validation, and exact gizmos.

## Horizontal polygon regions and building floors

Choose **Horizontal Polygon Prism (XZ + Y Height)** from a region's
**Region Shape** field. Edit the XZ footprint in Scene view, then set **Bottom Y**
and **Top Y** to the vertical range occupied by that region. Separate floors
should use separate regions and adjacent height ranges. The observer's
activate/load/unload boxes must have a small enough Y size to avoid intersecting
unwanted floors; touching a floor boundary intentionally counts as intersection
so adjacent floors can overlap briefly during stairs, lifts, or vertical motion.

Use **Add Vertex** and **Remove Last** in Shape Data for list editing. In Scene
view, drag a vertex to move it, click a green edge midpoint to insert a vertex,
or hold Ctrl (Cmd on macOS) and click a red vertex to remove it. A valid polygon
always retains at least three vertices.

Use `GridRegionSelection` or `QuadtreeRegionSelection` for XZ open worlds.
`GridRegionSelection2D` indexes XY and remains intended for side-scrollers.

`Assets/Tests/Editor/StreamingVolumeTests.cs` covers concave polygon containment,
polygon/box and polygon/polygon intersection, contact, depth separation,
serialization, invalid outlines, reverse dispatch, selector/index paths,
activation, unload, and portal protection. Native provider sampling, camera
rotation, adaptive metadata/prediction, and readiness recovery are covered by
`Assets/Tests/Editor/AdaptiveStreamingBoundsProviderTests.cs`.
