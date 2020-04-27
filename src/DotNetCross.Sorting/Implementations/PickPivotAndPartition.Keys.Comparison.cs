using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal static partial class ComparisonImpl
    {
        internal static int PickPivotAndPartition<TKey>(
            ref TKey keys, int lo, int hi,
            Comparison<TKey> comparison)
        {
            ref var newStart = ref Unsafe.Add(ref keys, lo);
            var newLength = hi - lo + 1;
            //return PickPivotAndPartitionOld(ref keys, lo, hi, comparison);
            var p = PickPivotAndPartition(ref newStart, newLength, comparison);
            return p + lo;
        }

        // https://github.com/dotnet/runtime/pull/35297/files
        internal static int PickPivotAndPartition<TKey>(
            ref TKey keys, int length,
            Comparison<TKey> comparison)
        {
            //return PickPivotAndPartitionOld(ref keys, 0, length - 1, comparison);
            Debug.Assert(comparison != null);
            Debug.Assert(length > 2);

            int lo = 0;
            int hi = length - 1;
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

        internal static int PickPivotAndPartitionOld<TKey>(
            ref TKey keys, int lo, int hi,
            Comparison<TKey> comparison)
        {
            Debug.Assert(comparison != null);
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
            ref TKey keysLeft = ref Unsafe.Add(ref keys, lo);
            ref TKey keysMiddle = ref Unsafe.Add(ref keys, middle);
            ref TKey keysRight = ref Unsafe.Add(ref keys, hi);
            Sort3(ref keysLeft, ref keysMiddle, ref keysRight, comparison);

            TKey pivot = keysMiddle;

            int left = lo;
            int nextToLast = hi - 1;
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

                //Swap(ref keys, left, right);
                //while (left < nextToLast && comparison(Unsafe.Add(ref keys, ++left), pivot) < 0) ;
                //// Check if bad comparable/comparison
                //if (left == nextToLast && comparison(Unsafe.Add(ref keys, left), pivot) < 0)
                //    ThrowHelper.ThrowArgumentException_BadComparer(comparison);

                //while (right > lo && comparison(pivot, Unsafe.Add(ref keys, --right)) < 0) ;
                //// Check if bad comparable/comparison
                //if (right == lo && comparison(pivot, Unsafe.Add(ref keys, right)) < 0)
                //    ThrowHelper.ThrowArgumentException_BadComparer(comparison);

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