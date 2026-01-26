using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public struct InstanceRenderData
{
    public Matrix4x4 objectToWorld;
}

public sealed class GPUInstancedRenderSystem : MonoBehaviour
{
    public static GPUInstancedRenderSystem Instance { get; private set; }

    [SerializeField] private Mesh m_Mesh;
    [SerializeField] private Material m_Material;
    [SerializeField] private Bounds m_WorldBounds = new Bounds(Vector3.zero, Vector3.one * 100f);

    private readonly List<Transform> _instanceTransforms = new();
    private readonly Dictionary<Transform, int> _indexMap = new();

    private TransformAccessArray _transformAccess;
    private NativeArray<InstanceRenderData> _instanceData;
    private ComputeBuffer _instanceBuffer;

    private RenderParams _renderParams;
    private int _count;


    void Awake()
    {
        Instance = this;

        _transformAccess = new TransformAccessArray(64);

        _renderParams = new RenderParams(m_Material)
        {
            worldBounds = m_WorldBounds
        };
    }

    void Update()
    {
        if (_count == 0)
            return;

        var job = new ExtractInstanceDataJob
        {
            instanceData = _instanceData
        };

        JobHandle handle = job.Schedule(_transformAccess);
        handle.Complete();

        _instanceBuffer.SetData(_instanceData, 0, 0, _count);
        m_Material.SetBuffer("_InstanceDataBuffer", _instanceBuffer);

        Graphics.RenderMeshPrimitives(_renderParams, m_Mesh, 0, _count);
    }

    void OnDestroy()
    {
        DisposeNative();
        Instance = null;
    }

    
    public void Register(Transform t)
    {
        if (t == null || _indexMap.ContainsKey(t))
            return;

        int index = _count;

        _indexMap[t] = index;
        _instanceTransforms.Add(t);

        if (_count >= _transformAccess.capacity)
            _transformAccess.capacity = math.max(64, _transformAccess.capacity * 2);

        _transformAccess.Add(t);

        ResizeInstanceDataIfNeeded(_count + 1);
        ResizeBufferIfNeeded(_count + 1);

        _count++;
    }

    public void Unregister(Transform t)
    {
        if (!_indexMap.TryGetValue(t, out int index))
            return;

        RemoveAtSwapBack(index);
    }
    
    private void RemoveAtSwapBack(int index)
    {
        int last = _count - 1;

        Transform removed = _instanceTransforms[index];

        if (index != last)
        {
            Transform lastTransform = _instanceTransforms[last];

            _instanceTransforms[index] = lastTransform;
            _indexMap[lastTransform] = index;

            _instanceData[index] = _instanceData[last];
        }

        _indexMap.Remove(removed);

        _transformAccess.RemoveAtSwapBack(index);
        _instanceTransforms.RemoveAt(last);

        _count--;
    }
    
    private void ResizeInstanceDataIfNeeded(int needed)
    {
        if (_instanceData.IsCreated && _instanceData.Length >= needed)
            return;

        int oldLength = _instanceData.IsCreated ? _instanceData.Length : 0;
        int newSize = math.max(needed, oldLength > 0 ? oldLength * 2 : 64);

        var newArray = new NativeArray<InstanceRenderData>(newSize, Allocator.Persistent);

        if (_instanceData.IsCreated)
        {
            NativeArray<InstanceRenderData>.Copy(_instanceData, newArray, oldLength);
            _instanceData.Dispose();
        }

        _instanceData = newArray;
    }

    private void ResizeBufferIfNeeded(int needed)
    {
        int oldCapacity = _instanceBuffer != null ? _instanceBuffer.count : 0;

        if (_instanceBuffer != null && oldCapacity >= needed)
            return;

        if (_instanceBuffer != null)
        {
            _instanceBuffer.Release();
            _instanceBuffer = null;
        }

        int newSize = math.max(needed, oldCapacity > 0 ? oldCapacity * 2 : 64);

        _instanceBuffer = new ComputeBuffer(newSize, sizeof(float) * 16);
    }
    
    private void DisposeNative()
    {
        if (_transformAccess.isCreated)
            _transformAccess.Dispose();

        if (_instanceData.IsCreated)
            _instanceData.Dispose();

        if (_instanceBuffer != null)
        {
            _instanceBuffer.Release();
            _instanceBuffer = null;
        }
    }
}
