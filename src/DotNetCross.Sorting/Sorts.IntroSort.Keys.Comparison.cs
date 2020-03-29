﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        internal static partial class Comparison
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void IntroSort<TKey>(
                ref TKey keys, int length,
                Comparison<TKey> comparison)
            {
                var depthLimit = 2 * FloorLog2PlusOne(length);
                IntroSort(ref keys, 0, length - 1, depthLimit, comparison);
            }

            private static void IntroSort<TKey>(
                ref TKey keys,
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
                            Sort2(ref keys, lo, hi, comparison);
                            return;
                        }
                        if (partitionSize == 3)
                        {
                            ref TKey loRef = ref Unsafe.Add(ref keys, lo);
                            ref TKey miRef = ref Unsafe.Add(ref keys, hi - 1);
                            ref TKey hiRef = ref Unsafe.Add(ref keys, hi);
                            //ref TKey miRef = ref Unsafe.SubtractByteOffset(ref hiRef, new IntPtr(Unsafe.SizeOf<TKey>()));
                            Sort3(ref loRef, ref miRef, ref hiRef, comparison);
                            return;
                        }

                        InsertionSort(ref keys, lo, hi, comparison);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        HeapSort(ref keys, lo, hi, comparison);
                        return;
                    }
                    depthLimit--;

                    // We should never reach here, unless > 3 elements due to partition size
                    int p = PickPivotAndPartition(ref keys, lo, hi, comparison);
                    // Note we've already partitioned around the pivot and do not have to move the pivot again.
                    IntroSort(ref keys, p + 1, hi, depthLimit, comparison);
                    hi = p - 1;
                }
            }
        }
    }
}