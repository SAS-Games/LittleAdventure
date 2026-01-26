using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
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

    private TransformAccessArray _transformAccess;
    private NativeArray<InstanceRenderData> _instanceData;

    private ComputeBuffer _instanceBuffer;
    private RenderParams _renderParams;

    private bool _dirty;

    void Awake()
    {
        Instance = this;

        _renderParams = new RenderParams(m_Material)
        {
            worldBounds = m_WorldBounds
        };
    }

    void Update()
    {
        if (_dirty)
            Rebuild();

        if (!_transformAccess.isCreated || _transformAccess.length == 0)
            return;

        var job = new ExtractInstanceDataJob
        {
            instanceData = _instanceData,
        };

        JobHandle handle = job.Schedule(_transformAccess);
        handle.Complete();

        _instanceBuffer.SetData(_instanceData);
        m_Material.SetBuffer("_InstanceDataBuffer", _instanceBuffer);

        Graphics.RenderMeshPrimitives(_renderParams, m_Mesh, 0, _instanceData.Length);
    }

    public void Register(Transform t)
    {
        if (t == null)
            return;

        _instanceTransforms.Add(t);
        _dirty = true;
    }

    public void Unregister(Transform t)
    {
        if (t == null)
            return;

        int index = _instanceTransforms.IndexOf(t);
        if (index < 0)
            return;

        _instanceTransforms.RemoveAt(index);
        _dirty = true;
    }

    void Rebuild()
    {
        DisposeNative();

        int count = _instanceTransforms.Count;
        if (count == 0)
            return;

        _instanceData = new NativeArray<InstanceRenderData>(count, Allocator.Persistent);
        _transformAccess = new TransformAccessArray(_instanceTransforms.ToArray());

        _instanceBuffer = new ComputeBuffer(count, sizeof(float) * 16);
        _dirty = false;
    }

    void DisposeNative()
    {
        if (_transformAccess.isCreated)
            _transformAccess.Dispose();

        if (_instanceData.IsCreated)
            _instanceData.Dispose();

        if (_instanceBuffer != null)
            _instanceBuffer.Release();
    }

    void OnDestroy()
    {
        DisposeNative();
        Instance = null;
        _dirty = false;
    }
}