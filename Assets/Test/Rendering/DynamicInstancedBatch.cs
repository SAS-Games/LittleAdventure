using Unity.Jobs;
using UnityEngine.Jobs;
public sealed class DynamicInstancedBatch : GPUInstancedBatchBase
{
    public static DynamicInstancedBatch Instance { get; private set; }
    protected override void Awake()
    {
        Instance = this;
        base.Awake();
    }
    protected override void UpdateInstanceData()
    {
        var job = new ExtractInstanceDataJob
        {
            instanceData = _instanceData
        };

        JobHandle handle = job.Schedule(_transformAccess);
        handle.Complete();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Instance = null;
    }
}
