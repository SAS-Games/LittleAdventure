using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public readonly struct ArenaValue<T> where T : struct
{
    private readonly NativeArray<T> array;

    internal ArenaValue(NativeArray<T> arr)
    {
        array = arr;
    }

    public ref T Value
    {
        get
        {
            unsafe
            {
                return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), 0);
            }
        }
    }

    public NativeArray<T> AsNativeArray() => array;
}