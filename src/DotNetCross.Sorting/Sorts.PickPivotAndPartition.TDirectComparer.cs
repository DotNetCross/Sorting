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

                // Compute median-of-three. But also partition them, since we've done the comparison.

                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know 
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint. 
                //       Saves one subtraction per loop compared to 
                //       `int middle = lo + ((hi - lo) >> 1);`
                int middle = (int)(((uint)hi + (uint)lo) >> 1);

                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                ref TKey keysLeft = ref Unsafe.Add(ref keys, lo);
                ref TKey keysMiddle = ref Unsafe.Add(ref keys, middle);
                ref TKey keysRight = ref Unsafe.Add(ref keys, hi);
                Sort3(ref keysLeft, ref keysMiddle, ref keysRight, comparer);

                TKey pivot = keysMiddle;

                int left = lo;
                int right = hi - 1;
                // We already partitioned lo and hi and put the pivot in hi - 1.  
                // And we pre-increment & decrement below.
                keysRight = ref Unsafe.Add(ref keysRight, -1);
                Swap(ref keysMiddle, ref keysRight);

                while (Unsafe.IsAddressLessThan(ref keysLeft, ref keysRight))
                {
                    // PERF: For internal direct comparers the range checks are not needed
                    //       since we know they cannot be bogus i.e. pass the pivot without being false.
                    do { ++left; keysLeft = ref Unsafe.Add(ref keysLeft, 1); } 
                    while (comparer.LessThan(keysLeft, pivot));
                    do { keysRight = ref Unsafe.Add(ref keysRight, -1); }
                    while (comparer.LessThan(pivot, keysRight));

                    if (Unsafe.AreSame(ref keysLeft, ref keysRight) ||
                        Unsafe.IsAddressGreaterThan(ref keysLeft, ref keysRight))// (left >= right)
                        break;

                    Swap(ref keysLeft, ref keysRight);
                }
                // Put pivot in the right location.
                keysRight = ref Unsafe.Add(ref keys, hi - 1);
                if (!Unsafe.AreSame(ref keysLeft, ref keysRight))
                {
                    Swap(ref keysLeft, ref keysRight);
                }

                return left;
            }
        }
    }
}
