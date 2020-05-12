using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal static partial class KeysValuesSorter_TComparer
    {
        internal static int PickPivotAndPartition<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values, int length,
            TComparer comparer)
            where TComparer : IComparer<TKey>
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
            Swap(ref keysMiddle, ref Unsafe.Add(ref keys, right));
            Swap(ref values, middle, right);

            while (left < right)
            {
                // TODO: Would be good to be able to update local ref here

                while (left < nextToLast && comparer.Compare(Unsafe.Add(ref keys, ++left), pivot) < 0) ;
                // Check if bad comparable/comparer
                if (left == nextToLast && comparer.Compare(Unsafe.Add(ref keys, left), pivot) < 0)
                    ThrowHelper.ThrowArgumentException_BadComparer(comparer);

                while (right > 0 && comparer.Compare(pivot, Unsafe.Add(ref keys, --right)) < 0) ;
                // Check if bad comparable/comparer
                if (right == 0 && comparer.Compare(pivot, Unsafe.Add(ref keys, right)) < 0)
                    ThrowHelper.ThrowArgumentException_BadComparer(comparer);

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