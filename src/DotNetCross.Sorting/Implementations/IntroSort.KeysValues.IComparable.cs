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
            IntroSort(ref keys, ref values, 0, length - 1, depthLimit);
        }

        void IntroSort(
            ref TKey keys, ref TValue values,
            int lo, int hi, int depthLimit)
        {
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
                        Sort2(ref keys, ref values, lo, hi);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        Sort3(ref keys, ref values, lo, hi - 1, hi);
                        return;
                    }
                    InsertionSort(ref keys, ref values, lo, hi);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(ref keys, ref values, lo, hi);
                    return;
                }
                depthLimit--;

                // We should never reach here, unless > 3 elements due to partition size
                int p = PickPivotAndPartition(ref keys, ref values, lo, hi);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(ref keys, ref values, p + 1, hi, depthLimit);
                hi = p - 1;
            }
        }
    }
}