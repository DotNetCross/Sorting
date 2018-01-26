using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetCross.Sorting
{
    // Code-gen was poor when using this...
    //internal static class SortValues<T>
    //{
    //    public static readonly bool Yes = typeof(T) != typeof(Void);
    //}

    //internal interface ISwapper
    //{
    //    void Swap(int i, int j);
    //}
    //
    //// https://github.com/dotnet/roslyn/issues/20226
    //internal ref struct Swapper<T> : ISwapper // DAMN!!! ref structs can't inherit interfaces!!!
    //{
    //    Span<T> _items;
    //
    //    public Swapper(Span<T> items)
    //    {
    //        _items = items;
    //    }
    //
    //    public void Swap(int i, int j)
    //    {
    //        ref T start = ref _items.DangerousGetPinnableReference();
    //        SpanSortHelper.Swap(ref Unsafe.Add(ref start, i),
    //                            ref Unsafe.Add(ref start, j));
    //    }
    //}

    // Could be computed as below, but overhead for small lengths probably too big
    // https://stackoverflow.com/questions/11376288/fast-computing-of-log2-for-64-bit-integers
    //const int tab32[32] = {
    //     0,  9,  1, 10, 13, 21,  2, 29,
    //    11, 14, 16, 18, 22, 25,  3, 30,
    //     8, 12, 20, 28, 15, 17, 24,  7,
    //    19, 27, 23,  6, 26,  5,  4, 31};
    //
    //int log2_32(uint32_t value)
    //{
    //    value |= value >> 1;
    //    value |= value >> 2;
    //    value |= value >> 4;
    //    value |= value >> 8;
    //    value |= value >> 16;
    //    return tab32[(uint32_t)(value * 0x07C4ACDD) >> 27];
    //}

    // Note how old used the full length of keys array to limit, seems like a bug.
    //IntroSort(keys, left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2PlusOne(keys.Length), comparer);
    // In native code this is done right, so only for when using managed code:
    // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
    // https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.h#L139
    // This appears to have been fixed in native and now in managed:
    // https://github.com/dotnet/coreclr/pull/16002
}
