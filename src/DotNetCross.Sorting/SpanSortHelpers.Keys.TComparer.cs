﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

using static System.SpanSortHelpersCommon;
using S = System.SpanSortHelpersKeys;

namespace System
{
    internal static partial class SpanSortHelpersKeys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<TKey>(this Span<TKey> keys)
        {
            int length = keys.Length;
            if (length < 2)
                return;

            // PERF: Try specialized here for optimal performance
            // Code-gen is weird unless used in loop outside
            if (!TrySortSpecialized(
                ref keys.DangerousGetPinnableReference(), length))
            {
                DefaultSpanSortHelper<TKey>.s_default.Sort(
                    ref keys.DangerousGetPinnableReference(),
                    length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<TKey, TComparer>(
            this Span<TKey> keys, TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            int length = keys.Length;
            if (length < 2)
                return;

            DefaultSpanSortHelper<TKey, TComparer>.s_default.Sort(
                ref keys.DangerousGetPinnableReference(),
                keys.Length, comparer);
        }

        // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<TKey, TComparer>(
            ref TKey keys, int length,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            IntrospectiveSort(ref keys, length, comparer);
        }

        private static void IntrospectiveSort<TKey, TComparer>(
            ref TKey keys, int length,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            var depthLimit = 2 * FloorLog2PlusOne(length);
            IntroSort(ref keys, 0, length - 1, depthLimit, comparer);
        }

        private static void IntroSort<TKey, TComparer>(
            ref TKey keys, 
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

                    InsertionSort(ref keys, lo, hi, comparer);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(ref keys, lo, hi, comparer);
                    return;
                }
                depthLimit--;

                // We should never reach here, unless > 3 elements due to partition size
                int p = PickPivotAndPartition(ref keys, lo, hi, comparer);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(ref keys, p + 1, hi, depthLimit, comparer);
                hi = p - 1;
            }
        }

        private static int PickPivotAndPartition<TKey, TComparer>(
            ref TKey keys, int lo, int hi,
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
            ref TKey keysAtLo = ref Unsafe.Add(ref keys, lo);
            ref TKey keysAtMiddle = ref Unsafe.Add(ref keys, middle);
            ref TKey keysAtHi = ref Unsafe.Add(ref keys, hi);
            Sort3(ref keysAtLo, ref keysAtMiddle, ref keysAtHi, comparer);

            TKey pivot = keysAtMiddle;

            int left = lo;
            int right = hi - 1;
            // We already partitioned lo and hi and put the pivot in hi - 1.  
            // And we pre-increment & decrement below.
            Swap(ref keysAtMiddle, ref Unsafe.Add(ref keys, right));

            while (left < right)
            {
                // TODO: Would be good to be able to update local ref here
                while (comparer.LessThan(Unsafe.Add(ref keys, ++left), pivot)) ;
                // TODO: Would be good to be able to update local ref here
                while (comparer.LessThan(pivot, Unsafe.Add(ref keys, --right))) ;

                if (left >= right)
                    break;

                Swap(ref keys, left, right);
            }
            // Put pivot in the right location.
            right = hi - 1;
            if (left != right)
            {
                Swap(ref keys, left, right);
            }
            return left;
        }

        private static void HeapSort<TKey, TComparer>(
            ref TKey keys, int lo, int hi,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            Debug.Assert(comparer != null);
            Debug.Assert(lo >= 0);
            Debug.Assert(hi > lo);

            int n = hi - lo + 1;
            for (int i = n / 2; i >= 1; --i)
            {
                DownHeap(ref keys, i, n, lo, comparer);
            }
            for (int i = n; i > 1; --i)
            {
                Swap(ref keys, lo, lo + i - 1);
                DownHeap(ref keys, 1, i - 1, lo, comparer);
            }
        }

