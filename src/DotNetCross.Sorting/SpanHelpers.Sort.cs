// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

namespace System
{
    internal static partial class SpanHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<T>(this Span<T> span)
        {
            Span<int> values = default;
            // PERF: Try specialized here for optimal performance
            // Code-gen is weird unless used in loop outside
            if (!SpanSortHelper.TrySortSpecialized(span, ref values.DangerousGetPinnableReference()))
            {
                Sort(span, Comparer<T>.Default);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<T, TComparer>(
            this Span<T> span, TComparer comparer)
            where TComparer : IComparer<T>
        {
            Span<int> values = default;
            DefaultSpanSortHelper<T, TComparer>.s_default.Sort(span, ref values.DangerousGetPinnableReference(), comparer);
        }

        internal interface ILessThanComparer<T>
        {
            bool LessThan(T x, T y);
        }
        //
        // Type specific LessThanComparer(s) to ensure optimal code-gen
        //
        internal struct SByteLessThanComparer : ILessThanComparer<sbyte>
        {
            public bool LessThan(sbyte x, sbyte y) => x < y;
        }
        internal struct ByteLessThanComparer : ILessThanComparer<byte>
        {
            public bool LessThan(byte x, byte y) => x < y;
        }
        internal struct Int16LessThanComparer : ILessThanComparer<short>
        {
            public bool LessThan(short x, short y) => x < y;
        }
        internal struct UInt16LessThanComparer : ILessThanComparer<ushort>
        {
            public bool LessThan(ushort x, ushort y) => x < y;
        }
        internal struct Int32LessThanComparer : ILessThanComparer<int>
        {
            public bool LessThan(int x, int y) => x < y;
        }
        internal struct UInt32LessThanComparer : ILessThanComparer<uint>
        {
            public bool LessThan(uint x, uint y) => x < y;
        }
        internal struct Int64LessThanComparer : ILessThanComparer<long>
        {
            public bool LessThan(long x, long y) => x < y;
        }
        internal struct UInt64LessThanComparer : ILessThanComparer<ulong>
        {
            public bool LessThan(ulong x, ulong y) => x < y;
        }
        internal struct SingleLessThanComparer : ILessThanComparer<float>
        {
            public bool LessThan(float x, float y) => x < y;
        }
        internal struct DoubleLessThanComparer : ILessThanComparer<double>
        {
            public bool LessThan(double x, double y) => x < y;
        }

        // Helper to allow sharing all code via inlineable functor for IComparer<T>
        internal struct ComparerLessThanComparer<T, TComparer> : ILessThanComparer<T>
            where TComparer : IComparer<T>
        {
            readonly TComparer _comparer;

            public ComparerLessThanComparer(TComparer comparer)
            {
                _comparer = comparer;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(T x, T y) => _comparer.Compare(x, y) < 0;
        }
        // Helper to allow sharing all code via inlineable functor for IComparable<T>
        internal struct ComparableLessThanComparer<T> : ILessThanComparer<T>//, IComparer<T>
            where T : IComparable<T>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(T x, T y) => x.CompareTo(y) < 0;
        }


        // Helper to allow sharing all code via IComparer<T> inlineable
        internal struct ComparisonComparer<T> : IComparer<T>
        {
            readonly Comparison<T> m_comparison;

            public ComparisonComparer(Comparison<T> comparison)
            {
                m_comparison = comparison;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(T x, T y) => m_comparison(x, y);

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //public bool LessThan(T x, T y) => m_comparison(x, y) < 0;
        }

        // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
        internal interface ISpanSortHelper<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            void Sort(Span<TKey> keys, ref int values, in TComparer comparer);
        }

        internal interface IIsNaN<T>
        {
            bool IsNaN(T value);
        }
        internal struct SingleIsNaN : IIsNaN<float>
        {
            public bool IsNaN(float value) => float.IsNaN(value);
        }
        internal struct DoubleIsNaN : IIsNaN<double>
        {
            public bool IsNaN(double value) => double.IsNaN(value);
        }

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

        internal static class SpanSortHelper
        {
            // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
            // https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp

            // This is the threshold where Introspective sort switches to Insertion sort.
            // Empirically, 16 seems to speed up most cases without slowing down others, at least for integers.
            // Large value types may benefit from a smaller number.
            internal const int IntrosortSizeThreshold = 16;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool TrySortSpecialized<T>(Span<T> span, ref int values)
            {
                int length = span.Length;
                // Type unfolding adopted from https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp#L268
                if (typeof(T) == typeof(sbyte))
                {
                    ref var keys = ref Unsafe.As<T, sbyte>(ref span.DangerousGetPinnableReference());//ref MemoryManager.GetReference(span));
                    Sort(ref keys, ref values, length, new SByteLessThanComparer());

                    return true;
                }
                else if (typeof(T) == typeof(byte) ||
                         typeof(T) == typeof(bool)) // Use byte for bools to reduce code size
                {
                    ref var keys = ref Unsafe.As<T, byte>(ref span.DangerousGetPinnableReference());//ref MemoryManager.GetReference(span));
                    Sort(ref keys, ref values, length, new ByteLessThanComparer());

                    return true;
                }
                else if (typeof(T) == typeof(short) ||
                         typeof(T) == typeof(char)) // Use short for chars to reduce code size
                {
                    ref var keys = ref Unsafe.As<T, short>(ref span.DangerousGetPinnableReference());//ref MemoryManager.GetReference(span));
                    Sort(ref keys, ref values, length, new Int16LessThanComparer());

                    return true;
                }
                else if (typeof(T) == typeof(ushort))
                {
                    ref var keys = ref Unsafe.As<T, ushort>(ref span.DangerousGetPinnableReference());//ref MemoryManager.GetReference(span));
                    Sort(ref keys, ref values, length, new UInt16LessThanComparer());

                    return true;
                }
                else if (typeof(T) == typeof(int))
                {
                    ref var keys = ref Unsafe.As<T, int>(ref span.DangerousGetPinnableReference());//ref MemoryManager.GetReference(span));
                    Sort(ref keys, ref values, length, new Int32LessThanComparer());

                    return true;
                }
                else if (typeof(T) == typeof(uint))
                {
                    ref var keys = ref Unsafe.As<T, uint>(ref span.DangerousGetPinnableReference());//ref MemoryManager.GetReference(span));
                    Sort(ref keys, ref values, length, new UInt32LessThanComparer());

                    return true;
                }
                else if (typeof(T) == typeof(long))
                {
                    ref var keys = ref Unsafe.As<T, long>(ref span.DangerousGetPinnableReference());//ref MemoryManager.GetReference(span));
                    Sort(ref keys, ref values, length, new Int64LessThanComparer());

                    return true;
                }
                else if (typeof(T) == typeof(ulong))
                {
                    ref var keys = ref Unsafe.As<T, ulong>(ref span.DangerousGetPinnableReference());//ref MemoryManager.GetReference(span));
                    Sort(ref keys, ref values, length, new UInt64LessThanComparer());

                    return true;
                }
                else if (typeof(T) == typeof(float))
                {
                    ref var keys = ref Unsafe.As<T, float>(ref span.DangerousGetPinnableReference());//ref MemoryManager.GetReference(span));

                    // Comparison to NaN is always false, so do a linear pass 
                    // and swap all NaNs to the front of the array
                    var left = NaNPrepass(ref keys, ref values, length, new SingleIsNaN());

                    ref var afterNaNsKeys = ref Unsafe.Add(ref keys, left);
                    Sort(ref afterNaNsKeys, ref values, length - left, new SingleLessThanComparer());

                    return true;
                }
                else if (typeof(T) == typeof(double))
                {
                    ref var keys = ref Unsafe.As<T, double>(ref span.DangerousGetPinnableReference());//ref MemoryManager.GetReference(span));

                    // Comparison to NaN is always false, so do a linear pass 
                    // and swap all NaNs to the front of the array
                    var left = NaNPrepass(ref keys, ref values, length, new DoubleIsNaN());

                    ref var afterNaNsKeys = ref Unsafe.Add(ref keys, left);
                    Sort(ref afterNaNsKeys, ref values, length - left, new DoubleLessThanComparer());

                    return true;
                }
                else
                {
                    return false;
                }
            }

            // For sorting, move all NaN instances to front of the input array
            private static int NaNPrepass<T, TIsNaN>(ref T keys, ref int values, int length, in TIsNaN isNaN)
                where TIsNaN : struct, IIsNaN<T>
            {
                int left = 0;
                for (int i = 0; i <= length; i++)
                {
                    ref T current = ref Unsafe.Add(ref keys, i);
                    if (isNaN.IsNaN(current))
                    {
                        ref T previous = ref Unsafe.Add(ref keys, left);

                        Swap(ref previous, ref current);

                        ++left;
                    }
                }
                return left;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Sort<T, TComparer>(ref T keys, ref int values, int length, in TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                if (length < 2)
                    return;

                IntrospectiveSort(ref keys, ref values, length, comparer);
            }

            private static void IntrospectiveSort<T, TComparer>(ref T keys, ref int values, int length, in TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                // Note how old used the full length of keys array to limit, seems like a bug!
                //IntroSort(keys, left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2PlusOne(keys.Length), comparer);
                // In native code this is done right, so only for when using managed code:
                // https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.h#L139
                var depthLimit = 2 * FloorLog2PlusOne(length);
                IntroSort(ref keys, ref values, 0, length - 1, depthLimit, comparer);
                //IntroSort(ref keys, length - 1, depthLimit, comparer);
            }

            private static int FloorLog2PlusOne(int n)
            {
                int result = 0;
                while (n >= 1)
                {
                    result++;
                    n = n / 2;
                }
                return result;

                // Could be computed as below, but overhead for small lengths probably too big
                // https://stackoverflow.com/questions/11376288/fast-computing-of-log2-for-64-bit-integers
                //const int tab32[32] = {
                //     0,  9,  1, 10, 13, 21,  2, 29,
                //    11, 14, 16, 18, 22, 25,  3, 30,
                //     8, 12, 20, 28, 15, 17, 24,  7,
                //    19, 27, 23,  6, 26,  5,  4, 31};

                //int log2_32(uint32_t value)
                //{
                //    value |= value >> 1;
                //    value |= value >> 2;
                //    value |= value >> 4;
                //    value |= value >> 8;
                //    value |= value >> 16;
                //    return tab32[(uint32_t)(value * 0x07C4ACDD) >> 27];
                //}
            }

            private static void IntroSort<T, TComparer>(ref T keys, ref int values, int lo, int hi, int depthLimit, in TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);

                while (hi > lo)
                {
                    int partitionSize = hi - lo + 1;
                    if (partitionSize <= IntrosortSizeThreshold)
                    {
                        if (partitionSize == 1)
                        {
                            return;
                        }
                        if (partitionSize == 2)
                        {
                            SwapIfGreater(ref keys, lo, hi, comparer);
                            return;
                        }
                        if (partitionSize == 3)
                        {
                            ref T loRef = ref Unsafe.Add(ref keys, lo);
                            ref T miRef = ref Unsafe.Add(ref keys, hi - 1);
                            ref T hiRef = ref Unsafe.Add(ref keys, hi);
                            //ref T miRef = ref Unsafe.SubtractByteOffset(ref hiRef, new IntPtr(Unsafe.SizeOf<T>()));
                            Sort3(ref loRef, ref miRef, ref hiRef, comparer);
                            return;
                        }

                        InsertionSort(ref keys, ref values, lo, hi, comparer);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        HeapSort(ref keys, ref values, lo, hi, comparer);
                        return;
                    }
                    depthLimit--;

                    // We should never reach here, unless > 3 elements due to partition size
                    int p = PickPivotAndPartitionIntIndeces(ref keys, ref values, lo, hi, comparer);
                    //int p = PickPivotAndPartitionIntPtrIndeces(ref keys, lo, hi, comparer);
                    //int p = PickPivotAndPartitionIntPtrByteOffsets(ref keys, ref values, lo, hi, comparer);
                    // Note we've already partitioned around the pivot and do not have to move the pivot again.
                    IntroSort(ref keys, ref values,p + 1, hi, depthLimit, comparer);
                    hi = p - 1;
                }
            }

            private static void IntroSort<T, TComparer>(ref T keys, ref int values, int hi, int depthLimit, in TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                Debug.Assert(comparer != null);
                //Debug.Assert(lo >= 0);
                const int lo = 0;
                while (hi > lo)
                {
                    int partitionSize = hi - lo + 1;
                    if (partitionSize <= IntrosortSizeThreshold)
                    {
                        if (partitionSize == 1)
                        {
                            return;
                        }

                        if (partitionSize == 2)
                        {
                            SwapIfGreater(ref keys, lo, hi, comparer);
                            return;
                        }
                        if (partitionSize == 3)
                        {
                            ref T loRef = ref Unsafe.Add(ref keys, lo);
                            ref T miRef = ref Unsafe.Add(ref keys, hi - 1);
                            ref T hiRef = ref Unsafe.Add(ref keys, hi);
                            Sort3(ref loRef, ref miRef, ref hiRef, comparer);
                            return;
                        }

                        InsertionSort(ref keys, ref values, lo, hi, comparer);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        HeapSort(ref keys, ref values, lo, hi, comparer);
                        return;
                    }
                    depthLimit--;

                    // We should never reach here, unless > 3 elements due to partition size
                    //ref var keysAtLo = ref Unsafe.Add(ref keys, lo);
                    //int p = PickPivotAndPartitionIntIndeces(ref keys, lo, hi, comparer);
                    //int p = PickPivotAndPartitionIntPtrIndeces(ref keys, lo, hi, comparer);
                    int p = PickPivotAndPartitionIntPtrByteOffsets(ref keys, ref values, lo, hi, comparer);
                    // Note we've already partitioned around the pivot and do not have to move the pivot again.
                    ref var afterPivot = ref Unsafe.Add(ref keys, p + 1);
                    IntroSort(ref afterPivot, ref values, hi - (p + 1), depthLimit, comparer);
                    hi = p - 1;
                }
            }

            private static int PickPivotAndPartitionIntIndeces<T, TComparer>(ref T keys, ref int values, int lo, int hi, in TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                // Compute median-of-three.  But also partition them, since we've done the comparison.

                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know 
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint. 
                //       Saves one subtraction per loop compared to 
                //       `int middle = lo + ((hi - lo) >> 1);`
                int middle = (int)(((uint)hi + (uint)lo) >> 1);

                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                ref T loRef = ref Unsafe.Add(ref keys, lo);
                ref T miRef = ref Unsafe.Add(ref keys, middle);
                ref T hiRef = ref Unsafe.Add(ref keys, hi);
                Sort3(ref loRef, ref miRef, ref hiRef, comparer);

                T pivot = miRef;

                int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.
                Swap(ref miRef, ref Unsafe.Add(ref keys, right));

                while (left < right)
                {
                    // TODO: Would be good to update local ref here
                    while (comparer.LessThan(Unsafe.Add(ref keys, ++left), pivot)) ;
                    // TODO: Would be good to update local ref here
                    while (comparer.LessThan(pivot, Unsafe.Add(ref keys, --right))) ;

                    if (left >= right)
                        break;

                    // Indeces cannot be equal here
                    Swap(ref keys, left, right);
                }
                // Put pivot in the right location.
                right = (hi - 1);
                if (left != right)
                {
                    Swap(ref keys, left, right);
                }
                return left;
            }

            private static int PickPivotAndPartitionIntPtrIndeces<T, TComparer>(ref T keys, ref int values, int lo, int hi, in TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                // Compute median-of-three.  But also partition them, since we've done the comparison.

                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know 
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint. 
                //       Saves one subtraction per loop compared to 
                //       `int middle = lo + ((hi - lo) >> 1);`
                var middle = new IntPtr((int)(((uint)hi + (uint)lo) >> 1));

                var low = new IntPtr(lo);
                var high = new IntPtr(hi);

                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                ref T loRef = ref Unsafe.Add(ref keys, low);
                ref T miRef = ref Unsafe.Add(ref keys, middle);
                ref T hiRef = ref Unsafe.Add(ref keys, high);
                Sort3(ref loRef, ref miRef, ref hiRef, comparer);

                T pivot = miRef;

                // Put pivot in the right location.
                IntPtr left = low;
                IntPtr right = high - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.
                Swap(ref miRef, ref Unsafe.Add(ref keys, right));

                while (left.LessThan(right))
                {
                    // TODO: Would be good to update local ref here
                    do
                    {
                        left += 1;
                    }
                    while (comparer.LessThan(Unsafe.Add(ref keys, left), pivot));
                    //while (comparer.Compare(pivot, Unsafe.Add(ref keys, left)) >= 0) ;
                    // TODO: Would be good to update local ref here
                    do
                    {
                        right -= 1;
                    }
                    while (comparer.LessThan(pivot, Unsafe.Add(ref keys, right)));

                    //if (left >= right)
                    //if (left.GreaterThanEqual(right))
                    if (right.LessThan(left))
                        break;

                    // Indeces cannot be equal here
                    Swap(ref keys, left, right);
                }
                // Put pivot in the right location.
                right = high - 1;
                if (left != right)
                {
                    Swap(ref keys, left, right);
                }
                return (int)left;
            }

            private static int PickPivotAndPartitionIntPtrByteOffsets<T, TComparer>(ref T keys, ref int values, int lo, int hi, in TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                // Compute median-of-three.  But also partition them, since we've done the comparison.

                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know 
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint. 
                //       Saves one subtraction per loop compared to 
                //       `int middle = lo + ((hi - lo) >> 1);`
                var middle = new IntPtr((int)(((uint)hi + (uint)lo) >> 1));
                var low = new IntPtr(lo);
                var high = new IntPtr(hi);

                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                ref T loRef = ref Unsafe.Add(ref keys, low);
                ref T miRef = ref Unsafe.Add(ref keys, middle);
                ref T hiRef = ref Unsafe.Add(ref keys, high);
                Sort3(ref loRef, ref miRef, ref hiRef, comparer);

                T pivot = miRef;

                // Put pivot in the right location.
                IntPtr left = low;
                IntPtr right = high - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.
                Swap(ref miRef, ref Unsafe.Add(ref keys, right));

                IntPtr leftBytes = left.Multiply(Unsafe.SizeOf<T>());
                IntPtr rightBytes = right.Multiply(Unsafe.SizeOf<T>());

                while (leftBytes.LessThan(rightBytes))
                {
                    // TODO: Would be good to update local ref here
                    do
                    {
                        leftBytes += Unsafe.SizeOf<T>();
                    }
                    while (comparer.LessThan(Unsafe.AddByteOffset(ref keys, leftBytes), pivot));
                    // TODO: Would be good to update local ref here
                    do
                    {
                        rightBytes -= Unsafe.SizeOf<T>();
                    }
                    while (comparer.LessThan(pivot, Unsafe.AddByteOffset(ref keys, rightBytes)));

                    if (leftBytes.GreaterThanEqual(rightBytes))
                        break;

                    // Indeces cannot be equal here
                    //Swap(ref keys, left, right);
                    Swap(ref Unsafe.AddByteOffset(ref keys, leftBytes), ref Unsafe.AddByteOffset(ref keys, rightBytes));
                }
                // Put pivot in the right location.
                //right = (hi - 1);
                rightBytes = new IntPtr(hi - 1).Multiply(Unsafe.SizeOf<T>());
                if (leftBytes != rightBytes)
                {
                    //Swap(ref keys, left, right);
                    Swap(ref Unsafe.AddByteOffset(ref keys, leftBytes), ref Unsafe.AddByteOffset(ref keys, rightBytes));
                }
                return (int)leftBytes.Divide(Unsafe.SizeOf<T>());
            }

            private static void HeapSort<T, TComparer>(ref T keys, ref int values, int lo, int hi, in TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                Debug.Assert(keys != null);
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                int n = hi - lo + 1;
                for (int i = n / 2; i >= 1; --i)
                {
                    DownHeap(ref keys, ref values, i, n, lo, comparer);
                }
                for (int i = n; i > 1; --i)
                {
                    Swap(ref keys, lo, lo + i - 1);
                    DownHeap(ref keys, ref values, 1, i - 1, lo, comparer);
                }
            }

            private static void DownHeap<T, TComparer>(ref T keys, ref int values, int i, int n, int lo, in TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                Debug.Assert(keys != null);
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);

                ////T d = keys[lo + i - 1];
                //T d = Unsafe.Add(ref keys, lo + i - 1);
                //int child;
                //while (i <= n / 2)
                //{
                //    child = 2 * i;
                //    //if (child < n && comparer(keys[lo + child - 1], keys[lo + child]) < 0)
                //    if (child < n && comparer.Compare(Unsafe.Add(ref keys, lo + child - 1),
                //        Unsafe.Add(ref keys, lo + child)) < 0)
                //    {
                //        child++;
                //    }
                //    //if (!(comparer(d, keys[lo + child - 1]) < 0))
                //    if (!(comparer.Compare(d, Unsafe.Add(ref keys, lo + child - 1)) < 0))
                //        break;
                //    // keys[lo + i - 1] = keys[lo + child - 1]
                //    Unsafe.Add(ref keys, lo + i - 1) = Unsafe.Add(ref keys, lo + child - 1);
                //    i = child;
                //}
                ////keys[lo + i - 1] = d;
                //Unsafe.Add(ref keys, lo + i - 1) = d;

                //T d = keys[lo + i - 1];
                ref T refLo = ref Unsafe.Add(ref keys, lo);
                ref T refLoMinus1 = ref Unsafe.Subtract(ref refLo, 1);
                T d = Unsafe.Add(ref refLoMinus1, i);
                var nHalf = n / 2;
                while (i <= nHalf)
                {
                    int child = i << 1;

                    //if (child < n && comparer(keys[lo + child - 1], keys[lo + child]) < 0)
                    if (child < n &&
                        comparer.LessThan(Unsafe.Add(ref refLoMinus1, child), Unsafe.Add(ref refLo, child)))
                    {
                        ++child;
                    }

                    //if (!(comparer(d, keys[lo + child - 1]) < 0))
                    if (!(comparer.LessThan(d, Unsafe.Add(ref refLoMinus1, child))))
                        break;

                    // keys[lo + i - 1] = keys[lo + child - 1]
                    Unsafe.Add(ref refLoMinus1, i) = Unsafe.Add(ref refLoMinus1, child);

                    i = child;
                }
                //keys[lo + i - 1] = d;
                Unsafe.Add(ref keys, lo + i - 1) = d;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InsertionSort<T, TComparer>(ref T keys, ref int values, int lo, int hi, in TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                Debug.Assert(keys != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi >= lo);

                for (int i = lo; i < hi; i++)
                {
                    //t = keys[i + 1];
                    var t = Unsafe.Add(ref keys, i + 1);
                    // Need local ref that can be updated!
                    int j = i;
                    while (j >= lo && comparer.LessThan(t, Unsafe.Add(ref keys, j)))
                    {
                        Unsafe.Add(ref keys, j + 1) = Unsafe.Add(ref keys, j);
                        --j;
                    }
                    Unsafe.Add(ref keys, j + 1) = t;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Sort3<T, TComparer>(ref T r0, ref T r1, ref T r2, in TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                //SwapIfGreater(ref r0, ref r1, comparer); // swap the low with the mid point
                //SwapIfGreater(ref r0, ref r2, comparer); // swap the low with the high
                //SwapIfGreater(ref r1, ref r2, comparer); // swap the middle with the high

                if (comparer.LessThan(r0, r1)) //r0 < r1)
                {
                    if (comparer.LessThan(r1, r2)) //(r1 < r2)
                    {
                        return;
                    }
                    else if (comparer.LessThan(r0, r2)) //(r0 < r2)
                    {
                        Swap(ref r1, ref r2);
                    }
                    else
                    {
                        T tmp = r0;
                        r0 = r2;
                        r2 = r1;
                        r1 = tmp;
                    }
                }
                else
                {
                    if (comparer.LessThan(r0, r2)) //(r0 < r2)
                    {
                        Swap(ref r0, ref r1);
                    }
                    else if (comparer.LessThan(r2, r1)) //(r2 < r1)
                    {
                        Swap(ref r0, ref r2);
                    }
                    else
                    {
                        T tmp = r0;
                        r0 = r1;
                        r1 = r2;
                        r2 = tmp;
                    }
                }
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void SwapIfGreater<T, TComparer>(ref T keys, int i, int j, TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                Debug.Assert(i != j);
                // Check moved to the one case actually needing it, not all!
                //if (i != j)
                {
                    ref var iElement = ref Unsafe.Add(ref keys, i);
                    ref var jElement = ref Unsafe.Add(ref keys, j);
                    SwapIfGreater(ref iElement, ref jElement, comparer);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void SwapIfGreater<T, TComparer>(ref T a, ref T b, in TComparer comparer)
                where TComparer : ILessThanComparer<T>
            {
                //if (comparer.Compare(a, b) > 0)
                if (comparer.LessThan(b, a))
                {
                    T temp = a;
                    a = b;
                    b = temp;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Swap<T>(ref T items, int i, int j)
            {
                // TODO: Is the i!=j check necessary? Most cases not needed?
                // Only in one case it seems, REFACTOR
                Debug.Assert(i != j);
                // No place needs this anymore
                //if (i != j)
                {
                    ref var iElement = ref Unsafe.Add(ref items, i);
                    ref var jElement = ref Unsafe.Add(ref items, j);
                    Swap(ref iElement, ref jElement);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Swap<T>(ref T items, IntPtr i, IntPtr j)
            {
                Debug.Assert(i != j);
                // No place needs this anymore
                //if (i != j)
                {
                    ref var iElement = ref Unsafe.Add(ref items, i);
                    ref var jElement = ref Unsafe.Add(ref items, j);
                    Swap(ref iElement, ref jElement);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Swap<T>(ref T a, ref T b)
            {
                T temp = a;
                a = b;
                b = temp;
            }
        }

        internal static class DefaultSpanSortHelper<T, TComparer>
            where TComparer : IComparer<T>
        {
            //private static volatile ISpanSortHelper<T, TComparer> defaultArraySortHelper;

            //public static ISpanSortHelper<T, TComparer> Default
            //{
            //    get
            //    {
            //        ISpanSortHelper<T, TComparer> sorter = defaultArraySortHelper;
            //        if (sorter == null)
            //            sorter = CreateArraySortHelper();

            //        return sorter;
            //    }
            //}
            internal static readonly ISpanSortHelper<T, TComparer> s_default = CreateSortHelper();

            private static ISpanSortHelper<T, TComparer> CreateSortHelper()
            {
                if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
                {
                    // TODO: Is there a faster way?
                    var ctor = typeof(ComparableSpanSortHelper<,>)
                        .MakeGenericType(new Type[] { typeof(T), typeof(TComparer) })
                        .GetConstructor(Array.Empty<Type>());

                    return (ISpanSortHelper<T, TComparer>)ctor.Invoke(Array.Empty<object>());
                    // coreclr does the following:
                    //return (IArraySortHelper<T, TComparer>)
                    //    RuntimeTypeHandle.Allocate(
                    //        .TypeHandle.Instantiate());
                }
                else
                {
                    return new SpanSortHelper<T, TComparer>();
                }
            }
        }

        internal class SpanSortHelper<T, TComparer> : ISpanSortHelper<T, TComparer>
            where TComparer : IComparer<T>
        {
            public void Sort(Span<T> keys, ref int values, in TComparer comparer)
            {
                // Add a try block here to detect IComparers (or their
                // underlying IComparables, etc) that are bogus.
                //
                // TODO: Do we need the try/catch? Just let it triggle up...
                //try
                //{
                    if (typeof(TComparer) == typeof(IComparer<T>) && comparer == null)
                    {
                        SpanSortHelper.Sort(
                            ref keys.DangerousGetPinnableReference(), ref values, keys.Length, 
                            new ComparerLessThanComparer<T, IComparer<T>>(Comparer<T>.Default));
                    }
                    else
                    {
                        SpanSortHelper.Sort(
                            ref keys.DangerousGetPinnableReference(), ref values, keys.Length,
                            new ComparerLessThanComparer<T, IComparer<T>>(comparer));
                    }
                //}
                //catch (IndexOutOfRangeException e)
                //{
                //    throw e;
                //    //IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
                //}
                //catch (Exception e)
                //{
                //    throw e;
                //    //throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
                //}
            }
        }

        internal class ComparableSpanSortHelper<T, TComparer>
            : ISpanSortHelper<T, TComparer>
            where T : IComparable<T>
            where TComparer : IComparer<T>
        {
            public void Sort(Span<T> keys, ref int values, in TComparer comparer)
            {
                // Add a try block here to detect IComparers (or their
                // underlying IComparables, etc) that are bogus.
                //
                // TODO: Do we need the try/catch? Just let it triggle up...
                //try
                //{
                    if (comparer == null ||
                        // Cache this in generic traits helper class perhaps
                        (!typeof(TComparer).IsValueType &&
                         object.ReferenceEquals(comparer, Comparer<T>.Default))) // Or "=="?
                    {
                        if (!SpanSortHelper.TrySortSpecialized(keys, ref values))
                        {
                            SpanSortHelper.Sort(
                                ref keys.DangerousGetPinnableReference(), ref values, keys.Length,
                                new ComparableLessThanComparer<T>());
                        }
                    }
                    else
                    {
                        SpanSortHelper.Sort(
                            ref keys.DangerousGetPinnableReference(), ref values, keys.Length,
                            new ComparerLessThanComparer<T, TComparer>(comparer));
                    }
                //}
                //catch (IndexOutOfRangeException e)
                //{
                //    throw e;
                //    //IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
                //}
                //catch (Exception e)
                //{
                //    throw e;
                //    //throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
                //}
            }
        }
    }
}
