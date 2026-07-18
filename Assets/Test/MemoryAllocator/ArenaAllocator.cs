using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public unsafe sealed class ArenaAllocator : IDisposable
{
    private byte* basePtr;
    private long capacity;      // total bytes
    private long offset;        // allocated bytes
    private bool disposed;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
    private AtomicSafetyHandle safetyHandle;
    private bool safetyInitialized;
#endif

    public ArenaAllocator(long capacityBytes)
    {
        if (capacityBytes <= 0) throw new ArgumentOutOfRangeException(nameof(capacityBytes));

        capacity = capacityBytes;
        offset   = 0;
        disposed = false;

        basePtr = (byte*)UnsafeUtility.Malloc(capacity, 16, Allocator.Persistent);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        safetyHandle      = AtomicSafetyHandle.Create();
        safetyInitialized = true;
#endif
    }

   
    private static void ValidateAlignment(int alignment)
    {
        if (alignment <= 0) throw new ArgumentOutOfRangeException(nameof(alignment));
        if ((alignment & (alignment - 1)) != 0)
            throw new ArgumentException("Alignment must be a power of two.", nameof(alignment));
    }

    internal bool TryAllocateBytes(long bytes, out void* result, int alignment)
    {
        result = null;
        if (disposed) throw new ObjectDisposedException(nameof(ArenaAllocator));
        if (bytes <= 0) return false;

        ValidateAlignment(alignment);

        // Current pointer
        byte* cur = basePtr + offset;

        // Align the pointer
        long alignMask = alignment - 1;
        byte* aligned = (byte*)(((ulong)cur + (ulong)alignMask) & ~(ulong)alignMask);

        long newOffset = (aligned - basePtr) + bytes;

        if (newOffset > capacity) return false;

        result = aligned;
        offset = newOffset;
        return true;
    }

    internal void* AllocateBytes(long bytes, int alignment)
    {
        if (!TryAllocateBytes(bytes, out void* ptr, alignment))
            throw new OutOfMemoryException($"ArenaAllocator overflow allocating {bytes} bytes.");

        return ptr;
    }

   
    public ArenaValue<T> AllocateValue<T>() where T : struct
    {
        long size  = UnsafeUtility.SizeOf<T>();
        int  align = UnsafeUtility.AlignOf<T>();

        void* ptr = AllocateBytes(size, align);
        var arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, 1, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, safetyHandle);
#endif

        return new ArenaValue<T>(arr);
    }

    public ArenaArray<T> AllocateArray<T>(int length) where T : struct
    {
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

        long bytes = (long)UnsafeUtility.SizeOf<T>() * length;
        int align  = UnsafeUtility.AlignOf<T>();

        void* ptr = AllocateBytes(bytes, align);
        var arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, length, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, safetyHandle);
#endif

        return new ArenaArray<T>(arr);
    }

    public void Reset()
    {
        if (disposed) throw new ObjectDisposedException(nameof(ArenaAllocator));

        offset = 0;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.Release(safetyHandle);
        safetyHandle = AtomicSafetyHandle.Create();
#endif
    }

    public long UsedBytes => offset;
    public long FreeBytes => capacity - offset;

    public void Dispose()
    {
        if (disposed) return;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (safetyInitialized)
        {
            AtomicSafetyHandle.Release(safetyHandle);
            safetyInitialized = false;
        }
#endif

        if (basePtr != null)
        {
            UnsafeUtility.Free(basePtr, Allocator.Persistent);
            basePtr = null;
        }

        disposed = true;
        GC.SuppressFinalize(this);
    }

    ~ArenaAllocator()
    {
        if (!disposed)
        {
            Debug.LogWarning("ArenaAllocator leaked — auto-dispose triggered.");
            Dispose();
        }
    }
}
