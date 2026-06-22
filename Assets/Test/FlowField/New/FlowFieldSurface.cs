using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FlowFieldSurface : MonoBehaviour
{
    public enum FlowFieldBakeSourceMode
    {
        UseColliders,
        UseRenderers
    }

    [SerializeField] private float agentRadius = 0.5f;

    [Header("Bake Source")] 
    [SerializeField] private Transform bakeRoot;
    [SerializeField] private FlowFieldBakeSourceMode sourceMode = FlowFieldBakeSourceMode.UseColliders;
    [SerializeField] private LayerMask bakeLayers = ~0;

    [Header("Grid")] 
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float maxSlope = 45f;

    [Header("Output")] 
    [SerializeField] private FlowFieldAsset asset;
    [SerializeField] private FlowFieldAreaCostConfig areaCostConfig;
    
    [ContextMenu("Bake")]
    public void Bake()
    {
        if (asset == null) { Debug.LogError("FlowFieldAsset not assigned."); return; }
        if (bakeRoot == null) { Debug.LogError("Bake Root not assigned."); return; }

        Bounds bounds = CalculateBounds();
        int width = Mathf.CeilToInt(bounds.size.x / cellSize);
        int height = Mathf.CeilToInt(bounds.size.z / cellSize);
        int count = width * height;

        asset.width = width;
        asset.height = height;
        asset.cellSize = cellSize;
        asset.origin = new Vector2(bounds.min.x, bounds.min.z);
        asset.costs = new byte[count];
        asset.terrainHeights = new float[count];
        asset.terrainNormals = new Vector3[count];

        BakeCells(width, height);

#if UNITY_EDITOR
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
#endif
        Debug.Log($"FlowField baked: {width} x {height} ({count} cells)");
    }

    private Bounds CalculateBounds()
    {
        return sourceMode == FlowFieldBakeSourceMode.UseColliders ? CalculateColliderBounds() : CalculateRendererBounds();
    }

    private Bounds CalculateColliderBounds()
    {
        Collider[] colliders = bakeRoot.GetComponentsInChildren<Collider>();
        bool initialized = false;
        Bounds bounds = default;

        foreach (Collider collider in colliders)
        {
            if (((1 << collider.gameObject.layer) & bakeLayers.value) == 0) continue;
            if (!initialized) { bounds = collider.bounds; initialized = true; }
            else bounds.Encapsulate(collider.bounds);
        }
        return bounds;
    }

    private Bounds CalculateRendererBounds()
    {
        Renderer[] renderers = bakeRoot.GetComponentsInChildren<Renderer>();
        bool initialized = false;
        Bounds bounds = default;

        foreach (Renderer renderer in renderers)
        {
            if (((1 << renderer.gameObject.layer) & bakeLayers.value) == 0) continue;
            if (!initialized) { bounds = renderer.bounds; initialized = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }

    private void BakeCells(int width, int height)
    {
        Vector2 origin = asset.origin;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = x + y * width;
                Vector3 worldPos = new Vector3(origin.x + (x + 0.5f) * cellSize, 0f, origin.y + (y + 0.5f) * cellSize);
                BakeCell(index, worldPos);
            }
        }
    }

    private void BakeCell(int index, Vector3 worldPos)
    {
        // 1. Raycast to find the ground
        Vector3 rayStart = worldPos + Vector3.up * 1000f;
        if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit groundHit, 2000f, bakeLayers))
        {
            asset.costs[index] = byte.MaxValue;
            asset.terrainHeights[index] = 0f;
            asset.terrainNormals[index] = Vector3.up;
            return;
        }

        asset.terrainHeights[index] = groundHit.point.y;
        asset.terrainNormals[index] = groundHit.normal;

        // 2. Slope check
        float slope = Vector3.Angle(Vector3.up, groundHit.normal);
        if (slope > maxSlope)
        {
            asset.costs[index] = byte.MaxValue;
            return;
        }

        // 3. OverlapBox for Volume Obstacles & Area Costs
        Vector3 center = new Vector3(worldPos.x, groundHit.point.y + 1f, worldPos.z); // Lift center slightly above ground line
        Vector3 halfExtents = new Vector3(cellSize * 0.5f + agentRadius, 1f, cellSize * 0.5f + agentRadius);
        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, bakeLayers);

        byte bestCost = 3; // Default ground cost

        foreach (Collider collider in hits)
        {
            // Ignore the ground itself that we hit during the raycast step
            if (collider == groundHit.collider)
                continue;

            FlowFieldArea area = collider.GetComponentInParent<FlowFieldArea>();

            // CRITICAL FIX: If it is a solid physical collider but has no script attached, 
            // treat it as an unscripted, hard blocking obstacle (e.g. standard wall mesh/primitive)
            if (area == null)
            {
                // If it's a trigger collider, we ignore it unless it has a FlowFieldArea component
                if (collider.isTrigger) 
                    continue;

                asset.costs[index] = byte.MaxValue;
                return;
            }

            // Process explicit Area zones
            byte areaCost = areaCostConfig.GetCost(area.areaId);
            if (areaCost == byte.MaxValue)
            {
                asset.costs[index] = byte.MaxValue;
                return;
            }

            if (areaCost > bestCost)
            {
                bestCost = areaCost;
            }
        }

        asset.costs[index] = bestCost;
    }
}
