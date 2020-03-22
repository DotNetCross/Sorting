using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        internal static partial class IComparable
        {
            internal static void HeapSort<TKey>(
                ref TKey keys, int lo, int hi)
                where TKey : IComparable<TKey>
            {

                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                int n = hi - lo + 1;
                for (int i = n / 2; i >= 1; --i)
                {
                    DownHeap(ref keys, i, n, lo);
                }
                for (int i = n; i > 1; --i)
                {
                    Swap(ref keys, lo, lo + i - 1);
                    DownHeap(ref keys, 1, i - 1, lo);
                }
            }

            internal static void DownHeap<TKey>(
                ref TKey keys, int i, int n, int lo)
                where TKey : IComparable<TKey>
            {

                Debug.Assert(lo >= 0);

                //TKey d = keys[lo + i - 1];
                ref TKey keysAtLo = ref Unsafe.Add(ref keys, lo);
                ref TKey keysAtLoMinus1 = ref Unsafe.Add(ref keysAtLo, -1); // No Subtract??
                TKey d = Unsafe.Add(ref keysAtLoMinus1, i);
                var nHalf = n / 2;
                while (i <= nHalf)
                {
                    int child = i << 1;

                    //if (child < n && (keys[lo + child - 1] == null || keys[lo + child - 1].CompareTo(keys[lo + child]) < 0))
                    if (child < n &&
                        (Unsafe.Add(ref keysAtLoMinus1, child) == null ||
                         Unsafe.Add(ref keysAtLoMinus1, child).CompareTo(Unsafe.Add(ref keysAtLo, child)) < 0))
                    {
                        ++child;
                    }

                    //if (keys[lo + child - 1] == null || keys[lo + child - 1].CompareTo(d) < 0)
                    if (Unsafe.Add(ref keysAtLoMinus1, child) == null ||
                        Unsafe.Add(ref keysAtLoMinus1, child).CompareTo(d) < 0)
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
}
