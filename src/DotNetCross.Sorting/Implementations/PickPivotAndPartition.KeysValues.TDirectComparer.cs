using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal static partial class KeysValuesSorter_TDirectComparer
    {
        internal static int PickPivotAndPartition<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values, int length,
            TComparer comparer)
            where TComparer : IDirectComparer<TKey>
        {
            Debug.Assert(comparer != null);
            Debug.Assert(length > 2);
            //
            // Compute median-of-three.  But also partition them, since we've done the comparison.
            //
            // Sort left, middle and right appropriately, then pick middle as the pivot.
            int middle = (length - 1) >> 1;
            ref TKey keysMiddle = ref Sort3(ref keys, ref values, 0, middle, length - 1, comparer);

            TKey pivot = keysMiddle;

            int left = 0;
            int nextToLast = length - 2;
            int right = nextToLast;
            // We already partitioned lo and hi and put the pivot in hi - 1.  
            // And we pre-increment & decrement below.
            ref TKey keysLeft = ref Unsafe.Add(ref keys, left);
            ref TKey keysRight = ref Unsafe.Add(ref keys, right);
            Swap(ref keysMiddle, ref keysRight);
            Swap(ref values, middle, right);

            while (left < right)
            {
                if (pivot == null)
                {
                    do { ++left; keysLeft = ref Unsafe.Add(ref keysLeft, 1); }
                    while (left < right && keysLeft == null);

                    do { --right; keysRight = ref Unsafe.Add(ref keysRight, -1); }
                    while (right > 0 && keysRight != null);
                }
                else
                {
                    // PERF: For internal direct comparers the range checks are not needed
                    //       since we know they cannot be bogus i.e. pass the pivot without being false.
                    do { ++left; keysLeft = ref Unsafe.Add(ref keysLeft, 1); }
                    while (comparer.LessThan(keysLeft, pivot));

                    do { --right; keysRight = ref Unsafe.Add(ref keysRight, -1); }
                    while (comparer.LessThan(pivot, keysRight));
                }

                //if (Unsafe.AreSame(ref keysLeft, ref keysRight) ||
                //    Unsafe.IsAddressGreaterThan(ref keysLeft, ref keysRight))
                //    break;
                if (left >= right)
                    break;

                // PERF: Swap manually inlined here for better code-gen
                var t = keysLeft;
                keysLeft = keysRight;
                keysRight = t;
                // PERF: Swap manually inlined here for better code-gen
                ref var valuesLeft = ref Unsafe.Add(ref values, left);
                ref var valuesRight = ref Unsafe.Add(ref values, right);
                var v = valuesLeft;
                valuesLeft = valuesRight;
                valuesRight = v;
            }
            // Put pivot in the right location.
            right = nextToLast;
            if (left != right)
            {
                Swap(ref keys, left, right);
                Swap(ref values, left, right);
            }
            return left;
        }
    }
}