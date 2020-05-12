using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Common;

namespace DotNetCross.Sorting
{
    internal partial class KeysValuesSorter_Comparison<TKey, TValue>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntroSort(
            ref TKey keys, ref TValue values, int length,
            Comparison<TKey> comparison)
        {
            var depthLimit = 2 * FloorLog2PlusOne(length);
            IntroSort(ref keys, ref values, length, depthLimit, comparison);
        }

        private static void IntroSort(
            ref TKey keys, ref TValue values,
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
                        Sort2(ref keys, ref values, 0, 1, comparison);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        Sort3(ref keys, ref values, 0, 1, 2, comparison);
                        return;
                    }
                    InsertionSort(ref keys, ref values, partitionSize, comparison);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(ref keys, ref values, partitionSize, comparison);
                    return;
                }
                depthLimit--;

                // We should never reach here, unless > 3 elements due to partition size
                int p = PickPivotAndPartition(ref keys, ref values, partitionSize, comparison);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                var rightPartitionStart = p + 1;
                var rightPartitionLength = partitionSize - rightPartitionStart;
                ref var keysRightPartition = ref Unsafe.Add(ref keys, rightPartitionStart);
                ref var valuesRightPartition = ref Unsafe.Add(ref values, rightPartitionStart);
                IntroSort(ref keysRightPartition, ref valuesRightPartition, rightPartitionLength, depthLimit, comparison);

                Debug.Assert((p + rightPartitionLength + 1) == partitionSize);
                partitionSize = p;
            }
        }
    }
}