﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Common;

namespace DotNetCross.Sorting
{
    internal static partial class KeysSorter_TDirectComparer<TKey, TComparer>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void IntroSort(
            ref TKey keys, int length,
            TComparer comparer)
        {
            var depthLimit = 2 * FloorLog2PlusOne(length);
            IntroSort(ref keys, length, depthLimit, comparer);
        }

        private static void IntroSort(
            ref TKey keys,
            int length, int depthLimit,
            TComparer comparer)
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
                        Sort2(ref keys, ref Unsafe.Add(ref keys, 1), comparer);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        ref TKey loRef = ref keys;
                        ref TKey miRef = ref Unsafe.Add(ref keys, 1);
                        ref TKey hiRef = ref Unsafe.Add(ref keys, 2);
                        //ref TKey miRef = ref Unsafe.SubtractByteOffset(ref hiRef, new IntPtr(Unsafe.SizeOf<TKey>()));
                        Sort3(ref loRef, ref miRef, ref hiRef, comparer);
                        return;
                    }

                    InsertionSort(ref keys, partitionSize, comparer);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(ref keys, partitionSize, comparer);
                    return;
                }
                depthLimit--;

                // We should never reach here, unless > 3 elements due to partition size
                int p = PickPivotAndPartition(ref keys, partitionSize, comparer);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                var rightPartitionStart = p + 1;
                var rightPartitionLength = partitionSize - rightPartitionStart;
                ref var keysRightPartition = ref Unsafe.Add(ref keys, rightPartitionStart);
                IntroSort(ref keysRightPartition, rightPartitionLength, depthLimit, comparer);

                Debug.Assert((p + rightPartitionLength + 1) == partitionSize);
                partitionSize = p;
            }
        }
    }
}