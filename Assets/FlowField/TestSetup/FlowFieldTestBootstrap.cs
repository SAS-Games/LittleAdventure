using UnityEngine;
using Unity.Mathematics;

public class FlowFieldTestBootstrap : MonoBehaviour
{
    public static FlowFieldSampler Sampler;

    FlowField flowField;

    [Header("Grid Settings")]
    public int width = 32;
    public int height = 32;
    public float cellSize = 1f;
    public Vector2 origin = Vector2.zero;

    [Header("Target")]
    public Transform target;

    void Start()
    {
        // Create grid
        FlowFieldGrid grid = new FlowFieldGrid(
            width,
            height,
            cellSize,
            origin
        );

        // Optional: fill cost = 1
        for (int i = 0; i < grid.CellCount; i++)
            grid.Cost[i] = 1;

        // Create flow field
        flowField = new FlowField(grid);

        // Build toward target
        int2 targetCell = FlowFieldGridUtility.WorldToCell(
            target.position,
            grid.Origin,
            grid.CellSize
        );

        flowField.Build(targetCell);

        // Create sampler
        Sampler = new FlowFieldSampler(grid);
    }

    void OnDestroy()
    {
        flowField?.Dispose();
    }
}