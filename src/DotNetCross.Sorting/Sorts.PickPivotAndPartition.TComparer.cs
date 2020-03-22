using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        internal static partial class TComparer
        {
            internal static int PickPivotAndPartition<TKey, TComparer>(
                ref TKey keys, int lo, int hi,
                TComparer comparer)
                where TComparer : IComparer<TKey>
            {
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                // Compute median-of-three.  But also partition them, since we've done the comparison.

                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know 
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint. 
                //       Saves one subtraction per loop compared to 
                //       `int middle = lo + ((hi - lo) >> 1);`
                int middle = (int)(((uint)hi + (uint)lo) >> 1);

                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                ref TKey keysAtLo = ref Unsafe.Add(ref keys, lo);
                ref TKey keysAtMiddle = ref Unsafe.Add(ref keys, middle);
                ref TKey keysAtHi = ref Unsafe.Add(ref keys, hi);
                Sort3(ref keysAtLo, ref keysAtMiddle, ref keysAtHi, comparer);

                TKey pivot = keysAtMiddle;

                int left = lo;
                int right = hi - 1;
                // We already partitioned lo and hi and put the pivot in hi - 1.  
                // And we pre-increment & decrement below.
                Swap(ref keysAtMiddle, ref Unsafe.Add(ref keys, right));

                while (left < right)
                {
                    // TODO: Would be good to be able to update local ref here

                    while (left < (hi - 1) && comparer.Compare(Unsafe.Add(ref keys, ++left), pivot) < 0) ;
                    // Check if bad comparable/comparer
                    if (left == (hi - 1) && comparer.Compare(Unsafe.Add(ref keys, left), pivot) < 0)
                        ThrowHelper.ThrowArgumentException_BadComparer(comparer);

                    while (right > lo && comparer.Compare(pivot, Unsafe.Add(ref keys, --right)) < 0) ;
                    // Check if bad comparable/comparer
                    if (right == lo && comparer.Compare(pivot, Unsafe.Add(ref keys, right)) < 0)
                        ThrowHelper.ThrowArgumentException_BadComparer(comparer);

                    if (left >= right)
                        break;

                    Swap(ref keys, left, right);
                }
                // Put pivot in the right location.
                right = hi - 1;
                if (left != right)
                {
                    Swap(ref keys, left, right);
                }
                return left;
            }
        }
    }
}
