﻿//#define USE_NATIVE_INTS
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

#if USE_NATIVE_INTS
using nint = System.IntPtr;
using nuint = System.UIntPtr;
#else
using nint = System.Int32;
using nuint = System.UInt32;
#endif
namespace System
{
    internal static class Int32Helper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool GreaterThan(this int a, int b)
        {
            return a > b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool GreaterThanEqual(this int a, int b)
        {
            return a >= b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool LessThan(this int a, int b)
        {
            return a < b;
        }
    }
}

namespace System
{
    internal static class SpanSortHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<TKey>(this Span<TKey> keys)
        {
            // PERF: Try specialized here for optimal performance
            // Code-gen is weird unless used in loop outside
            if (!TrySortSpecialized(
                ref keys.DangerousGetPinnableReference(), keys.Length))
            {
                Span<Void> values = default;
                DefaultSpanSortHelper<TKey, Void>.s_default.Sort(
                    ref keys.DangerousGetPinnableReference(),
                    ref values.DangerousGetPinnableReference(),
                    keys.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<TKey, TComparer>(
            this Span<TKey> keys, TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            Span<Void> values = default;
            DefaultSpanSortHelper<TKey, Void, TComparer>.s_default.Sort(
                ref keys.DangerousGetPinnableReference(),
                ref values.DangerousGetPinnableReference(),
                keys.Length, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<TKey, TValue>(this Span<TKey> keys, Span<TValue> values)
        {
            if (keys.Length != values.Length)
                ThrowHelper.ThrowArgumentException_ItemsMustHaveSameLength();

            // PERF: Try specialized here for optimal performance
            // Code-gen is weird unless used in loop outside
            if (!TrySortSpecializedWithValues(
                ref keys.DangerousGetPinnableReference(), 
                ref values.DangerousGetPinnableReference(), 
                keys.Length))
            {
                Sort(keys, values, Comparer<TKey>.Default);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<TKey, TValue, TComparer>(
            this Span<TKey> keys, Span<TValue> values, TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            DefaultSpanSortHelper<TKey, TValue, TComparer>.s_default.Sort(
                ref keys.DangerousGetPinnableReference(),
                ref values.DangerousGetPinnableReference(),
                keys.Length, comparer);
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(sbyte x, sbyte y) => x < y;
        }
        internal struct ByteLessThanComparer : ILessThanComparer<byte>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(byte x, byte y) => x < y;
        }
        internal struct Int16LessThanComparer : ILessThanComparer<short>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(short x, short y) => x < y;
        }
        internal struct UInt16LessThanComparer : ILessThanComparer<ushort>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(ushort x, ushort y) => x < y;
        }
        internal struct Int32LessThanComparer : ILessThanComparer<int>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(int x, int y) => x < y;
        }
        internal struct UInt32LessThanComparer : ILessThanComparer<uint>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(uint x, uint y) => x < y;
        }
        internal struct Int64LessThanComparer : ILessThanComparer<long>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(long x, long y) => x < y;
        }
        internal struct UInt64LessThanComparer : ILessThanComparer<ulong>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(ulong x, ulong y) => x < y;
        }
        internal struct SingleLessThanComparer : ILessThanComparer<float>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(float x, float y) => x < y;
        }
        internal struct DoubleLessThanComparer : ILessThanComparer<double>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        }


        internal interface IIsNaN<T>
        {
            bool IsNaN(T value);
        }
        internal struct SingleIsNaN : IIsNaN<float>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsNaN(float value) => float.IsNaN(value);
        }
        internal struct DoubleIsNaN : IIsNaN<double>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsNaN(double value) => double.IsNaN(value);
        }

        // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
        // https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp

        // This is the threshold where Introspective sort switches to Insertion sort.
        // Empirically, 16 seems to speed up most cases without slowing down others, at least for integers.
        // Large value types may benefit from a smaller number.
        internal const int IntrosortSizeThreshold = 16;

        internal struct Void { }

        // Code-gen was poor when using this...
        //internal static class SortValues<T>
        //{
        //    public static readonly bool Yes = typeof(T) != typeof(Void);
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySortSpecialized<TKey>(
            ref TKey keys, int length)
        {
            Void values;
            return TrySortSpecialized(ref keys, ref values, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySortSpecializedWithValues<TKey, TValue>(
            ref TKey keys, ref TValue values, int length)
        {
            return TrySortSpecialized(ref keys, ref values, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySortSpecialized<TKey, TValue>(
            ref TKey keys, ref TValue values, int length)
        {
            // Types unfolded adopted from https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp#L268
            if (typeof(TKey) == typeof(sbyte))
            {
                ref var specificKeys = ref Unsafe.As<TKey, sbyte>(ref keys);
                Sort(ref specificKeys, ref values, length, new SByteLessThanComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(byte) ||
                     typeof(TKey) == typeof(bool)) // Use byte for bools to reduce code size
            {
                ref var specificKeys = ref Unsafe.As<TKey, byte>(ref keys);
                Sort(ref specificKeys, ref values, length, new ByteLessThanComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(short) ||
                     typeof(TKey) == typeof(char)) // Use short for chars to reduce code size
            {
                ref var specificKeys = ref Unsafe.As<TKey, short>(ref keys);
                Sort(ref specificKeys, ref values, length, new Int16LessThanComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(ushort))
            {
                ref var specificKeys = ref Unsafe.As<TKey, ushort>(ref keys);
                Sort(ref specificKeys, ref values, length, new UInt16LessThanComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(int))
            {
                ref var specificKeys = ref Unsafe.As<TKey, int>(ref keys);
                Sort(ref specificKeys, ref values, length, new Int32LessThanComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(uint))
            {
                ref var specificKeys = ref Unsafe.As<TKey, uint>(ref keys);
                Sort(ref specificKeys, ref values, length, new UInt32LessThanComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(long))
            {
                ref var specificKeys = ref Unsafe.As<TKey, long>(ref keys);
                Sort(ref specificKeys, ref values, length, new Int64LessThanComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(ulong))
            {
                ref var specificKeys = ref Unsafe.As<TKey, ulong>(ref keys);
                Sort(ref specificKeys, ref values, length, new UInt64LessThanComparer());
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
                Sort(ref afterNaNsKeys, ref afterNaNsValues, length - left, new SingleLessThanComparer());

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
                Sort(ref afterNaNsKeys, ref afterNaNsValues, length - left, new DoubleLessThanComparer());

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
        internal static void Sort<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values, int length,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            if (length < 2)
                return;

            IntrospectiveSort(ref keys, ref values, length, comparer);
        }

        private static void IntrospectiveSort<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values, int length,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            // Note how old used the full length of keys array to limit, seems like a bug.
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

        private static void IntroSort<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values,
            int lo, int hi, int depthLimit,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
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
                        Sort2(ref keys, ref values, lo, hi, comparer);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        //ops.Sort3(ref keys, ref values, lo, hi - 1, hi, comparer);
                        Sort3(ref keys, ref values, lo, hi - 1, hi, comparer);
                        return;
                    }

                    InsertionSort(ref keys, ref values, (nint)lo, (nint)hi, comparer);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(ref keys, ref values, lo, hi, comparer);
                    return;
                }
                depthLimit--;

                // We should never reach here, unless > 3 elements due to partition size
                int p = PickPivotAndPartition(ref keys, ref values, lo, hi, comparer);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(ref keys, ref values, p + 1, hi, depthLimit, comparer);
                hi = p - 1;
            }
        }

        private static int PickPivotAndPartition<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values, int lo, int hi,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
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
            //TKey pivot = ops.Sort3(ref keys, ref values, lo, middle, hi, comparer);
            //TKey pivot = Sort3(ref keys, ref values, lo, middle, hi, comparer);
            ref TKey miRef = ref Sort3(ref keys, ref values, lo, middle, hi, comparer);
            TKey pivot = miRef;

            nint mid = (nint)middle;
            nint left = (nint)lo, right = (nint)(hi - 1);  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.
            Swap(ref miRef, ref Unsafe.Add(ref keys, right));
            if (typeof(TValue) != typeof(Void))
            {
                Swap(ref values, mid, right);
            }

            while (left.LessThan(right))// < right)
            {
                do
                {
                    left += 1;
                }
                while (comparer.LessThan(Unsafe.Add(ref keys, left), pivot)) ;

                do
                {
                    right -= 1;
                }
                while (comparer.LessThan(pivot, Unsafe.Add(ref keys, right))) ;

                if (!left.LessThan(right)) //left >= right)
                    break;

                Swap(ref keys, left, right);
                if (typeof(TValue) != typeof(Void))
                {
                    Swap(ref values, left, right);
                }
            }
            // Put pivot in the right location.
            right = (nint)(hi - 1);
            if (left != right)
            {
                Swap(ref keys, left, right);
                if (typeof(TValue) != typeof(Void))
                {
                    Swap(ref values, left, right);
                }
            }
            return (int)left;
        }

        private static void HeapSort<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values, int lo, int hi,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
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
                if (typeof(TValue) != typeof(Void))
                {
                    Swap(ref values, lo, lo + i - 1);
                }
                DownHeap(ref keys, ref values, 1, i - 1, lo, comparer);
            }
        }

        private static void DownHeap<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values, int i, int n, int lo,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            Debug.Assert(keys != null);
            Debug.Assert(comparer != null);
            Debug.Assert(lo >= 0);

            //T d = keys[lo + i - 1];
            ref TKey keysAtLo = ref Unsafe.Add(ref keys, lo);
            ref TKey keysAtLoMinus1 = ref Unsafe.Subtract(ref keysAtLo, 1);

            ref TValue valuesAtLoMinus1 = ref typeof(TValue) != typeof(Void) ? ref Unsafe.Add(ref values, lo - 1) : ref values;

            TKey d = Unsafe.Add(ref keysAtLoMinus1, i);
            TValue dValue = typeof(TValue) != typeof(Void) ? Unsafe.Add(ref valuesAtLoMinus1, i) : default;

            var nHalf = n / 2;
            while (i <= nHalf)
            {
                int child = i << 1;

                //if (child < n && comparer(keys[lo + child - 1], keys[lo + child]) < 0)
                if (child < n &&
                    comparer.LessThan(Unsafe.Add(ref keysAtLoMinus1, child), Unsafe.Add(ref keysAtLo, child)))
                {
                    ++child;
                }

                //if (!(comparer(d, keys[lo + child - 1]) < 0))
                if (!(comparer.LessThan(d, Unsafe.Add(ref keysAtLoMinus1, child))))
                    break;

                // keys[lo + i - 1] = keys[lo + child - 1]
                Unsafe.Add(ref keysAtLoMinus1, i) = Unsafe.Add(ref keysAtLoMinus1, child);
                if (typeof(TValue) != typeof(Void))
                {
                    Unsafe.Add(ref valuesAtLoMinus1, i) = Unsafe.Add(ref valuesAtLoMinus1, child);
                }

                i = child;
            }
            //keys[lo + i - 1] = d;
            Unsafe.Add(ref keysAtLoMinus1, i) = d;
            if (typeof(TValue) != typeof(Void))
            {
                Unsafe.Add(ref values, lo + i - 1) = dValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InsertionSort<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values, nint lo, nint hi,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            //Debug.Assert(keys != null);
            //Debug.Assert(lo >= 0);
            //Debug.Assert(hi >= lo);

            for (nint i = lo; i.LessThan(hi); i += 1)
            {
                nint j = i;
                //t = keys[i + 1];
                var t = Unsafe.Add(ref keys, j + 1);
                // Need local ref that can be updated
                if (j.GreaterThanEqual(lo) && comparer.LessThan(t, Unsafe.Add(ref keys, j)))
                {
                    var v = typeof(TValue) != typeof(Void) ? Unsafe.Add(ref values, j + 1) : default;
                    do
                    {
                        Unsafe.Add(ref keys, j + 1) = Unsafe.Add(ref keys, j);
                        if (typeof(TValue) != typeof(Void))
                        {
                            Unsafe.Add(ref values, j + 1) = Unsafe.Add(ref values, j);
                        }
                        j -= 1;
                    }
                    while (j.GreaterThanEqual(lo) && comparer.LessThan(t, Unsafe.Add(ref keys, j)));

                    Unsafe.Add(ref keys, j + 1) = t;
                    if (typeof(TValue) != typeof(Void))
                    {
                        Unsafe.Add(ref values, j + 1) = v;
                    }
                }
            }
            //for (int i = lo; i < hi; i++)
            //{
            //    //t = keys[i + 1];
            //    var t = Unsafe.Add(ref keys, i + 1);
            //    var v = typeof(TValue) != typeof(Void) ? Unsafe.Add(ref values, i + 1) : default;
            //    // Need local ref that can be updated
            //    int j = i;
            //    while (j >= lo && comparer.LessThan(t, Unsafe.Add(ref keys, j)))
            //    {
            //        Unsafe.Add(ref keys, j + 1) = Unsafe.Add(ref keys, j);
            //        if (typeof(TValue) != typeof(Void))
            //        {
            //            Unsafe.Add(ref values, j + 1) = Unsafe.Add(ref values, j);
            //        }
            //        --j;
            //    }
            //    Unsafe.Add(ref keys, j + 1) = t;
            //    if (typeof(TValue) != typeof(Void))
            //    {
            //        Unsafe.Add(ref values, j + 1) = v;
            //    }
            //}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Sort2<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values, int i, int j,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            Debug.Assert(i != j);
            {
                ref var a = ref Unsafe.Add(ref keys, i);
                ref var b = ref Unsafe.Add(ref keys, j);
                if (comparer.LessThan(b, a))
                {
                    Swap(ref a, ref b);
                    if (typeof(TValue) != typeof(Void))
                    {
                        Swap(ref values, i, j);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref TKey Sort3<TKey, TValue, TLessThanComparer>(
            ref TKey keys, ref TValue values, int i0, int i1, int i2,
            TLessThanComparer comparer)
            where TLessThanComparer : ILessThanComparer<TKey>
        {
            ref var r0 = ref Unsafe.Add(ref keys, i0);
            ref var r1 = ref Unsafe.Add(ref keys, i1);
            ref var r2 = ref Unsafe.Add(ref keys, i2);

            if (comparer.LessThan(r0, r1)) //r0 < r1)
            {
                if (comparer.LessThan(r1, r2)) //(r1 < r2)
                {
                    return ref r1;
                }
                else if (comparer.LessThan(r0, r2)) //(r0 < r2)
                {
                    Swap(ref r1, ref r2);
                    if (typeof(TValue) != typeof(Void))
                    {
                        ref var v1 = ref Unsafe.Add(ref values, i1);
                        ref var v2 = ref Unsafe.Add(ref values, i2);
                        Swap(ref v1, ref v2);
                    }
                }
                else
                {
                    TKey tmp = r0;
                    r0 = r2;
                    r2 = r1;
                    r1 = tmp;
                    if (typeof(TValue) != typeof(Void))
                    {
                        ref var v0 = ref Unsafe.Add(ref values, i0);
                        ref var v1 = ref Unsafe.Add(ref values, i1);
                        ref var v2 = ref Unsafe.Add(ref values, i2);
                        TValue vTemp = v0;
                        v0 = v2;
                        v2 = v1;
                        v1 = vTemp;
                    }
                }
            }
            else
            {
                if (comparer.LessThan(r0, r2)) //(r0 < r2)
                {
                    Swap(ref r0, ref r1);
                    if (typeof(TValue) != typeof(Void))
                    {
                        ref var v0 = ref Unsafe.Add(ref values, i0);
                        ref var v1 = ref Unsafe.Add(ref values, i1);
                        Swap(ref v0, ref v1);
                    }
                }
                else if (comparer.LessThan(r2, r1)) //(r2 < r1)
                {
                    Swap(ref r0, ref r2);
                    if (typeof(TValue) != typeof(Void))
                    {
                        ref var v0 = ref Unsafe.Add(ref values, i0);
                        ref var v2 = ref Unsafe.Add(ref values, i2);
                        Swap(ref v0, ref v2);
                    }
                }
                else
                {
                    TKey tmp = r0;
                    r0 = r1;
                    r1 = r2;
                    r2 = tmp;
                    if (typeof(TValue) != typeof(Void))
                    {
                        ref var v0 = ref Unsafe.Add(ref values, i0);
                        ref var v1 = ref Unsafe.Add(ref values, i1);
                        ref var v2 = ref Unsafe.Add(ref values, i2);
                        TValue vTemp = v0;
                        v0 = v1;
                        v1 = v2;
                        v2 = vTemp;
                    }
                }
            }
            return ref r1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<T>(ref T items, int i, int j)
        {
            Debug.Assert(i != j);
            Swap(ref Unsafe.Add(ref items, i), ref Unsafe.Add(ref items, j));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<T>(ref T items, IntPtr i, IntPtr j)
        {
            Debug.Assert(i != j);
            Swap(ref Unsafe.Add(ref items, i), ref Unsafe.Add(ref items, j));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }


    internal static class DefaultSpanSortHelper<TKey, TValue>
    {
        internal static readonly ISpanSortHelper<TKey, TValue> s_default = CreateSortHelper();

        private static ISpanSortHelper<TKey, TValue> CreateSortHelper()
        {
            if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
            {
                // TODO: Is there a faster way? A way without heap alloc? 
                // Albeit, this only happens once for each type combination
                var ctor = typeof(ComparableSpanSortHelper<,>)
                    .MakeGenericType(new Type[] { typeof(TKey), typeof(TValue) })
                    .GetConstructor(Array.Empty<Type>());

                return (ISpanSortHelper<TKey, TValue>)ctor.Invoke(Array.Empty<object>());
                // coreclr does the following:
                //return (IArraySortHelper<T, TComparer>)
                //    RuntimeTypeHandle.Allocate(
                //        .TypeHandle.Instantiate());
            }
            else
            {
                return new SpanSortHelper<TKey, TValue>();
            }
        }
    }

    internal interface ISpanSortHelper<TKey, TValue>
    {
        void Sort(ref TKey keys, ref TValue values, int length);
    }

    internal class SpanSortHelper<TKey, TValue> : ISpanSortHelper<TKey, TValue>
    {
        public void Sort(ref TKey keys, ref TValue values, int length)
        {
            SpanSortHelpers.Sort(
                ref keys, ref values, length,
                new SpanSortHelpers.ComparerLessThanComparer<TKey, IComparer<TKey>>(Comparer<TKey>.Default));
        }
    }

    internal class ComparableSpanSortHelper<TKey, TValue>
        : ISpanSortHelper<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        public void Sort(ref TKey keys, ref TValue values, int length)
        {
            SpanSortHelpers.Sort(
                ref keys, ref values, length,
                new SpanSortHelpers.ComparableLessThanComparer<TKey>());
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
                // TODO: Is there a faster way? A way without heap alloc? 
                // Albeit, this only happens once for each type combination
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

    // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
    internal interface ISpanSortHelper<TKey, TValue, TComparer>
        where TComparer : IComparer<TKey>
    {
        void Sort(ref TKey keys, ref TValue values, int length, TComparer comparer);
    }

    internal class SpanSortHelper<TKey, TValue, TComparer> : ISpanSortHelper<TKey, TValue, TComparer>
        where TComparer : IComparer<TKey>
    {
        public void Sort(ref TKey keys, ref TValue values, int length, TComparer comparer)
        {
            // Add a try block here to detect IComparers (or their
            // underlying IComparables, etc) that are bogus.
            //
            // TODO: Do we need the try/catch?
            //try
            //{
            if (typeof(TComparer) == typeof(IComparer<TKey>) && comparer == null)
            {
                SpanSortHelpers.Sort(
                    ref keys, ref values, length,
                    new SpanSortHelpers.ComparerLessThanComparer<TKey, IComparer<TKey>>(Comparer<TKey>.Default));
            }
            else
            {
                SpanSortHelpers.Sort(
                    ref keys, ref values, length,
                    new SpanSortHelpers.ComparerLessThanComparer<TKey, IComparer<TKey>>(comparer));
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
        public void Sort(ref TKey keys, ref TValue values, int length,
            TComparer comparer)
        {
            // Add a try block here to detect IComparers (or their
            // underlying IComparables, etc) that are bogus.
            //
            // TODO: Do we need the try/catch?
            //try
            //{
            if (comparer == null ||
                // Cache this in generic traits helper class perhaps
                (!typeof(TComparer).IsValueType &&
                 object.ReferenceEquals(comparer, Comparer<TKey>.Default))) // Or "=="?
            {
                if (!SpanSortHelpers.TrySortSpecialized(ref keys, ref values, length))
                {
                    SpanSortHelpers.Sort(
                        ref keys, ref values, length,
                        new SpanSortHelpers.ComparableLessThanComparer<TKey>());
                }
            }
            else
            {
                SpanSortHelpers.Sort(
                    ref keys, ref values, length,
                    new SpanSortHelpers.ComparerLessThanComparer<TKey, TComparer>(comparer));
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
