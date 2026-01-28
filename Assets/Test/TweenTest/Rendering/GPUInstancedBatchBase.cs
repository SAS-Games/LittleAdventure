using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public struct InstanceRenderData
{
    public Matrix4x4 objectToWorld;
    public float4 color;
}

public abstract class GPUInstancedBatchBase : MonoBehaviour
{
    [SerializeField] private Mesh m_Mesh;
    [SerializeField] private Material m_Material;
    [SerializeField] private Bounds m_WorldBounds = new Bounds(Vector3.zero, Vector3.one * 100f);

    private readonly List<Transform> _instanceTransforms = new();
    private readonly Dictionary<Transform, int> _indexMap = new();

    protected TransformAccessArray _transformAccess;
    protected NativeArray<InstanceRenderData> _instanceData;
    private ComputeBuffer _instanceBuffer;

    private RenderParams _renderParams;
    private int _count;
    protected virtual bool RequiresTransformAccess => true;
    protected virtual bool RequiresPerFrameUpload => true;

    protected virtual void Awake()
    {
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

        if (RequiresTransformAccess)
            UpdateInstanceData();

        if (RequiresPerFrameUpload)
            _instanceBuffer.SetData(_instanceData, 0, 0, _count);

        m_Material.SetBuffer("_InstanceDataBuffer", _instanceBuffer);
        Graphics.RenderMeshPrimitives(_renderParams, m_Mesh, 0, _count);
    }

    protected virtual void OnDestroy()
    {
        DisposeNative();
    }

    protected abstract void UpdateInstanceData();
    public void Register(Transform t)
    {
        Register(t, Color.white);
    }

    public void Register(Transform t, Color color)
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

        SetColor(t, color);
        SetFromTransform(t);
        _count++;
    }

    public void Unregister(Transform t)
    {
        if (!_indexMap.TryGetValue(t, out int index))
            return;

        RemoveAtSwapBack(index);
    }

    public void SetPosition(Transform t, Vector3 position)
    {
        if (!_indexMap.TryGetValue(t, out int index))
            return;

        InstanceRenderData data = _instanceData[index];

        Matrix4x4 m = data.objectToWorld;
        m.m03 = position.x;
        m.m13 = position.y;
        m.m23 = position.z;

        data.objectToWorld = m;
        _instanceData[index] = data;
    }

    public void SetRotation(Transform t, Quaternion rotation)
    {
        if (!_indexMap.TryGetValue(t, out int index))
            return;

        InstanceRenderData data = _instanceData[index];

        Vector3 pos = data.objectToWorld.GetColumn(3);
        Vector3 scale = Vector3.one;

        data.objectToWorld = Matrix4x4.TRS(pos, rotation, scale);
        _instanceData[index] = data;
    }


    public void SetFromTransform(Transform t)
    {
        if (!_indexMap.TryGetValue(t, out int index))
            return;

        InstanceRenderData data = _instanceData[index];
        data.objectToWorld = t.localToWorldMatrix;
        _instanceData[index] = data;
    }


    public void SetTRS(Transform t, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (!_indexMap.TryGetValue(t, out int index))
            return;

        InstanceRenderData data = _instanceData[index];
        data.objectToWorld = Matrix4x4.TRS(position, rotation, scale);
        _instanceData[index] = data;
    }


    public void SetColor(Transform t, Color color)
    {
        if (!_indexMap.TryGetValue(t, out int index))
            return;

        InstanceRenderData data = _instanceData[index];
        data.color = new float4(color.r, color.g, color.b, color.a);
        _instanceData[index] = data;
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

        _instanceBuffer = new ComputeBuffer(newSize, sizeof(float) * (16 + 4));
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