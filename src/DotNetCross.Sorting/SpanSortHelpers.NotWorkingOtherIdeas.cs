using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DotNetCross.Sorting
{
    public static class OtherIdeas
    {
        // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs

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


        //private static void IntroSort<TKey, TComparer>(ref TKey keys, ref int values, int hi, int depthLimit, in TComparer comparer)
        //    where TComparer : IDirectComparer<TKey>
        //{
        //    Debug.Assert(comparer != null);
        //    //Debug.Assert(lo >= 0);
        //    const int lo = 0;
        //    while (hi > lo)
        //    {
        //        int partitionSize = hi - lo + 1;
        //        if (partitionSize <= IntrosortSizeThreshold)
        //        {
        //            if (partitionSize == 1)
        //            {
        //                return;
        //            }

        //            if (partitionSize == 2)
        //            {
        //                Sort2(ref keys, lo, hi, comparer);
        //                return;
        //            }
        //            if (partitionSize == 3)
        //            {
        //                ref TKey loRef = ref Unsafe.Add(ref keys, lo);
        //                ref TKey miRef = ref Unsafe.Add(ref keys, hi - 1);
        //                ref TKey hiRef = ref Unsafe.Add(ref keys, hi);
        //                Sort3(ref loRef, ref miRef, ref hiRef, comparer);
        //                return;
        //            }

        //            InsertionSort(ref keys, ref values, lo, hi, comparer);
        //            return;
        //        }

        //        if (depthLimit == 0)
        //        {
        //            HeapSort(ref keys, ref values, lo, hi, comparer);
        //            return;
        //        }
        //        depthLimit--;

        //        // We should never reach here, unless > 3 elements due to partition size
        //        //ref var keysAtLo = ref Unsafe.Add(ref keys, lo);
        //        int p = PickPivotAndPartition(ref keys, ref values, lo, hi, comparer);
        //        // Note we've already partitioned around the pivot and do not have to move the pivot again.
        //        ref var afterPivot = ref Unsafe.Add(ref keys, p + 1);
        //        IntroSort(ref afterPivot, ref values, hi - (p + 1), depthLimit, comparer);
        //        hi = p - 1;
        //    }
        //}

        //private static int PickPivotAndPartitionIntPtrIndeces<TKey, TComparer>(
        //    ref TKey keys, ref int values, int lo, int hi,
        //    in TComparer comparer)
        //    where TComparer : IDirectComparer<TKey>
        //{
        //    Debug.Assert(comparer != null);
        //    Debug.Assert(lo >= 0);
        //    Debug.Assert(hi > lo);

        //    // Compute median-of-three.  But also partition them, since we've done the comparison.

        //    // PERF: `lo` or `hi` will never be negative inside the loop,
        //    //       so computing median using uints is safe since we know 
        //    //       `length <= int.MaxValue`, and indices are >= 0
        //    //       and thus cannot overflow an uint. 
        //    //       Saves one subtraction per loop compared to 
        //    //       `int middle = lo + ((hi - lo) >> 1);`
        //    var middle = new IntPtr((int)(((uint)hi + (uint)lo) >> 1));

        //    var low = new IntPtr(lo);
        //    var high = new IntPtr(hi);

        //    // Sort lo, mid and hi appropriately, then pick mid as the pivot.
        //    ref TKey loRef = ref Unsafe.Add(ref keys, low);
        //    ref TKey miRef = ref Unsafe.Add(ref keys, middle);
        //    ref TKey hiRef = ref Unsafe.Add(ref keys, high);
        //    Sort3(ref loRef, ref miRef, ref hiRef, comparer);

        //    TKey pivot = miRef;

        //    // Put pivot in the right location.
        //    IntPtr left = low;
        //    IntPtr right = high - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.
        //    Swap(ref miRef, ref Unsafe.Add(ref keys, right));

        //    while (left.LessThan(right))
        //    {
        //        // TODO: Would be good to update local ref here
        //        do
        //        {
        //            left += 1;
        //        }
        //        while (comparer.LessThan(Unsafe.Add(ref keys, left), pivot));
        //        //while (comparer.Compare(pivot, Unsafe.Add(ref keys, left)) >= 0) ;
        //        // TODO: Would be good to update local ref here
        //        do
        //        {
        //            right -= 1;
        //        }
        //        while (comparer.LessThan(pivot, Unsafe.Add(ref keys, right)));

        //        //if (left >= right)
        //        //if (left.GreaterThanEqual(right))
        //        if (right.LessThan(left))
        //            break;

        //        // Indeces cannot be equal here
        //        Swap(ref keys, left, right);
        //    }
        //    // Put pivot in the right location.
        //    right = high - 1;
        //    if (left != right)
        //    {
        //        Swap(ref keys, left, right);
        //    }
        //    return (int)left;
        //}

        //private static int PickPivotAndPartitionIntPtrByteOffsets<TKey, TComparer>(
        //    ref TKey keys, ref int values, int lo, int hi,
        //    in TComparer comparer)
        //    where TComparer : IDirectComparer<TKey>
        //{
        //    Debug.Assert(comparer != null);
        //    Debug.Assert(lo >= 0);
        //    Debug.Assert(hi > lo);

        //    // Compute median-of-three.  But also partition them, since we've done the comparison.

        //    // PERF: `lo` or `hi` will never be negative inside the loop,
        //    //       so computing median using uints is safe since we know 
        //    //       `length <= int.MaxValue`, and indices are >= 0
        //    //       and thus cannot overflow an uint. 
        //    //       Saves one subtraction per loop compared to 
        //    //       `int middle = lo + ((hi - lo) >> 1);`
        //    var middle = new IntPtr((int)(((uint)hi + (uint)lo) >> 1));
        //    var low = new IntPtr(lo);
        //    var high = new IntPtr(hi);

        //    // Sort lo, mid and hi appropriately, then pick mid as the pivot.
        //    ref TKey loRef = ref Unsafe.Add(ref keys, low);
        //    ref TKey miRef = ref Unsafe.Add(ref keys, middle);
        //    ref TKey hiRef = ref Unsafe.Add(ref keys, high);
        //    Sort3(ref loRef, ref miRef, ref hiRef, comparer);

        //    TKey pivot = miRef;

        //    // Put pivot in the right location.
        //    IntPtr left = low;
        //    IntPtr right = high - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.
        //    Swap(ref miRef, ref Unsafe.Add(ref keys, right));

        //    IntPtr leftBytes = left.Multiply(Unsafe.SizeOf<TKey>());
        //    IntPtr rightBytes = right.Multiply(Unsafe.SizeOf<TKey>());

        //    while (leftBytes.LessThan(rightBytes))
        //    {
        //        // TODO: Would be good to update local ref here
        //        do
        //        {
        //            leftBytes += Unsafe.SizeOf<TKey>();
        //        }
        //        while (comparer.LessThan(Unsafe.AddByteOffset(ref keys, leftBytes), pivot));
        //        // TODO: Would be good to update local ref here
        //        do
        //        {
        //            rightBytes -= Unsafe.SizeOf<TKey>();
        //        }
        //        while (comparer.LessThan(pivot, Unsafe.AddByteOffset(ref keys, rightBytes)));

        //        if (leftBytes.GreaterThanEqual(rightBytes))
        //            break;

        //        // Indeces cannot be equal here
        //        //Swap(ref keys, left, right);
        //        Swap(ref Unsafe.AddByteOffset(ref keys, leftBytes), ref Unsafe.AddByteOffset(ref keys, rightBytes));
        //    }
        //    // Put pivot in the right location.
        //    //right = (hi - 1);
        //    rightBytes = new IntPtr(hi - 1).Multiply(Unsafe.SizeOf<TKey>());
        //    if (leftBytes != rightBytes)
        //    {
        //        //Swap(ref keys, left, right);
        //        Swap(ref Unsafe.AddByteOffset(ref keys, leftBytes), ref Unsafe.AddByteOffset(ref keys, rightBytes));
        //    }
        //    return (int)leftBytes.Divide(Unsafe.SizeOf<TKey>());
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static ref TKey Sort3<TKey, TComparer>(
        //    ref TKey keys, int i0, int i1, int i2,
        //    TComparer comparer)
        //    where TComparer : IDirectComparer<TKey>
        //{
        //    ref var r0 = ref Unsafe.Add(ref keys, i0);
        //    ref var r1 = ref Unsafe.Add(ref keys, i1);
        //    ref var r2 = ref Unsafe.Add(ref keys, i2);

        //    if (comparer.LessThan(r0, r1)) //r0 < r1)
        //    {
        //        if (comparer.LessThan(r1, r2)) //(r1 < r2)
        //        {
        //            return ref r1;
        //        }
        //        else if (comparer.LessThan(r0, r2)) //(r0 < r2)
        //        {
        //            Swap(ref r1, ref r2);
        //        }
        //        else
        //        {
        //            TKey tmp = r0;
        //            r0 = r2;
        //            r2 = r1;
        //            r1 = tmp;
        //        }
        //    }
        //    else
        //    {
        //        if (comparer.LessThan(r0, r2)) //(r0 < r2)
        //        {
        //            Swap(ref r0, ref r1);
        //        }
        //        else if (comparer.LessThan(r2, r1)) //(r2 < r1)
        //        {
        //            Swap(ref r0, ref r2);
        //        }
        //        else
        //        {
        //            TKey tmp = r0;
        //            r0 = r1;
        //            r1 = r2;
        //            r2 = tmp;
        //        }
        //    }
        //    return ref r1;
        //}
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static void Swap<T>(ref T items, IntPtr i, IntPtr j)
        //{
        //    Debug.Assert(i != j);
        //    Swap(ref Unsafe.Add(ref items, i), ref Unsafe.Add(ref items, j));
        //}
    }
}
