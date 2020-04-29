using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal partial class KeysSorter_Comparison<TKey>
    {
        internal static void HeapSort(
            ref TKey keys, int length,
            Comparison<TKey> comparison)

        {
            Debug.Assert(comparison != null);
            Debug.Assert(length >= 0);

            int n = length;
            for (int i = n >> 1; i >= 1; --i)
            {
                DownHeap(ref keys, i, n, 0, comparison);
            }
            for (int i = n; i > 1; --i)
            {
                Swap(ref keys, 0, i - 1);
                DownHeap(ref keys, 1, i - 1, 0, comparison);
            }
        }

        internal static void DownHeap(
            ref TKey keys, int i, int n, int lo,
            Comparison<TKey> comparison)

        {
            Debug.Assert(comparison != null);
            Debug.Assert(lo >= 0);

            //TKey d = keys[lo + i - 1];
            ref TKey keysAtLo = ref Unsafe.Add(ref keys, lo);
            ref TKey keysAtLoMinus1 = ref Unsafe.Add(ref keysAtLo, -1); // No Subtract??
            TKey d = Unsafe.Add(ref keysAtLoMinus1, i);
            var nHalf = n >> 1;
            while (i <= nHalf)
            {
                int child = i << 1;

                //if (child < n && comparison(keys[lo + child - 1], keys[lo + child]) < 0)
                if (child < n &&
                    comparison(Unsafe.Add(ref keysAtLoMinus1, child), Unsafe.Add(ref keysAtLo, child)) < 0)
                {
                    ++child;
                }

                //if (!(comparison(d, keys[lo + child - 1]) < 0))
                if (!(comparison(d, Unsafe.Add(ref keysAtLoMinus1, child)) < 0))
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
