using Unity.Collections;
using UnityEngine.Jobs;
using System.Collections.Generic;
using UnityEngine;
public sealed class TweenedBatch : GPUInstancedBatchBase
{
    private TransformAccessArray _dirtyTransforms;
    private NativeArray<int> _dirtyRenderIndices;

    private readonly Dictionary<int, int> _dirtyIndexMap = new();

    public static TweenedBatch Instance { get; private set; }
    protected override void Awake()
    {
        Instance = this;
        base.Awake();

        _dirtyTransforms = new TransformAccessArray(64);
        _dirtyRenderIndices = new NativeArray<int>(64, Allocator.Persistent);
    }

    protected override void UpdateInstanceData()
    {
        if (_dirtyTransforms.length == 0)
            return;

        var job = new UpdateDirtyInstancesJob
        {
            instanceData = _instanceData,
            renderIndices = _dirtyRenderIndices
        };

        job.Schedule(_dirtyTransforms).Complete();
    }

    protected override void OnDestroy()
    {
        if (_dirtyTransforms.isCreated)
            _dirtyTransforms.Dispose();

        if (_dirtyRenderIndices.IsCreated)
            _dirtyRenderIndices.Dispose();

        base.OnDestroy();
        Instance = null;
    }


    public void SetDirty(Transform t, bool dirty)
    {
        int id = t.GetInstanceID();

        if (dirty)
        {
            if (_dirtyIndexMap.ContainsKey(id))
                return;

            if (!TryGetRenderIndex(id, out int renderIndex))
                return;

            ResizeDirtyIfNeeded(_dirtyTransforms.length + 1);

            int dirtyIndex = _dirtyTransforms.length;

            _dirtyIndexMap[id] = dirtyIndex;

            _dirtyTransforms.Add(t);
            _dirtyRenderIndices[dirtyIndex] = renderIndex;
        }
        else
        {
            RemoveDirty(id);
        }
    }

    private void ResizeDirtyIfNeeded(int needed)
    {
        if (_dirtyRenderIndices.IsCreated && _dirtyRenderIndices.Length >= needed)
            return;

        int old = _dirtyRenderIndices.IsCreated ? _dirtyRenderIndices.Length : 0;
        int newSize = Mathf.Max(needed, old > 0 ? old * 2 : 32);

        var newArray = new NativeArray<int>(newSize, Allocator.Persistent);

        if (_dirtyRenderIndices.IsCreated)
        {
            NativeArray<int>.Copy(_dirtyRenderIndices, newArray, old);
            _dirtyRenderIndices.Dispose();
        }

        _dirtyRenderIndices = newArray;
    }

    private void RemoveDirty(int id)
    {
        if (!_dirtyIndexMap.TryGetValue(id, out int index))
            return;

        int last = _dirtyTransforms.length - 1;

        if (index != last)
        {
            _dirtyTransforms[index] = _dirtyTransforms[last];
            _dirtyRenderIndices[index] = _dirtyRenderIndices[last];

            int swappedId = _dirtyTransforms[index].GetInstanceID();

            _dirtyIndexMap[swappedId] = index;
        }

        _dirtyTransforms.RemoveAtSwapBack(index);
        _dirtyIndexMap.Remove(id);
    }
}
