using SAS.Core;
using UnityEngine;
using Unity.Mathematics;

public sealed class FlowFieldManager : Singleton<FlowFieldManager>
{
    public FlowField ActiveFlowField
    {
        get;
        private set;
    }

    public FlowFieldSampler ActiveSampler
    {
        get;
        private set;
    }

    public int2 ActiveTargetCell
    {
        get;
        private set;
    }

    public Vector3 ActiveTargetWorldPosition
    {
        get;
        private set;
    }

    /// <summary>
    /// Builds a new flow field using a world-space target position.
    /// </summary>
    public void Rebuild(
        FlowFieldAsset asset,
        Vector3 targetWorldPosition)
    {
        if (asset == null)
        {
            Debug.LogError(
                "FlowFieldManager: Asset is null.");
            return;
        }

        DisposeCurrent();

        FlowFieldGrid grid =
            FlowFieldGridFactory.Create(asset);

        ActiveTargetWorldPosition =
            targetWorldPosition;

        ActiveTargetCell =
            FlowFieldGridUtility.WorldToCell(
                targetWorldPosition,
                grid.Origin,
                grid.CellSize);

        ActiveFlowField =
            new FlowField(grid);

        ActiveFlowField.Build(
            ActiveTargetCell);

        ActiveSampler =
            new FlowFieldSampler(grid);

        Debug.Log(
            $"FlowField Built\n" +
            $"Target World: {ActiveTargetWorldPosition}\n" +
            $"Target Cell : {ActiveTargetCell}");
    }

    /// <summary>
    /// Rebuild using an already known grid cell.
    /// Useful for debugging.
    /// </summary>
    public void Rebuild(
        FlowFieldAsset asset,
        int2 targetCell)
    {
        if (asset == null)
        {
            Debug.LogError(
                "FlowFieldManager: Asset is null.");
            return;
        }

        DisposeCurrent();

        FlowFieldGrid grid =
            FlowFieldGridFactory.Create(asset);

        ActiveTargetCell =
            targetCell;

        ActiveTargetWorldPosition =
            FlowFieldGridUtility.CellToWorld(
                targetCell,
                grid.Origin,
                grid.CellSize);

        ActiveFlowField =
            new FlowField(grid);

        ActiveFlowField.Build(
            ActiveTargetCell);

        ActiveSampler =
            new FlowFieldSampler(grid);

        Debug.Log(
            $"FlowField Built\n" +
            $"Target World: {ActiveTargetWorldPosition}\n" +
            $"Target Cell : {ActiveTargetCell}");
    }

    public float2 SampleDirection(
        Vector3 worldPosition)
    {
        if (ActiveSampler == null)
            return float2.zero;

        return ActiveSampler.SampleDirection(
            worldPosition);
    }

    public bool HasActiveFlowField =>
        ActiveFlowField != null;

    private void DisposeCurrent()
    {
        ActiveFlowField?.Dispose();

        ActiveFlowField = null;
        ActiveSampler = null;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DisposeCurrent();
    }
}