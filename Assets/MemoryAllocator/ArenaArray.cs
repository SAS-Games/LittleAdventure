using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public readonly struct ArenaArray<T> where T : struct
{
    private readonly NativeArray<T> array;

    internal ArenaArray(NativeArray<T> arr)
    {
        array = arr;
    }

    public int Length => array.Length;

    public ref T this[int index]
    {
        get
        {
            unsafe
            {
                return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
            }
        }
    }

    public NativeArray<T> AsNativeArray() => array;
}