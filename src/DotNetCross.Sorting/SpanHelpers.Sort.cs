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
        internal static void Sort<T>(this Span<T> keys)
        {
            // PERF: Try specialized here for optimal performance
            // Code-gen is weird unless used in loop outside
            if (!SpanSortHelper.TrySortSpecialized(
                ref keys.DangerousGetPinnableReference(), keys.Length))
            {
                Sort(keys, Comparer<T>.Default);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<T, TComparer>(
            this Span<T> keys, TComparer comparer)
            where TComparer : IComparer<T>
        {
            Span<SpanSortHelper.Void> values = default;
            DefaultSpanSortHelper<T, SpanSortHelper.Void, TComparer>.s_default.Sort(
                ref keys.DangerousGetPinnableReference(), 
                ref values.DangerousGetPinnableReference(), 
                keys.Length, comparer, new SpanSortHelper.KeysSortOps());
        }

        internal interface ILessThanComparer<T>
        {
            bool LessThan(T x, T y);
        }
        //
        // Type specific LessThanComparer(s) to ensure optimal code-gen
        //

        // Could this be done better by simply casting? and being generic?
        // This way we do not need special casing for TValue...
        internal struct GenericSByteLessThanComparer<T> : ILessThanComparer<T>
        {
            // Does this trick work for this as well?
            public bool LessThan(T x, T y) => ((sbyte)(object)x) < ((sbyte)(object)y);
        }

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
        internal interface ISpanSortHelper<TKey, TValue, TComparer>
            where TComparer : IComparer<TKey>
        {
            void Sort<TSortOps>(ref TKey keys, ref TValue values, int length, TComparer comparer, TSortOps ops)
                where TSortOps : SpanSortHelper.ISortOps;
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
        //internal ref struct Swapper<T> : ISwapper // ref structs can't inherit interfaces
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

            internal struct Void { }

            // Could move TKey generic type to the methods, would avoid the need to unfold specialized sort twice...
            internal interface ISortOps
            {
                bool SortValues { get; }

                void Swap<TKey, TValue>(ref TKey keys, ref TValue values, int i, int j);
                void Copy<TKey, TValue>(ref TKey keys, ref TValue values, int sourceIndex, int destinationIndex);
                void Write<TKey, TValue>(TKey key, TValue value, int index, ref TKey keys, ref TValue values);

                ref TKey Sort3<TKey, TValue, TLessThanComparer>(
                    ref TKey keys, ref TValue values, int lo, int mi, int hi,
                    TLessThanComparer comparer)
                    where TLessThanComparer : ILessThanComparer<TKey>;

                void InsertionSort<TKey, TValue, TComparer>(
                    ref TKey keys, ref TValue values, int lo, int hi,
                    TComparer comparer)
                    where TComparer : ILessThanComparer<TKey>;
            }
            internal struct KeysSortOps : ISortOps
            {
                public bool SortValues
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => false;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Swap<TKey, TValue>(ref TKey keys, ref TValue _, int i, int j)
                {
                    ref var a = ref Unsafe.Add(ref keys, i);
                    ref var b = ref Unsafe.Add(ref keys, j);
                    TKey temp = a;
                    a = b;
                    b = temp;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Copy<TKey, TValue>(ref TKey keys, ref TValue values, int sourceIndex, int destinationIndex)
                {
                    Unsafe.Add(ref keys, destinationIndex) = Unsafe.Add(ref keys, sourceIndex);
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Write<TKey, TValue>(TKey key, TValue value, int index, ref TKey keys, ref TValue values)
                {
                    Unsafe.Add(ref keys, index) = key;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public ref TKey Sort3<TKey, TValue, TLessThanComparer>(
                    ref TKey keys, ref TValue values, int lo, int mi, int hi,
                    TLessThanComparer comparer)
                    where TLessThanComparer : ILessThanComparer<TKey>
                {
                    ref var r0 = ref Unsafe.Add(ref keys, lo);
                    ref var r1 = ref Unsafe.Add(ref keys, mi);
                    ref var r2 = ref Unsafe.Add(ref keys, hi);
                    if (comparer.LessThan(r0, r1)) //r0 < r1)
                    {
                        if (comparer.LessThan(r1, r2)) //(r1 < r2)
                        {
                            return ref r1;
                        }
                        else if (comparer.LessThan(r0, r2)) //(r0 < r2)
                        {
                            SpanSortHelper.Swap(ref r1, ref r2);
                        }
                        else
                        {
                            TKey tmp = r0;
                            r0 = r2;
                            r2 = r1;
                            r1 = tmp;
                        }
                    }
                    else
                    {
                        if (comparer.LessThan(r0, r2)) //(r0 < r2)
                        {
                            SpanSortHelper.Swap(ref r0, ref r1);
                        }
                        else if (comparer.LessThan(r2, r1)) //(r2 < r1)
                        {
                            SpanSortHelper.Swap(ref r0, ref r2);
                        }
                        else
                        {
                            TKey tmp = r0;
                            r0 = r1;
                            r1 = r2;
                            r2 = tmp;
                        }
                    }
                    return ref r1;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void InsertionSort<TKey, TValue, TComparer>(
                    ref TKey keys, ref TValue values, int lo, int hi,
                    TComparer comparer)
                    where TComparer : ILessThanComparer<TKey>
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
            }
            internal struct KeysValuesSortOps : ISortOps
            {
                public bool SortValues
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => true;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Swap<TKey, TValue>(ref TKey keys, ref TValue values, int i, int j)
                {
                    ref var keyA = ref Unsafe.Add(ref keys, i);
                    ref var keyB = ref Unsafe.Add(ref keys, j);
                    TKey keyTemp = keyA;
                    keyA = keyB;
                    keyB = keyTemp;

                    ref var valueA = ref Unsafe.Add(ref values, i);
                    ref var valueB = ref Unsafe.Add(ref values, j);
                    TValue valueTemp = valueA;
                    valueA = valueB;
                    valueB = valueTemp;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Copy<TKey, TValue>(ref TKey keys, ref TValue values, int sourceIndex, int destinationIndex)
                {
                    Unsafe.Add(ref keys, destinationIndex) = Unsafe.Add(ref keys, sourceIndex);
                    Unsafe.Add(ref values, destinationIndex) = Unsafe.Add(ref values, sourceIndex);
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Write<TKey, TValue>(TKey key, TValue value, int index, ref TKey keys, ref TValue values)
                {
                    Unsafe.Add(ref keys, index) = key;
                    Unsafe.Add(ref values, index) = value;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public ref TKey Sort3<TKey, TValue, TLessThanComparer>(
                    ref TKey keys, ref TValue values, int lo, int mi, int hi,
                    TLessThanComparer comparer)
                    where TLessThanComparer : ILessThanComparer<TKey>
                {
                    throw new NotImplementedException();
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void InsertionSort<TKey, TValue, TComparer>(
                    ref TKey keys, ref TValue values, int lo, int hi,
                    TComparer comparer)
                    where TComparer : ILessThanComparer<TKey>
                {
                    throw new NotImplementedException();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool TrySortSpecialized<TKey>(
                ref TKey keys, int length)
            {
                Void values;
                return TrySortSpecialized(ref keys, ref values, length, 
                    new KeysSortOps());
            }

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //internal static bool TrySortSpecialized<TKey, TValue>(
            //    ref TKey keys, ref TValue values, int length)
            //{
            //    return TrySortSpecialized(ref keys, ref values, length,
            //        new KeysValuesSwapper());
            //}

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool TrySortSpecialized<TKey, TValue, TSortOps>(
                ref TKey keys, ref TValue values, int length, 
                TSortOps ops)
                where TSortOps : ISortOps
            {
                // Types unfolded adopted from https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp#L268
                if (typeof(TKey) == typeof(sbyte))
                {
                    ref var specificKeys = ref Unsafe.As<TKey, sbyte>(ref keys);
                    Sort(ref specificKeys, ref values, length, new SByteLessThanComparer(), ops);
                    return true;
                }
                else if (typeof(TKey) == typeof(byte) ||
                         typeof(TKey) == typeof(bool)) // Use byte for bools to reduce code size
                {
                    ref var specificKeys = ref Unsafe.As<TKey, byte>(ref keys);
                    Sort(ref specificKeys, ref values, length, new ByteLessThanComparer(), ops);

                    return true;
                }
                else if (typeof(TKey) == typeof(short) ||
                         typeof(TKey) == typeof(char)) // Use short for chars to reduce code size
                {
                    ref var specificKeys = ref Unsafe.As<TKey, short>(ref keys);
                    Sort(ref specificKeys, ref values, length, new Int16LessThanComparer(), ops);

                    return true;
                }
                else if (typeof(TKey) == typeof(ushort))
                {
                    ref var specificKeys = ref Unsafe.As<TKey, ushort>(ref keys);
                    Sort(ref specificKeys, ref values, length, new UInt16LessThanComparer(), ops);

                    return true;
                }
                else if (typeof(TKey) == typeof(int))
                {
                    ref var specificKeys = ref Unsafe.As<TKey, int>(ref keys);
                    Sort(ref specificKeys, ref values, length, new Int32LessThanComparer(), ops);

                    return true;
                }
                else if (typeof(TKey) == typeof(uint))
                {
                    ref var specificKeys = ref Unsafe.As<TKey, uint>(ref keys);
                    Sort(ref specificKeys, ref values, length, new UInt32LessThanComparer(), ops);

                    return true;
                }
                else if (typeof(TKey) == typeof(long))
                {
                    ref var specificKeys = ref Unsafe.As<TKey, long>(ref keys);
                    Sort(ref specificKeys, ref values, length, new Int64LessThanComparer(), ops);

                    return true;
                }
                else if (typeof(TKey) == typeof(ulong))
                {
                    ref var specificKeys = ref Unsafe.As<TKey, ulong>(ref keys);
                    Sort(ref specificKeys, ref values, length, new UInt64LessThanComparer(), ops);

                    return true;
                }
                else if (typeof(TKey) == typeof(float))
                {
                    ref var specificKeys = ref Unsafe.As<TKey, float>(ref keys);

                    // Comparison to NaN is always false, so do a linear pass 
                    // and swap all NaNs to the front of the array
                    var left = NaNPrepass(ref specificKeys, ref values, length, new SingleIsNaN());

                    ref var afterNaNsKeys = ref Unsafe.Add(ref specificKeys, left);
                    ref var afterNaNsValues = ref Unsafe.Add(ref values, left);
                    Sort(ref afterNaNsKeys, ref afterNaNsValues, length - left, new SingleLessThanComparer(), ops);

                    return true;
                }
                else if (typeof(TKey) == typeof(double))
                {
                    ref var specificKeys = ref Unsafe.As<TKey, double>(ref keys);

                    // Comparison to NaN is always false, so do a linear pass 
                    // and swap all NaNs to the front of the array
                    var left = NaNPrepass(ref specificKeys, ref values, length, new DoubleIsNaN());

                    ref var afterNaNsKeys = ref Unsafe.Add(ref specificKeys, left);
                    ref var afterNaNsValues = ref Unsafe.Add(ref values, left);
                    Sort(ref afterNaNsKeys, ref afterNaNsValues, length - left, new DoubleLessThanComparer(), ops);

                    return true;
                }
                else
                {
                    return false;
                }
            }

            // For sorting, move all NaN instances to front of the input array
            private static int NaNPrepass<TKey, TValue, TIsNaN>(
                ref TKey keys, ref TValue values, int length, 
                TIsNaN isNaN)
                where TIsNaN : struct, IIsNaN<TKey>
            {
                int left = 0;
                for (int i = 0; i <= length; i++)
                {
                    ref TKey current = ref Unsafe.Add(ref keys, i);
                    if (isNaN.IsNaN(current))
                    {
                        ref TKey previous = ref Unsafe.Add(ref keys, left);

                        Swap(ref previous, ref current);

                        ++left;
                    }
                }
                return left;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Sort<TKey, TValue, TComparer, TSortOps>(
                ref TKey keys, ref TValue values, int length, 
                TComparer comparer, TSortOps ops)
                where TComparer : ILessThanComparer<TKey>
                where TSortOps : ISortOps
            {
                if (length < 2)
                    return;

                IntrospectiveSort(ref keys, ref values, length, comparer, ops);
            }

            private static void IntrospectiveSort<TKey, TValue, TComparer, TSortOps>(
                ref TKey keys, ref TValue values, int length, 
                TComparer comparer, TSortOps ops)
                where TComparer : ILessThanComparer<TKey>
                where TSortOps : ISortOps
            {
                // Note how old used the full length of keys array to limit, seems like a bug!
                //IntroSort(keys, left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2PlusOne(keys.Length), comparer);
                // In native code this is done right, so only for when using managed code:
                // https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.h#L139
                var depthLimit = 2 * FloorLog2PlusOne(length);
                IntroSort(ref keys, ref values, 0, length - 1, depthLimit, comparer, ops);
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

            private static void IntroSort<TKey, TValue, TComparer, TSortOps>(
                ref TKey keys, ref TValue values, 
                int lo, int hi, int depthLimit, 
                TComparer comparer, TSortOps ops)
                where TComparer : ILessThanComparer<TKey>
                where TSortOps : ISortOps
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
                            ops.Sort3(ref keys, ref values, lo, hi - 1, hi, comparer);
                            //ref TKey loRef = ref Unsafe.Add(ref keys, lo);
                            //ref TKey miRef = ref Unsafe.Add(ref keys, hi - 1);
                            //ref TKey hiRef = ref Unsafe.Add(ref keys, hi);
                            ////ref T miRef = ref Unsafe.SubtractByteOffset(ref hiRef, new IntPtr(Unsafe.SizeOf<T>()));
                            //Sort3(ref loRef, ref miRef, ref hiRef, comparer, ops);
                            return;
                        }

                        ops.InsertionSort(ref keys, ref values, lo, hi, comparer);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        HeapSort(ref keys, ref values, lo, hi, comparer, ops);
                        return;
                    }
                    depthLimit--;

                    // We should never reach here, unless > 3 elements due to partition size
                    int p = PickPivotAndPartitionIntIndeces(ref keys, ref values, lo, hi, comparer, ops);
                    //int p = PickPivotAndPartitionIntPtrIndeces(ref keys, lo, hi, comparer);
                    //int p = PickPivotAndPartitionIntPtrByteOffsets(ref keys, ref values, lo, hi, comparer);
                    // Note we've already partitioned around the pivot and do not have to move the pivot again.
                    IntroSort(ref keys, ref values, p + 1, hi, depthLimit, comparer, ops);
                    hi = p - 1;
                }
            }

            private static void IntroSort<TKey, TValue, TComparer, TSortOps>(
                ref TKey keys, ref TValue values, int hi, int depthLimit, 
                TComparer comparer, TSortOps ops)
                where TComparer : ILessThanComparer<TKey>
                where TSortOps : ISortOps
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
                            ops.Sort3(ref keys, ref values, lo, hi - 1, hi, comparer);
                            //ref TKey loRef = ref Unsafe.Add(ref keys, lo);
                            //ref TKey miRef = ref Unsafe.Add(ref keys, hi - 1);
                            //ref TKey hiRef = ref Unsafe.Add(ref keys, hi);
                            //Sort3(ref loRef, ref miRef, ref hiRef, comparer);
                            return;
                        }

                        InsertionSort(ref keys, ref values, lo, hi, comparer, ops);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        HeapSort(ref keys, ref values, lo, hi, comparer, ops);
                        return;
                    }
                    depthLimit--;

                    // We should never reach here, unless > 3 elements due to partition size
                    //ref var keysAtLo = ref Unsafe.Add(ref keys, lo);
                    int p = PickPivotAndPartitionIntIndeces(ref keys, ref values, lo, hi, comparer, ops);
                    //int p = PickPivotAndPartitionIntPtrIndeces(ref keys, lo, hi, comparer);
                    //int p = PickPivotAndPartitionIntPtrByteOffsets(ref keys, ref values, lo, hi, comparer);
                    // Note we've already partitioned around the pivot and do not have to move the pivot again.
                    ref var keysAfterPivot = ref Unsafe.Add(ref keys, p + 1);
                    ref var valuesAfterPivot = ref Unsafe.Add(ref values, p + 1);
                    IntroSort(ref keysAfterPivot, ref valuesAfterPivot, hi - (p + 1), depthLimit, comparer, ops);
                    hi = p - 1;
                }
            }

            private static int PickPivotAndPartitionIntIndeces<TKey, TValue, TComparer, TSortOps>(
                ref TKey keys, ref TValue values, int lo, int hi, 
                TComparer comparer, TSortOps ops)
                where TComparer : ILessThanComparer<TKey>
                where TSortOps : ISortOps
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
                ops.Sort3(ref keys, ref values, lo, middle, hi, comparer);
                //ref TKey loRef = ref Unsafe.Add(ref keys, lo);
                //ref TKey miRef = ref Unsafe.Add(ref keys, middle);
                //ref TKey hiRef = ref Unsafe.Add(ref keys, hi);
                //Sort3(ref loRef, ref miRef, ref hiRef, comparer);
                //TKey pivot = miRef;

                ref TKey miRef = ref Unsafe.Add(ref keys, middle);
                TKey pivot = miRef;

                int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.
                //Swap(ref miRef, ref Unsafe.Add(ref keys, right));
                ops.Swap(ref keys, ref values, middle, right);

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

            private static void HeapSort<TKey, TValue, TComparer, TSortOps>(
                ref TKey keys, ref TValue values, int lo, int hi, 
                TComparer comparer, TSortOps ops)
                where TComparer : ILessThanComparer<TKey>
                where TSortOps : ISortOps
            {
                Debug.Assert(keys != null);
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                int n = hi - lo + 1;
                for (int i = n / 2; i >= 1; --i)
                {
                    DownHeap(ref keys, ref values, i, n, lo, comparer, ops);
                }
                for (int i = n; i > 1; --i)
                {
                    ops.Swap(ref keys, ref values, lo, lo + i - 1);
                    DownHeap(ref keys, ref values, 1, i - 1, lo, comparer, ops);
                }
            }

            private static void DownHeap<TKey, TValue, TComparer, TSortOps>(
                ref TKey keys, ref TValue values, int i, int n, int lo, 
                TComparer comparer, TSortOps ops)
                where TComparer : ILessThanComparer<TKey>
                where TSortOps : ISortOps
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
                ref TKey refLo = ref Unsafe.Add(ref keys, lo);
                ref TKey refLoMinus1 = ref Unsafe.Subtract(ref refLo, 1);
                TKey d = Unsafe.Add(ref refLoMinus1, i);
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
            private static void InsertionSort<TKey, TValue, TComparer, TSortOps>(
                ref TKey keys, ref TValue values, int lo, int hi, 
                TComparer comparer, TSortOps ops)
                where TComparer : ILessThanComparer<TKey>
                where TSortOps : ISortOps
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
            internal static void Sort3<TKey, TValue, TComparer, TSortOps>(
                ref TKey r0, ref TKey r1, ref TKey r2, 
                TComparer comparer, TSortOps ops)
                where TComparer : ILessThanComparer<TKey>
                where TSortOps : ISortOps
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
                        TKey tmp = r0;
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
                        TKey tmp = r0;
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

        internal static class DefaultSpanSortHelper<TKey, TValue, TComparer>
            where TComparer : IComparer<TKey>
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
            internal static readonly ISpanSortHelper<TKey, TValue, TComparer> s_default = CreateSortHelper();

            private static ISpanSortHelper<TKey, TValue, TComparer> CreateSortHelper()
            {
                if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
                {
                    // TODO: Is there a faster way?
                    var ctor = typeof(ComparableSpanSortHelper<,,>)
                        .MakeGenericType(new Type[] { typeof(TKey), typeof(TValue), typeof(TComparer) })
                        .GetConstructor(Array.Empty<Type>());

                    return (ISpanSortHelper<TKey, TValue, TComparer>)ctor.Invoke(Array.Empty<object>());
                    // coreclr does the following:
                    //return (IArraySortHelper<T, TComparer>)
                    //    RuntimeTypeHandle.Allocate(
                    //        .TypeHandle.Instantiate());
                }
                else
                {
                    return new SpanSortHelper<TKey, TValue, TComparer>();
                }
            }
        }

        internal class SpanSortHelper<TKey, TValue, TComparer> : ISpanSortHelper<TKey, TValue, TComparer>
            where TComparer : IComparer<TKey>
        {
            public void Sort<TSortOps>(ref TKey keys, ref TValue values, int length, TComparer comparer, TSortOps ops)
                where TSortOps : SpanSortHelper.ISortOps
            {
                // Add a try block here to detect IComparers (or their
                // underlying IComparables, etc) that are bogus.
                //
                // TODO: Do we need the try/catch? Just let it triggle up...
                //try
                //{
                    if (typeof(TComparer) == typeof(IComparer<TKey>) && comparer == null)
                    {
                        SpanSortHelper.Sort(
                            ref keys, ref values, length, 
                            new ComparerLessThanComparer<TKey, IComparer<TKey>>(Comparer<TKey>.Default),
                            ops);
                    }
                    else
                    {
                        SpanSortHelper.Sort(
                            ref keys, ref values, length,
                            new ComparerLessThanComparer<TKey, IComparer<TKey>>(comparer),
                            ops);
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

        internal class ComparableSpanSortHelper<TKey, TValue, TComparer>
            : ISpanSortHelper<TKey, TValue, TComparer>
            where TKey : IComparable<TKey>
            where TComparer : IComparer<TKey>
        {
            public void Sort<TSortOps>(ref TKey keys, ref TValue values, int length, 
                TComparer comparer, TSortOps ops)
                where TSortOps : SpanSortHelper.ISortOps
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
                         object.ReferenceEquals(comparer, Comparer<TKey>.Default))) // Or "=="?
                    {
                        if (!SpanSortHelper.TrySortSpecialized(ref keys, ref values, length, ops))
                        {
                            SpanSortHelper.Sort(
                                ref keys, ref values, length,
                                new ComparableLessThanComparer<TKey>(),
                                ops);
                        }
                    }
                    else
                    {
                        SpanSortHelper.Sort(
                            ref keys, ref values, length,
                            new ComparerLessThanComparer<TKey, TComparer>(comparer),
                            ops);
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