        private static void DownHeap<TKey, TComparer>(
            ref TKey keys, int i, int n, int lo,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            Debug.Assert(comparer != null);
            Debug.Assert(lo >= 0);

            //TKey d = keys[lo + i - 1];
            ref TKey keysAtLo = ref Unsafe.Add(ref keys, lo);
            ref TKey keysAtLoMinus1 = ref Unsafe.Subtract(ref keysAtLo, 1);
            TKey d = Unsafe.Add(ref keysAtLoMinus1, i);
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

                i = child;
            }
            //keys[lo + i - 1] = d;
            Unsafe.Add(ref keysAtLoMinus1, i) = d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InsertionSort<TKey, TComparer>(
            ref TKey keys, int lo, int hi,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            Debug.Assert(lo >= 0);
            Debug.Assert(hi >= lo);

            for (int i = lo; i < hi; ++i)
            {
                int j = i;
                //t = keys[i + 1];
                var t = Unsafe.Add(ref keys, j + 1);
                // TODO: Would be good to be able to update local ref here
                if (j >= lo && comparer.LessThan(t, Unsafe.Add(ref keys, j)))
                {
                    do
                    {
                        Unsafe.Add(ref keys, j + 1) = Unsafe.Add(ref keys, j);
                        --j;
                    }
                    while (j >= lo && comparer.LessThan(t, Unsafe.Add(ref keys, j)));

                    Unsafe.Add(ref keys, j + 1) = t;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Sort3<TKey, TComparer>(
            ref TKey r0, ref TKey r1, ref TKey r2,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
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
        private static void Sort2<TKey, TComparer>(
            ref TKey keys, int i, int j,
            TComparer comparer)
            where TComparer : ILessThanComparer<TKey>
        {
            Debug.Assert(i != j);

            ref TKey a = ref Unsafe.Add(ref keys, i);
            ref TKey b = ref Unsafe.Add(ref keys, j);
            if (comparer.LessThan(b, a))
            {
                TKey temp = a;
                a = b;
                b = temp;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<T>(ref T items, int i, int j)
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


        internal static class DefaultSpanSortHelper<TKey>
        {
            internal static readonly ISpanSortHelper<TKey> s_default = CreateSortHelper();

            private static ISpanSortHelper<TKey> CreateSortHelper()
            {
                if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
                {
                    if (typeof(TKey).IsValueType)
                    {
                        // TODO: Is there a faster way? A way without heap alloc? 
                        // Albeit, this only happens once for each type combination
                        var ctor = typeof(ComparableSpanSortHelper<>)
                        .MakeGenericType(new Type[] { typeof(TKey) })
                        .GetConstructor(Array.Empty<Type>());

                        return (ISpanSortHelper<TKey>)ctor.Invoke(Array.Empty<object>());
                        // coreclr does the following:
                        //return (IArraySortHelper<T, TComparer>)
                        //    RuntimeTypeHandle.Allocate(
                        //        .TypeHandle.Instantiate());
                    }
                    else
                    {
                        // TODO: Is there a faster way? A way without heap alloc? 
                        // Albeit, this only happens once for each type combination
                        var ctor = typeof(IComparableSpanSortHelper<>)
                        .MakeGenericType(new Type[] { typeof(TKey) })
                        .GetConstructor(Array.Empty<Type>());

                        return (ISpanSortHelper<TKey>)ctor.Invoke(Array.Empty<object>());
                    }
                }
                else
                {
                    return new SpanSortHelper<TKey>();
                }
            }
        }

        internal interface ISpanSortHelper<TKey>
        {
            void Sort(ref TKey keys, int length);
        }

        internal class SpanSortHelper<TKey> : ISpanSortHelper<TKey>
        {
            public void Sort(ref TKey keys, int length)
            {
                S.Sort(ref keys, length,
                    new ComparerLessThanComparer<TKey, IComparer<TKey>>(Comparer<TKey>.Default));
            }
        }

        internal class ComparableSpanSortHelper<TKey>
            : ISpanSortHelper<TKey>
            where TKey : struct, IComparable<TKey>
        {
            public void Sort(ref TKey keys, int length)
            {
                S.Sort(ref keys, length, new ComparableLessThanComparer<TKey>());
            }
        }

        internal class IComparableSpanSortHelper<TKey>
            : ISpanSortHelper<TKey>
            where TKey : class, IComparable<TKey>
        {
            public void Sort(ref TKey keys, int length)
            {
                S.Sort<IComparable<TKey>, IComparableLessThanComparer<TKey>>(
                    ref Unsafe.As<TKey, IComparable<TKey>>(ref keys), length, 
                    new IComparableLessThanComparer<TKey>());
            }
        }


        internal static class DefaultSpanSortHelper<TKey, TComparer>
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
            internal static readonly ISpanSortHelper<TKey, TComparer> s_default = CreateSortHelper();

            private static ISpanSortHelper<TKey, TComparer> CreateSortHelper()
            {
                if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
                {
                    if (typeof(TKey).IsValueType)
                    {
                        // TODO: Is there a faster way? A way without heap alloc? 
                        // Albeit, this only happens once for each type combination
                        var ctor = typeof(ComparableSpanSortHelper<,>)
                            .MakeGenericType(new Type[] { typeof(TKey), typeof(TComparer) })
                            .GetConstructor(Array.Empty<Type>());

                        return (ISpanSortHelper<TKey, TComparer>)ctor.Invoke(Array.Empty<object>());
                        // coreclr does the following:
                        //return (IArraySortHelper<T, TComparer>)
                        //    RuntimeTypeHandle.Allocate(
                        //        .TypeHandle.Instantiate());
                    }
                    else
                    {
                        // TODO: Is there a faster way? A way without heap alloc? 
                        // Albeit, this only happens once for each type combination
                        var ctor = typeof(IComparableSpanSortHelper<,>)
                            .MakeGenericType(new Type[] { typeof(TKey), typeof(TComparer) })
                            .GetConstructor(Array.Empty<Type>());

                        return (ISpanSortHelper<TKey, TComparer>)ctor.Invoke(Array.Empty<object>());
                    }
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
            void Sort(ref TKey keys, int length, TComparer comparer);
        }

        internal class SpanSortHelper<TKey, TComparer> : ISpanSortHelper<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            public void Sort(ref TKey keys, int length, TComparer comparer)
            {
                // Add a try block here to detect IComparers (or their
                // underlying IComparables, etc) that are bogus.
                //
                // TODO: Do we need the try/catch?
                //try
                //{
                if (typeof(TComparer) == typeof(IComparer<TKey>) && comparer == null)
                {
                    S.Sort(ref keys, length,
                        new ComparerLessThanComparer<TKey, IComparer<TKey>>(Comparer<TKey>.Default));
                }
                else
                {
                    S.Sort(ref keys, length,
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
            public void Sort(ref TKey keys, int length,
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
                    if (!S.TrySortSpecialized(ref keys, length))
                    {
                        S.Sort(ref keys, length,
                            new ComparableLessThanComparer<TKey>());
                    }
                }
                else
                {
                    S.Sort(ref keys, length,
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
        internal class IComparableSpanSortHelper<TKey, TComparer>
            : ISpanSortHelper<TKey, TComparer>
            where TKey : class, IComparable<TKey>
            where TComparer : IComparer<TKey>
        {
            public void Sort(ref TKey keys, int length,
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
                    if (!S.TrySortSpecialized(ref keys, length))
                    {
                        if (typeof(TKey).IsValueType)
                        {
                            S.Sort(ref keys, length, new ComparableLessThanComparer<TKey>());
                        }
                        else
                        {
                            S.Sort(ref Unsafe.As<TKey, IComparable<TKey>>(ref keys), length,
                                new IComparableLessThanComparer<TKey>());
                        }
                    }
                }
                else
                {
                    S.Sort(ref keys, length,
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
