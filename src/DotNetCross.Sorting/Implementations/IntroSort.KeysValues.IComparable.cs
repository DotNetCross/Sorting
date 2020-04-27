using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Common;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysValuesSorter_Comparable<TKey, TValue>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntroSort(
            ref TKey keys, ref TValue values, int length)
        {
            var depthLimit = 2 * FloorLog2PlusOne(length);
            IntroSort(ref keys, ref values, length, depthLimit);
        }

        void IntroSort(
            ref TKey keys, ref TValue values,
            int length, int depthLimit)
        {
            int partitionSize = length;
            while (partitionSize > 1)
            {
                if (partitionSize <= IntrosortSizeThreshold)
                {
                    Debug.Assert(partitionSize >= 2);
                    if (partitionSize == 2)
                    {
                        Sort2(ref keys, ref values, 0, 1);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        Sort3(ref keys, ref values, 0, 1, 2);
                        return;
                    }
                    InsertionSort(ref keys, ref values, partitionSize);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(ref keys, ref values, partitionSize);
                    return;
                }
                depthLimit--;

                // We should never reach here, unless > 3 elements due to partition size
                int p = PickPivotAndPartition(ref keys, ref values, partitionSize);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                var rightPartitionStart = p + 1;
                var rightPartitionLength = length - rightPartitionStart;
                ref var keysRightPartition = ref Unsafe.Add(ref keys, rightPartitionStart);
                ref var valuesRightPartition = ref Unsafe.Add(ref values, rightPartitionStart);
                IntroSort(ref keysRightPartition, ref valuesRightPartition, rightPartitionLength, depthLimit);
                partitionSize = p;
                Debug.Assert(partitionSize + rightPartitionLength + 1 == length);
            }
        }
    }
}