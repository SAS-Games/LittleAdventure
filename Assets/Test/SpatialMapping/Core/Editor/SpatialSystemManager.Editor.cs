using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections.Generic;

public abstract partial class SpatialSystemManager
{
    [Header("Debug Visualization")]
    [SerializeField] private bool drawGrid = true;
    [SerializeField] private bool drawOnlyXZ = true;

    [SerializeField] private Color gridColor = new Color(0f, 1f, 0f, 0.25f);
    [SerializeField] private Color queryColor = new Color(1f, 0f, 0f, 0.25f);

    protected void DrawGridGizmos()
    {
        if (!drawGrid || database == null || !database.Grid.IsCreated)
            return;

        Gizmos.color = gridColor;

        var keys = database.Grid.GetKeyArray(Allocator.Temp);
        HashSet<int3> unique = new HashSet<int3>();

        foreach (var cell in keys)
        {
            if (!unique.Add(cell))
                continue;

            DrawCell(cell, cellSize);
        }

        keys.Dispose();
    }

    protected void DrawQueryGizmos(Bounds bounds)
    {
        if (!drawGrid)
            return;

        Gizmos.color = queryColor;

        int3 minCell = SpatialUtils.WorldToCell(bounds.min, cellSize);
        int3 maxCell = SpatialUtils.WorldToCell(bounds.max, cellSize);

        for (int x = minCell.x; x <= maxCell.x; x++)
            for (int y = minCell.y; y <= maxCell.y; y++)
                for (int z = minCell.z; z <= maxCell.z; z++)
                {
                    DrawCell(new int3(x, y, z), cellSize);
                }
    }

    private void DrawCell(int3 cell, int3 size)
    {
        Vector3 center = CellToWorldCenter(cell, size);
        Vector3 drawSize = new Vector3(size.x, size.y, size.z);

        if (drawOnlyXZ)
            drawSize.y = 0.1f;

        Gizmos.DrawWireCube(center, drawSize);
    }

    private static Vector3 CellToWorldCenter(int3 cell, int3 size)
    {
        return new Vector3(
            (cell.x + 0.5f) * size.x,
            (cell.y + 0.5f) * size.y,
            (cell.z + 0.5f) * size.z
        );
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (database != null)
            DrawGridGizmos();

        if (_lastQueryBounds.HasValue)
            DrawQueryGizmos(_lastQueryBounds.Value);
    }

    private Bounds? _lastQueryBounds;

    public void DebugQuery(Bounds bounds)
    {
        _lastQueryBounds = bounds;
    }
}