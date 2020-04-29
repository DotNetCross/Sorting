using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Common;

namespace DotNetCross.Sorting
{
    internal partial class KeysSorter_Comparison<TKey>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntroSort(
            ref TKey keys, int length,
            Comparison<TKey> comparison)
        {
            // TODO: Check if comparison is Comparer<TKey>.Default.Compare
            //       and if reference type or not

            var depthLimit = 2 * FloorLog2PlusOne(length);
            IntroSort(ref keys, length, depthLimit, comparison);
        }

        private static void IntroSort(
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
                        Sort3(ref loRef, ref miRef, ref hiRef, comparison);
                        return;
                    }

                    InsertionSort(ref keys, partitionSize, comparison);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(ref keys, partitionSize, comparison);
                    return;
                }
                depthLimit--;

                // We should never reach here, unless > 3 elements due to partition size
                int p = PickPivotAndPartition(ref keys, partitionSize, comparison);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                var rightPartitionStart = p + 1;
                var rightPartitionLength = partitionSize - rightPartitionStart;
                ref var keysRightPartition = ref Unsafe.Add(ref keys, rightPartitionStart);
                IntroSort(ref keysRightPartition, rightPartitionLength, depthLimit, comparison);

                Debug.Assert((p + rightPartitionLength + 1) == partitionSize);
                partitionSize = p;
            }
        }
    }
}