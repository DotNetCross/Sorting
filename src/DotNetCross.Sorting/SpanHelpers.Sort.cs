// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

using static System.SpanSortHelpersHelperTypes;
using S = System.SpanSortHelpersKeysOnly;

namespace System
{
    internal static partial class SpanSortHelpersKeysOnly
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<TKey>(this Span<TKey> keys)
        {
            Span<int> values = default;
            // PERF: Try specialized here for optimal performance
            // Code-gen is weird unless used in loop outside
            if (!TrySortSpecialized(
                ref keys.DangerousGetPinnableReference(), ref values.DangerousGetPinnableReference(), keys.Length))
            {
                Sort(keys, Comparer<TKey>.Default);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<TKey, TComparer>(
            this Span<TKey> keys, TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            Span<int> values = default;
            DefaultSpanSortHelper<TKey, TComparer>.s_default.Sort(
                keys, 
                ref values.DangerousGetPinnableReference(), 
                comparer);
        }

        // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
        // https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp

        // This is the threshold where Introspective sort switches to Insertion sort.
        // Empirically, 16 seems to speed up most cases without slowing down others, at least for integers.
        // Large value types may benefit from a smaller number.
        internal const int IntrosortSizeThreshold = 16;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySortSpecialized<TKey>(
            ref TKey keys, ref int values, int keysLength)
        {
            int length = keysLength;
            // Type unfolding adopted from https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp#L268
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
        private static int NaNPrepass<TKey, TIsNaN>(
            ref TKey keys, ref int values, int length, 
            in TIsNaN isNaN)
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
        internal static void Sort<TKey, TComparer>(
            ref TKey keys, ref int values, int length, 
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            if (length < 2)
                return;

            IntrospectiveSort(ref keys, ref values, length, comparer);
        }

        private static void IntrospectiveSort<TKey, TComparer>(
            ref TKey keys, ref int values, int length, 
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            var depthLimit = 2 * FloorLog2PlusOne(length);
            IntroSort(ref keys, ref values, 0, length - 1, depthLimit, comparer);
            //IntroSort(ref keys, length - 1, depthLimit, comparer);
        }

        private static int FloorLog2PlusOne(int n)
        {
            Debug.Assert(n >= 2);
            int result = 0;
            while (n >= 1)
            {
                result++;
                n = n / 2;
            }
            return result;
        }

        private static void IntroSort<TKey, TComparer>(
            ref TKey keys, ref int values, 
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
                        Sort2(ref keys, lo, hi, comparer);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        ref TKey loRef = ref Unsafe.Add(ref keys, lo);
                        ref TKey miRef = ref Unsafe.Add(ref keys, hi - 1);
                        ref TKey hiRef = ref Unsafe.Add(ref keys, hi);
                        //ref TKey miRef = ref Unsafe.SubtractByteOffset(ref hiRef, new IntPtr(Unsafe.SizeOf<TKey>()));
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
                int p = PickPivotAndPartition(ref keys, ref values, lo, hi, comparer);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(ref keys, ref values, p + 1, hi, depthLimit, comparer);
                hi = p - 1;
            }
        }

        private static int PickPivotAndPartition<TKey, TComparer>(
            ref TKey keys, ref int values, int lo, int hi, 
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
            ref TKey loRef = ref Unsafe.Add(ref keys, lo);
            ref TKey miRef = ref Unsafe.Add(ref keys, middle);
            ref TKey hiRef = ref Unsafe.Add(ref keys, hi);
            Sort3(ref loRef, ref miRef, ref hiRef, comparer);

            TKey pivot = miRef;

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


        private static void HeapSort<TKey, TComparer>(
            ref TKey keys, ref int values, int lo, int hi, 
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
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

        private static void DownHeap<TKey, TComparer>(
            ref TKey keys, ref int values, int i, int n, int lo, 
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            Debug.Assert(comparer != null);
            Debug.Assert(lo >= 0);

            //TKey d = keys[lo + i - 1];
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
        private static void InsertionSort<TKey, TComparer>(
            ref TKey keys, ref int values, int lo, int hi, 
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
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
        internal static void Sort3<TKey, TComparer>(
            ref TKey r0, ref TKey r1, ref TKey r2, 
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            //Sort2(ref r0, ref r1, comparer); // swap the low with the mid point
            //Sort2(ref r0, ref r2, comparer); // swap the low with the high
            //Sort2(ref r1, ref r2, comparer); // swap the middle with the high

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
        private static void Sort2<TKey, TComparer>(ref TKey keys, int i, int j, TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            Debug.Assert(i != j);

            ref var iElement = ref Unsafe.Add(ref keys, i);
            ref var jElement = ref Unsafe.Add(ref keys, j);
            Sort2(ref iElement, ref jElement, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Sort2<TKey, TComparer>(ref TKey a, ref TKey b, TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            if (comparer.LessThan(b, a))
            {
                TKey temp = a;
                a = b;
                b = temp;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<TKey>(ref TKey items, int i, int j)
        {
            Debug.Assert(i != j);

            ref var iElement = ref Unsafe.Add(ref items, i);
            ref var jElement = ref Unsafe.Add(ref items, j);
            Swap(ref iElement, ref jElement);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<TKey>(ref TKey items, IntPtr i, IntPtr j)
        {
            Debug.Assert(i != j);

            ref var iElement = ref Unsafe.Add(ref items, i);
            ref var jElement = ref Unsafe.Add(ref items, j);
            Swap(ref iElement, ref jElement);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Swap<TKey>(ref TKey a, ref TKey b)
        {
            TKey temp = a;
            a = b;
            b = temp;
        }

        internal static class DefaultSpanSortHelper<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            //private static volatile ISpanSortHelper<TKey, TComparer> defaultArraySortHelper;

            //public static ISpanSortHelper<TKey, TComparer> Default
            //{
            //    get
            //    {
            //        ISpanSortHelper<TKey, TComparer> sorter = defaultArraySortHelper;
            //        if (sorter == null)
            //            sorter = CreateArraySortHelper();

            //        return sorter;
            //    }
            //}
            internal static readonly ISpanSortHelper<TKey, TComparer> s_default = CreateSortHelper();

            private static ISpanSortHelper<TKey, TComparer> CreateSortHelper()
            {
                if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
                {
                    // TODO: Is there a faster way?
                    var ctor = typeof(ComparableSpanSortHelper<,>)
                        .MakeGenericType(new Type[] { typeof(TKey), typeof(TComparer) })
                        .GetConstructor(Array.Empty<Type>());

                    return (ISpanSortHelper<TKey, TComparer>)ctor.Invoke(Array.Empty<object>());
                    // coreclr does the following:
                    //return (IArraySortHelper<TKey, TComparer>)
                    //    RuntimeTypeHandle.Allocate(
                    //        .TypeHandle.Instantiate());
                }
                else
                {
                    return new SpanSortHelper<TKey, TComparer>();
                }
            }
        }

        // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
        internal interface ISpanSortHelper<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            void Sort(Span<TKey> keys, ref int values, TComparer comparer);
        }

        internal class SpanSortHelper<TKey, TComparer> : ISpanSortHelper<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            public void Sort(Span<TKey> keys, ref int values, TComparer comparer)
            {
                // Add a try block here to detect IComparers (or their
                // underlying IComparables, etc) that are bogus.
                //
                // TODO: Do we need the try/catch? Just let it triggle up...
                //try
                //{
                if (typeof(TComparer) == typeof(IComparer<TKey>) && comparer == null)
                {
                    S.Sort(
                        ref keys.DangerousGetPinnableReference(), ref values, keys.Length,
                        new ComparerLessThanComparer<TKey, IComparer<TKey>>(Comparer<TKey>.Default));
                }
                else
                {
                    S.Sort(
                        ref keys.DangerousGetPinnableReference(), ref values, keys.Length,
                        new ComparerLessThanComparer<TKey, IComparer<TKey>>(comparer));
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

        internal class ComparableSpanSortHelper<TKey, TComparer>
            : ISpanSortHelper<TKey, TComparer>
            where TKey : IComparable<TKey>
            where TComparer : IComparer<TKey>
        {
            public void Sort(Span<TKey> keys, ref int values, TComparer comparer)
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
                    ref TKey keysRef = ref keys.DangerousGetPinnableReference();
                    if (!TrySortSpecialized(ref keysRef, ref values, keys.Length))
                    {
                        S.Sort(ref keysRef, ref values, keys.Length,
                               new ComparableLessThanComparer<TKey>());
                    }
                }
                else
                {
                    S.Sort(ref keys.DangerousGetPinnableReference(), ref values, keys.Length,
                           new ComparerLessThanComparer<TKey, TComparer>(comparer));
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
