// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

using static System.SpanSortHelpersCommon;
using static DotNetCross.Sorting.Sorts.TComparer;

namespace System
{
    internal static partial class SpanSortHelpersKeys_Comparer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<TKey, TComparer>(
            ref TKey keys, int length,
            TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            IntrospectiveSort(ref keys, length, comparer);
        }

        private static void IntrospectiveSort<TKey, TComparer>(
            ref TKey keys, int length,
            TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            var depthLimit = 2 * FloorLog2PlusOne(length);
            IntroSort(ref keys, 0, length - 1, depthLimit, comparer);
        }

        private static void IntroSort<TKey, TComparer>(
            ref TKey keys, 
            int lo, int hi, int depthLimit,
            TComparer comparer)
            where TComparer : IComparer<TKey>
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
            where TComparer : IComparer<TKey>
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

                while (left < (hi - 1) && comparer.Compare(Unsafe.Add(ref keys, ++left), pivot) < 0) ;
                // Check if bad comparable/comparer
                if (left == (hi - 1) && comparer.Compare(Unsafe.Add(ref keys, left), pivot) < 0)
                    ThrowHelper.ThrowArgumentException_BadComparer(comparer);

                while (right > lo && comparer.Compare(pivot, Unsafe.Add(ref keys, --right)) < 0) ;
                // Check if bad comparable/comparer
                if (right == lo && comparer.Compare(pivot, Unsafe.Add(ref keys, right)) < 0)
                    ThrowHelper.ThrowArgumentException_BadComparer(comparer);

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


    }
}
