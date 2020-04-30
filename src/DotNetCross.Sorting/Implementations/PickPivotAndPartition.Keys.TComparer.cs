using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal partial class KeysSorter_TComparer<TKey, TComparer>
    {
        // https://sharplab.io/#v2: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=
        internal static int PickPivotAndPartition(
            ref TKey keys, int length,
            TComparer comparer)
        {
            Debug.Assert(comparer != null);
            Debug.Assert(length > 2);
            //
            // Compute median-of-three.  But also partition them, since we've done the comparison.
            //
            // Sort left, middle and right appropriately, then pick mid as the pivot.
            ref TKey keysLeft = ref keys;
            ref TKey keysMiddle = ref Unsafe.Add(ref keys, (length - 1) >> 1);
            ref TKey keysRight = ref Unsafe.Add(ref keys, length - 1);
            Sort3(ref keysLeft, ref keysMiddle, ref keysRight, comparer);

            TKey pivot = keysMiddle;

            ref var keysNextToLast = ref Unsafe.Add(ref keysRight, -1);
            keysRight = ref keysNextToLast;
            // We already partitioned lo and hi and put the pivot in hi - 1.  
            // And we pre-increment & decrement below.
            Swap(ref keysMiddle, ref keysRight);

            // Walk the left and right pointers, swapping elements as necessary, until they cross.
            while (Unsafe.IsAddressLessThan(ref keysLeft, ref keysRight))
            {
                //if (pivot == null)
                //{
                //    while (Unsafe.IsAddressLessThan(ref keysLeft, ref keysNextToLast) && 
                //           (keysLeft = ref Unsafe.Add(ref keysLeft, 1)) == null) ;
                //    while (Unsafe.IsAddressGreaterThan(ref keysRight, ref keys) && 
                //           (keysRight = ref Unsafe.Add(ref keysRight, -1)) == null) ;
                //}
                //else
                {
                    while (Unsafe.IsAddressLessThan(ref keysLeft, ref keysNextToLast) &&
                           comparer.Compare(keysLeft = ref Unsafe.Add(ref keysLeft, 1), pivot) < 0) ;
                    if (Unsafe.AreSame(ref keysLeft, ref keysNextToLast) && comparer.Compare(keysLeft, pivot) < 0)
                        ThrowHelper.ThrowArgumentException_BadComparer(comparer);

                    while (Unsafe.IsAddressGreaterThan(ref keysRight, ref keys) &&
                           comparer.Compare(pivot, keysRight = ref Unsafe.Add(ref keysRight, -1)) < 0) ;
                    if (Unsafe.AreSame(ref keysRight, ref keys) && comparer.Compare(pivot, keysRight) < 0)
                        ThrowHelper.ThrowArgumentException_BadComparer(comparer);
                }

                if (!Unsafe.IsAddressLessThan(ref keysLeft, ref keysRight))
                {
                    break;
                }

                // PERF: Swap manually inlined here for better code-gen
                var t = keysLeft;
                keysLeft = keysRight;
                keysRight = t;
            }

            // Put the pivot in the correct location.
            if (!Unsafe.AreSame(ref keysLeft, ref keysNextToLast))
            {
                Swap(ref keysLeft, ref keysNextToLast);
            }

            unsafe
            {
                if (sizeof(IntPtr) == 4)
                { return (int)Unsafe.ByteOffset(ref keys, ref keysLeft) / Unsafe.SizeOf<TKey>(); }
                else
                { return (int)((long)Unsafe.ByteOffset(ref keys, ref keysLeft) / Unsafe.SizeOf<TKey>()); }
            }
        }
    }
}