using Unity.Mathematics;
using UnityEngine;

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
        // Convert to grid space
        float2 local = (worldPosition.xz - grid.Origin) / grid.CellSize;

        int x = (int)math.floor(local.x);
        int y = (int)math.floor(local.y);

        float fx = local.x - x;
        float fy = local.y - y;

        // Clamp base cell
        if ((uint)x >= (uint)(grid.Width - 1) ||
            (uint)y >= (uint)(grid.Height - 1))
        {
            return float2.zero;
        }

        int i00 = x + y * grid.Width;
        int i10 = (x + 1) + y * grid.Width;
        int i01 = x + (y + 1) * grid.Width;
        int i11 = (x + 1) + (y + 1) * grid.Width;

        float2 d00 = grid.Flow[i00];
        float2 d10 = grid.Flow[i10];
        float2 d01 = grid.Flow[i01];
        float2 d11 = grid.Flow[i11];

        // Bilinear interpolation
        float2 dx0 = math.lerp(d00, d10, fx);
        float2 dx1 = math.lerp(d01, d11, fx);
        float2 dir = math.lerp(dx0, dx1, fy);

        // Normalize safely
        float lenSq = math.lengthsq(dir);
        return lenSq > 0.0001f ? dir / math.sqrt(lenSq) : float2.zero;
    }
}