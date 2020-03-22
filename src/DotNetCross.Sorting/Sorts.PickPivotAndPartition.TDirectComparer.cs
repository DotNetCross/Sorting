using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.SpanSortHelpersCommon;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        internal static partial class TDirectComparer
        {
            internal static int PickPivotAndPartition<TKey, TComparer>(
                ref TKey keys, int lo, int hi,
                TComparer comparer)
                where TComparer : IDirectComparer<TKey>
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

                    // PERF: For internal direct comparers the range checks are not needed
                    //       since we know they cannot be bogus i.e. pass the pivot without being false.
                    while (comparer.LessThan(Unsafe.Add(ref keys, ++left), pivot)) ;
                    while (comparer.LessThan(pivot, Unsafe.Add(ref keys, --right))) ;

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
