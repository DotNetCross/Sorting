using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal static partial class ComparisonImpl
    {
        // https://github.com/dotnet/runtime/pull/35297/files
        internal static int PickPivotAndPartition<TKey>(
            ref TKey keys, int length,
            Comparison<TKey> comparison)
        {
            Debug.Assert(comparison != null);
            Debug.Assert(length > 2);
            //
            // Compute median-of-three.  But also partition them, since we've done the comparison.
            //
            // Sort left, middle and right appropriately, then pick middle as the pivot.
            ref TKey keysLeft = ref keys;
            ref TKey keysMiddle = ref Unsafe.Add(ref keys, (length - 1) >> 1);
            ref TKey keysRight = ref Unsafe.Add(ref keys, length - 1);
            Sort3(ref keysLeft, ref keysMiddle, ref keysRight, comparison);

            TKey pivot = keysMiddle;

            int left = 0;
            int nextToLast = length - 2;
            int right = nextToLast;
            // We already partitioned lo and hi and put the pivot in hi - 1.  
            // And we pre-increment & decrement below.
            Swap(ref keysMiddle, ref Unsafe.Add(ref keys, right));

            while (left < right)
            {
                // TODO: Would be good to be able to update local ref here

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
            }
            // Put pivot in the right location.
            right = nextToLast;
            if (left != right)
            {
                Swap(ref keys, left, right);
            }
            return left;
        }
    }
}