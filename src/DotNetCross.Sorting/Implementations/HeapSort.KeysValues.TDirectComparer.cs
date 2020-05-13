using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal static partial class KeysValuesSorter_TDirectComparer<TKey, TValue, TComparer>
    {
        internal static void HeapSort(
            ref TKey keys, ref TValue values, int length,
            TComparer comparer)

        {
            Debug.Assert(comparer != null);
            Debug.Assert(length >= 0);

            int n = length;
            for (int i = n >> 1; i >= 1; --i)
            {
                DownHeap(ref keys, ref values, i, n, 0, comparer);
            }
            for (int i = n; i > 1; --i)
            {
                Swap(ref keys, 0, i - 1);
                Swap(ref values, 0, i - 1);
                DownHeap(ref keys, ref values, 1, i - 1, 0, comparer);
            }
        }

        private static void DownHeap(
            ref TKey keys, ref TValue values, int i, int n, int lo,
            TComparer comparer)

        {
            Debug.Assert(comparer != null);
            Debug.Assert(lo >= 0);

            //TKey d = keys[lo + i - 1];
            ref TKey keysAtLo = ref Unsafe.Add(ref keys, lo);
            ref TKey keysAtLoMinus1 = ref Unsafe.Add(ref keysAtLo, -1); // No Subtract??

            ref TValue valuesAtLoMinus1 = ref Unsafe.Add(ref values, lo - 1);

            TKey d = Unsafe.Add(ref keysAtLoMinus1, i);
            TValue dValue = Unsafe.Add(ref valuesAtLoMinus1, i);

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
                    //if (comparer.LessThan(Unsafe.Add(ref keysAtLoMinus1, child), d))
                    break;

                // keys[lo + i - 1] = keys[lo + child - 1]
                Unsafe.Add(ref keysAtLoMinus1, i) = Unsafe.Add(ref keysAtLoMinus1, child);
                Unsafe.Add(ref valuesAtLoMinus1, i) = Unsafe.Add(ref valuesAtLoMinus1, child);

                i = child;
            }
            //keys[lo + i - 1] = d;
            Unsafe.Add(ref keysAtLoMinus1, i) = d;
            Unsafe.Add(ref valuesAtLoMinus1, i) = dValue;
        }
    }
}