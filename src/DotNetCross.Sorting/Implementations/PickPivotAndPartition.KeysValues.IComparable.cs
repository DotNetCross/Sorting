using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysValuesSorter_Comparable<TKey, TValue>
    {
        internal int PickPivotAndPartition(
            ref TKey keys, ref TValue values, int lo, int hi)
        {

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
            ref TKey keysAtMiddle = ref Sort3(ref keys, ref values, lo, middle, hi);

            TKey pivot = keysAtMiddle;

            int left = lo;
            int right = hi - 1;
            // We already partitioned lo and hi and put the pivot in hi - 1.  
            // And we pre-increment & decrement below.
            Swap(ref keysAtMiddle, ref Unsafe.Add(ref keys, right));
            Swap(ref values, middle, right);

            while (left < right)
            {
                // TODO: Would be good to be able to update local ref here

                if (pivot == null)
                {
                    while (left < (hi - 1) && Unsafe.Add(ref keys, ++left) == null) ;
                    while (right > lo && Unsafe.Add(ref keys, --right) != null) ;
                }
                else
                {
                    while (left < (hi - 1) && pivot.CompareTo(Unsafe.Add(ref keys, ++left)) > 0) ;
                    // Check if bad comparable/comparer
                    if (left == (hi - 1) && pivot.CompareTo(Unsafe.Add(ref keys, left)) > 0)
                        ThrowHelper.ThrowArgumentException_BadComparable(typeof(TKey));

                    while (right > lo && pivot.CompareTo(Unsafe.Add(ref keys, --right)) < 0) ;
                    // Check if bad comparable/comparer
                    if (right == lo && pivot.CompareTo(Unsafe.Add(ref keys, right)) < 0)
                        ThrowHelper.ThrowArgumentException_BadComparable(typeof(TKey));
                }

                if (left >= right)
                    break;

                Swap(ref keys, left, right);
                Swap(ref values, left, right);
            }
            // Put pivot in the right location.
            right = hi - 1;
            if (left != right)
            {
                Swap(ref keys, left, right);
                Swap(ref values, left, right);
            }
            return left;
        }
    }
}