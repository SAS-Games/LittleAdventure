// using System.Collections.Generic;
// using UnityEngine;
//
// public sealed class GPUInstancedRenderSystem : MonoBehaviour
// {
//     public static GPUInstancedRenderSystem Instance;
//
//     [Header("Rendering")]
//     public Mesh mesh;
//     public Material material;
//
//     [Header("Source Transforms")]
//     public List<Transform> transforms = new();
//
//     struct InstanceData
//     {
//         public Matrix4x4 objectToWorld;
//
//         public static int Size()
//         {
//             return sizeof(float) * 16;
//         }
//     }
//
//     InstanceData[] _instanceData;
//     ComputeBuffer _buffer;
//     RenderParams _renderParams;
//
//     private void Awake()
//     {
//         Instance = this;
//     }
//
//     private void Start()
//     {
//         int count = transforms.Count;
//         _instanceData = new InstanceData[count];
//         _buffer = new ComputeBuffer(count, InstanceData.Size());
//         _renderParams = new RenderParams(material);
//         _renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
//     }
//
//     private void Update()
//     {
//         int count = transforms.Count;
//
//         for (int i = 0; i < count; i++)
//         {
//             _instanceData[i].objectToWorld = transforms[i].localToWorldMatrix;
//         }
//
//         _buffer.SetData(_instanceData);
//
//         material.SetBuffer("_InstanceDataBuffer", _buffer);
//
//         Graphics.RenderMeshPrimitives(_renderParams, mesh, 0, count);
//     }
//
//     private void OnDestroy()
//     {
//         if (_buffer != null)
//             _buffer.Release();
//     }
//
//    public void Register(Transform transform)
//     {
//         transforms.Add(transform);
//     }
// }