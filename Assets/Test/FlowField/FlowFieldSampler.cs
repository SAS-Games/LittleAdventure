using Unity.Mathematics;

public sealed class FlowFieldSampler
{
    readonly FlowFieldGrid grid;

    public FlowFieldSampler(FlowFieldGrid grid)
    {
        this.grid = grid;
    }
    
    /// <summary>
    /// Sample smooth flow direction at world position
    /// </summary>
    public float2 SampleDirection(float3 worldPosition)
    {
        // 1. Convert world position to grid space
        float2 local = (worldPosition.xz - grid.Origin) / grid.CellSize;

        local.x = math.clamp(local.x, 0f, grid.Width - 1.001f);
        local.y = math.clamp(local.y, 0f, grid.Height - 1.001f);

        int x = (int)math.floor(local.x);
        int y = (int)math.floor(local.y);

        float fx = local.x - x;
        float fy = local.y - y;

        int i00 = x + y * grid.Width;
        int i10 = (x + 1) + y * grid.Width;
        int i01 = x + (y + 1) * grid.Width;
        int i11 = (x + 1) + (y + 1) * grid.Width;

        // Get directions for all 4 cells
        float2 d00 = grid.Flow[i00];
        float2 d10 = grid.Flow[i10];
        float2 d01 = grid.Flow[i01];
        float2 d11 = grid.Flow[i11];

        // 2. FIXED: Instead of canceling interpolation, check individual cells.
        // If a neighbor is an obstacle, substitute it with d00 so it doesn't break the blend.
        if (grid.Cost[i10] == byte.MaxValue) d10 = d00;
        if (grid.Cost[i01] == byte.MaxValue) d01 = d00;
        if (grid.Cost[i11] == byte.MaxValue) d11 = d00;

        // 3. Smooth interpolation now works perfectly near walls
        float2 dx0 = math.lerp(d00, d10, fx);
        float2 dx1 = math.lerp(d01, d11, fx);
        float2 dir = math.lerp(dx0, dx1, fy);

        float lenSq = math.lengthsq(dir);

        // 4. Fallback for vector cancellation
        if (lenSq < 0.0001f)
        {
            float2 safeDir = d00;
            return math.lengthsq(safeDir) > 0.0001f ? math.normalize(safeDir) : float2.zero;
        }
        return math.normalize(dir);
    }
}