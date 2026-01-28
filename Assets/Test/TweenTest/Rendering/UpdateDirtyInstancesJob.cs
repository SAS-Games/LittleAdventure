using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;

[BurstCompile]
public struct UpdateDirtyInstancesJob : IJobParallelForTransform
{
    [NativeDisableParallelForRestriction]
    public NativeArray<InstanceRenderData> instanceData;

    [ReadOnly]
    public NativeArray<int> renderIndices;

    public void Execute(int index, TransformAccess transform)
    {
        int renderIndex = renderIndices[index];

        InstanceRenderData data = instanceData[renderIndex];
        data.objectToWorld = transform.localToWorldMatrix;
        instanceData[renderIndex] = data;
    }
}
