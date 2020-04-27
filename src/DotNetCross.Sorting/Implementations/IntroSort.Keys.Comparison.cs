using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Common;

namespace DotNetCross.Sorting
{
    internal static partial class ComparisonImpl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void IntroSort<TKey>(
            ref TKey keys, int length,
            Comparison<TKey> comparison)
        {
            var depthLimit = 2 * FloorLog2PlusOne(length);

            //IntroSort(ref keys, 0, length - 1, depthLimit, comparison);
            IntroSort(ref keys, length, depthLimit, comparison);
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

        private static void IntroSort<TKey>(
            ref TKey keys,
            int length, int depthLimit,
            Comparison<TKey> comparison)
        {
            Debug.Assert(comparison != null);
            int partitionSize = length;
            while (partitionSize > 1)
            {
                if (partitionSize <= IntrosortSizeThreshold)
                {
                    Debug.Assert(partitionSize >= 2);
                    if (partitionSize == 2)
                    {
                        Sort2(ref keys, ref Unsafe.Add(ref keys, 1), comparison);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        ref TKey loRef = ref keys;
                        ref TKey miRef = ref Unsafe.Add(ref keys, 1);
                        ref TKey hiRef = ref Unsafe.Add(ref keys, 2);
                        //ref TKey miRef = ref Unsafe.SubtractByteOffset(ref hiRef, new IntPtr(Unsafe.SizeOf<TKey>()));
                        Sort3(ref loRef, ref miRef, ref hiRef, comparison);
                        return;
                    }

                    InsertionSort(ref keys, partitionSize, comparison);
                    return;
                }

                if (depthLimit == 0)
                {
                    throw new NotImplementedException("TEST");
                    //HeapSort(ref keys, partitionSize, comparison);
                    return;
                }
                depthLimit--;

                // We should never reach here, unless > 3 elements due to partition size
                //int p = PickPivotAndPartition(ref keys, lo, hi, comparison);
                int p = PickPivotAndPartition(ref keys, partitionSize, comparison);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                var rightPartitionStart = p + 1;
                var rightPartitionLength = partitionSize - rightPartitionStart;
                ref var keysRightPartition = ref Unsafe.Add(ref keys, rightPartitionStart);
                IntroSort(ref keysRightPartition, rightPartitionLength, depthLimit, comparison);
                partitionSize = p;
                //hi = p - 1;
                ////Debug.Assert(partitionSize + rightPartitionLength + 1 == length);
                //IntroSort(ref keys, p + 1, hi, depthLimit, comparison);
                //hi = p - 1;
            }
        }
    }
}