# Streaming Test World

This self-contained test world contains one persistent scene and 25 contiguous
100 x 100 streaming levels arranged in a 5 x 5 grid around the world origin.
Each streaming level contains a ground plane and one `RegionBound` marker.

Open `StreamingTestPersistent.unity` or use
`Tools > Streaming > Test World > Open Persistent Scene`. The included editor
setup appends all 26 scenes to Build Settings without removing existing scenes.

To inspect the complete world outside Play Mode, open
`Tools > Streaming > Level Streaming Editor`, select the `Setup` page, and use
`Load All Scenes`. `Frame Complete World` focuses the Scene view on the full
grid. Use `Unload Streaming Scenes` when finished; modified scenes are offered
for saving before they are closed, and the persistent scene remains open.

In Play Mode, click the Game view and use WASD to fly. Use E to move up,
Q or C to move down, and Shift to boost speed. Hold the right mouse button to
look around. The mouse wheel changes camera field of view so you can verify that
zooming out expands the adaptive streaming bounds and loads more regions.
Use the Level Streaming Editor's Runtime page to inspect desired, loaded, active,
and unloaded regions. The ground grid is continuous when every level is loaded.
