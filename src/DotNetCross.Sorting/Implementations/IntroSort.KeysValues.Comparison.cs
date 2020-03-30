﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Common;

namespace DotNetCross.Sorting
{
    internal static partial class ComparisonImpl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void IntroSort<TKey, TValue>(
            ref TKey keys, ref TValue values, int length,
            Comparison<TKey> comparison)
        {
            var depthLimit = 2 * FloorLog2PlusOne(length);
            IntroSort(ref keys, ref values, 0, length - 1, depthLimit, comparison);
        }

        private static void IntroSort<TKey, TValue>(
            ref TKey keys, ref TValue values,
            int lo, int hi, int depthLimit,
            Comparison<TKey> comparison)
        {
            Debug.Assert(comparison != null);
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
                        Sort2(ref keys, ref values, lo, hi, comparison);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        Sort3(ref keys, ref values, lo, hi - 1, hi, comparison);
                        return;
                    }
                    InsertionSort(ref keys, ref values, lo, hi, comparison);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(ref keys, ref values, lo, hi, comparison);
                    return;
                }
                depthLimit--;

                // We should never reach here, unless > 3 elements due to partition size
                int p = PickPivotAndPartition(ref keys, ref values, lo, hi, comparison);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(ref keys, ref values, p + 1, hi, depthLimit, comparison);
                hi = p - 1;
            }
        }
    }
}