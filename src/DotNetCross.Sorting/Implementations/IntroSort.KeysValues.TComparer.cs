using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Common;

namespace DotNetCross.Sorting
{
    internal static partial class TComparerImpl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void IntroSort<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values, int length,
            TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            var depthLimit = 2 * FloorLog2PlusOne(length);
            IntroSort(ref keys, ref values, length, depthLimit, comparer);
        }

        private static void IntroSort<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values,
            int length, int depthLimit,
            TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            Debug.Assert(comparer != null);
            int partitionSize = length;
            while (partitionSize > 1)
            {
                if (partitionSize <= IntrosortSizeThreshold)
                {
                    Debug.Assert(partitionSize >= 2);
                    if (partitionSize == 2)
                    {
                        Sort2(ref keys, ref values, 0, 1, comparer);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        Sort3(ref keys, ref values, 0, 1, 2, comparer);
                        return;
                    }
                    InsertionSort(ref keys, ref values, partitionSize, comparer);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(ref keys, ref values, partitionSize, comparer);
                    return;
                }
                depthLimit--;

                // We should never reach here, unless > 3 elements due to partition size
                int p = PickPivotAndPartition(ref keys, ref values, partitionSize, comparer);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                var rightPartitionStart = p + 1;
                var rightPartitionLength = partitionSize - rightPartitionStart;
                ref var keysRightPartition = ref Unsafe.Add(ref keys, rightPartitionStart);
                ref var valuesRightPartition = ref Unsafe.Add(ref values, rightPartitionStart);
                IntroSort(ref keysRightPartition, ref valuesRightPartition, rightPartitionLength, depthLimit, comparer);

                Debug.Assert((p + rightPartitionLength + 1) == partitionSize);
                partitionSize = p;
            }
        }
    }
}