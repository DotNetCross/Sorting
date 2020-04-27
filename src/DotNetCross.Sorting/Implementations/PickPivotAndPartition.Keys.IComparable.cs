using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal partial class KeysSorter_Comparable<TKey>
    {
        internal int PickPivotAndPartition(
            ref TKey keys, int lo, int hi)
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
            ref TKey keysLeft = ref Unsafe.Add(ref keys, lo);
            ref TKey keysMiddle = ref Unsafe.Add(ref keys, middle);
            ref TKey keysRight = ref Unsafe.Add(ref keys, hi);
            Sort3(ref keysLeft, ref keysMiddle, ref keysRight);

            TKey pivot = keysMiddle;

            // We already partitioned lo and hi and put the pivot in hi - 1.  
            // And we pre-increment & decrement below.
            keysRight = ref Unsafe.Add(ref keysRight, -1);
            Swap(ref keysMiddle, ref keysRight);

            int left = lo;
            int right = hi - 1;
            while (left < right)
            {
                if (pivot == null)
                {
                    do { ++left; keysLeft = ref Unsafe.Add(ref keysLeft, 1); }
                    while (left < right && keysLeft == null);

                    do { --right; keysRight = ref Unsafe.Add(ref keysRight, -1); }
                    while (right > lo && keysRight != null);
                }
                else
                {
                    do { ++left; keysLeft = ref Unsafe.Add(ref keysLeft, 1); }
                    while (left < right && pivot.CompareTo(keysLeft) > 0);
                    // Check if bad comparable/comparer
                    if (left == right && pivot.CompareTo(keysLeft) > 0)
                        ThrowHelper.ThrowArgumentException_BadComparable(typeof(TKey));

                    do { --right; keysRight = ref Unsafe.Add(ref keysRight, -1); }
                    while (right > lo && pivot.CompareTo(keysRight) < 0);
                    // Check if bad comparable/comparer
                    if (right == lo && pivot.CompareTo(keysRight) < 0)
                        ThrowHelper.ThrowArgumentException_BadComparable(typeof(TKey));
                }

                if (left >= right)
                    break;

                // PERF: Swap manually inlined here for better code-gen
                var t = keysLeft;
                keysLeft = keysRight;
                keysRight = t;
            }
            // Put pivot in the right location.
            right = hi - 1;
            if (left != right)
            {
                Swap(ref keys, left, right);
            }
            return left;
        }

        // Appears Stephen Toub finally added the unsafe version in, which means my original code could have gotten in anyway :|
        // https://github.com/dotnet/runtime/pull/35297/
        // https://github.com/dotnet/runtime/blob/8c39b7ec3781af3b3f1b36ce3eeccbb15b3dfb32/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/ArraySortHelper.cs#L453-L499 
        // ArraySortHelper
        //private static int PickPivotAndPartition(Span<T> keys)
        //{
        //    Debug.Assert(keys.Length >= Array.IntrosortSizeThreshold);
        //
        //
        //    // Use median-of-three to select a pivot. Grab a reference to the 0th, Length-1th, and Length/2th elements, and sort them.
        //    ref T zeroRef = ref MemoryMarshal.GetReference(keys);
        //    ref T lastRef = ref Unsafe.Add(ref zeroRef, keys.Length - 1);
        //    ref T middleRef = ref Unsafe.Add(ref zeroRef, (keys.Length - 1) >> 1);
        //    SwapIfGreater(ref zeroRef, ref middleRef);
        //    SwapIfGreater(ref zeroRef, ref lastRef);
        //    SwapIfGreater(ref middleRef, ref lastRef);
        //
        //
        //    // Select the middle value as the pivot, and move it to be just before the last element.
        //    ref T nextToLastRef = ref Unsafe.Add(ref zeroRef, keys.Length - 2);
        //    T pivot = middleRef;
        //    Swap(ref middleRef, ref nextToLastRef);
        //
        //
        //    // Walk the left and right pointers, swapping elements as necessary, until they cross.
        //    ref T leftRef = ref zeroRef, rightRef = ref nextToLastRef;
        //    while (Unsafe.IsAddressLessThan(ref leftRef, ref rightRef))
        //    {
        //        if (pivot == null)
        //        {
        //            while (Unsafe.IsAddressLessThan(ref leftRef, ref nextToLastRef) && (leftRef = ref Unsafe.Add(ref leftRef, 1)) == null) ;
        //            while (Unsafe.IsAddressGreaterThan(ref rightRef, ref zeroRef) && (rightRef = ref Unsafe.Add(ref rightRef, -1)) == null) ;
        //        }
        //        else
        //        {
        //            while (Unsafe.IsAddressLessThan(ref leftRef, ref nextToLastRef) && pivot.CompareTo(leftRef = ref Unsafe.Add(ref leftRef, 1)) > 0) ;
        //            while (Unsafe.IsAddressGreaterThan(ref rightRef, ref zeroRef) && pivot.CompareTo(rightRef = ref Unsafe.Add(ref rightRef, -1)) < 0) ;
        //        }
        //
        //
        //        if (!Unsafe.IsAddressLessThan(ref leftRef, ref rightRef))
        //        {
        //            break;
        //        }
        //
        //
        //        Swap(ref leftRef, ref rightRef);
        //    }
        //
        //
        //    // Put the pivot in the correct location.
        //    if (!Unsafe.AreSame(ref leftRef, ref nextToLastRef))
        //    {
        //        Swap(ref leftRef, ref nextToLastRef);
        //    }
        //    return (int)((nint)Unsafe.ByteOffset(ref zeroRef, ref leftRef) / Unsafe.SizeOf<T>());
        //}

    }
}