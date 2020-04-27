using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Common;

namespace DotNetCross.Sorting
{
    internal partial class KeysSorter_Comparable<TKey>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntroSort(ref TKey keys, int length)
        {
            var depthLimit = 2 * FloorLog2PlusOne(length);
            IntroSort(ref keys, length, depthLimit);
        }

        void IntroSort(ref TKey keys, int length, int depthLimit)
        {
            int partitionSize = length;
            while (partitionSize > 1)
            {
                if (partitionSize <= IntrosortSizeThreshold)
                {
                    Debug.Assert(partitionSize >= 2);
                    if (partitionSize == 2)
                    {
                        Sort2(ref keys, ref Unsafe.Add(ref keys, 1));
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        ref TKey loRef = ref keys;
                        ref TKey miRef = ref Unsafe.Add(ref keys, 1);
                        ref TKey hiRef = ref Unsafe.Add(ref keys, 2);
                        //ref TKey miRef = ref Unsafe.SubtractByteOffset(ref hiRef, new IntPtr(Unsafe.SizeOf<TKey>()));
                        Sort3(ref loRef, ref miRef, ref hiRef);
                        return;
                    }

                    InsertionSort(ref keys, partitionSize);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(ref keys, partitionSize);
                    return;
                }
                depthLimit--;

                // We should never reach here, unless > 3 elements due to partition size
                int p = PickPivotAndPartition(ref keys, partitionSize);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                var rightPartitionStart = p + 1;
                var rightPartitionLength = length - rightPartitionStart;
                ref var keysRightPartition = ref Unsafe.Add(ref keys, rightPartitionStart);
                IntroSort(ref keysRightPartition, rightPartitionLength, depthLimit);
                partitionSize = p;
                Debug.Assert(partitionSize + rightPartitionLength + 1 == length);
            }
        }
    }
}
