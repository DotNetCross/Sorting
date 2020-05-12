using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal static partial class KeysValuesSorter_Comparable
    {
        internal static int PickPivotAndPartition<TKey, TValue>(
            ref TKey keys, ref TValue values, int length,
            Comparison<TKey> comparison)

        {
            Debug.Assert(comparison != null);
            Debug.Assert(length > 2);
            //
            // Compute median-of-three.  But also partition them, since we've done the comparison.
            //
            // Sort left, middle and right appropriately, then pick middle as the pivot.
            int middle = (length - 1) >> 1;
            ref TKey keysMiddle = ref Sort3(ref keys, ref values, 0, middle, length - 1, comparison);

            TKey pivot = keysMiddle;

            int left = 0;
            int nextToLast = length - 2;
            int right = nextToLast;
            // We already partitioned lo and hi and put the pivot in hi - 1.  
            // And we pre-increment & decrement below.
            Swap(ref keysMiddle, ref Unsafe.Add(ref keys, right));
            Swap(ref values, middle, right);

            while (left < right)
            {
                // TODO: Would be good to be able to update local ref here

                // TODO: For primitives and internal comparers the range checks can be eliminated

                while (left < nextToLast && comparison(Unsafe.Add(ref keys, ++left), pivot) < 0) ;
                // Check if bad comparable/comparison
                if (left == nextToLast && comparison(Unsafe.Add(ref keys, left), pivot) < 0)
                    ThrowHelper.ThrowArgumentException_BadComparer(comparison);

                while (right > 0 && comparison(pivot, Unsafe.Add(ref keys, --right)) < 0) ;
                // Check if bad comparable/comparison
                if (right == 0 && comparison(pivot, Unsafe.Add(ref keys, right)) < 0)
                    ThrowHelper.ThrowArgumentException_BadComparer(comparison);

                if (left >= right)
                    break;

                Swap(ref keys, left, right);
                Swap(ref values, left, right);
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