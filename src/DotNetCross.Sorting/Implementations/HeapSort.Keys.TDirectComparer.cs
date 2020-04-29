using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal static partial class TDirectComparerImpl
    {
        internal static void HeapSort<TKey, TComparer>(
            ref TKey keys, int length,
            TComparer comparer)
            where TComparer : IDirectComparer<TKey>
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

        internal static void DownHeap<TKey, TComparer>(
            ref TKey keys, int i, int n, int lo,
            TComparer comparer)
            where TComparer : IDirectComparer<TKey>
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
                    comparer.LessThan(Unsafe.Add(ref keysAtLoMinus1, child), Unsafe.Add(ref keysAtLo, child)))
                {
                    ++child;
                }

                //if (!(comparer(d, keys[lo + child - 1]) < 0))
                if (!(comparer.LessThan(d, Unsafe.Add(ref keysAtLoMinus1, child))))
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