using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal partial class KeysSorter_TComparer<TKey, TComparer>
    {
        internal static void HeapSort(
            ref TKey keys, int length,
            TComparer comparer)
        {
            Debug.Assert(comparer != null);
            Debug.Assert(length >= 0);

            int n = length;
            for (int i = n >> 1; i >= 1; --i)
            {
                DownHeap(ref keys, i, n, 0, comparer);
            }
            for (int i = n; i > 1; --i)
            {
                Swap(ref keys, 0, i - 1);
                DownHeap(ref keys, 1, i - 1, 0, comparer);
            }
        }

        internal static void DownHeap(
            ref TKey keys, int i, int n, int lo,
            TComparer comparer)
        {
            Debug.Assert(comparer != null);
            Debug.Assert(lo >= 0);

            //TKey d = keys[lo + i - 1];
            ref TKey keysAtLo = ref Unsafe.Add(ref keys, lo);
            ref TKey keysAtLoMinus1 = ref Unsafe.Add(ref keysAtLo, -1); // No Subtract??
            TKey d = Unsafe.Add(ref keysAtLoMinus1, i);
            var nHalf = n >> 1;
            while (i <= nHalf)
            {
                int child = i << 1;

                //if (child < n && comparer(keys[lo + child - 1], keys[lo + child]) < 0)
                if (child < n &&
                    comparer.Compare(Unsafe.Add(ref keysAtLoMinus1, child), Unsafe.Add(ref keysAtLo, child)) < 0)
                {
                    ++child;
                }

                //if (!(comparer(d, keys[lo + child - 1]) < 0))
                if (!(comparer.Compare(d, Unsafe.Add(ref keysAtLoMinus1, child)) < 0))
                    break;

                // keys[lo + i - 1] = keys[lo + child - 1]
                Unsafe.Add(ref keysAtLoMinus1, i) = Unsafe.Add(ref keysAtLoMinus1, child);

                i = child;
            }
            //keys[lo + i - 1] = d;
            Unsafe.Add(ref keysAtLoMinus1, i) = d;
        }
    }
}