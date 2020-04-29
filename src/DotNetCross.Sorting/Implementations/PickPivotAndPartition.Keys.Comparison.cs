using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal partial class KeysSorter_Comparison<TKey>
    {
        // https://github.com/dotnet/runtime/pull/35297/files
        internal static int PickPivotAndPartition(
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
                           comparison(keysLeft = ref Unsafe.Add(ref keysLeft, 1), pivot) < 0) ;
                    if (Unsafe.AreSame(ref keysLeft, ref keysNextToLast) && comparison(keysLeft, pivot) < 0)
                        ThrowHelper.ThrowArgumentException_BadComparer(comparison);

                    while (Unsafe.IsAddressGreaterThan(ref keysRight, ref keys) && 
                           comparison(pivot, keysRight = ref Unsafe.Add(ref keysRight, -1)) < 0) ;
                    if (Unsafe.AreSame(ref keysRight, ref keys) && comparison(pivot, keysRight) < 0)
                        ThrowHelper.ThrowArgumentException_BadComparer(comparison);
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