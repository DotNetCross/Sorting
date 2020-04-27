using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal static partial class ComparisonImpl
    {
        internal static void InsertionSort<TKey>(
            ref TKey keys, int lo, int hi,
            Comparison<TKey> comparison)
        {
            ref var newStart = ref Unsafe.Add(ref keys, lo);
            var newLength = hi - lo + 1;
            //InsertionSortOld(ref keys, lo, hi, comparison);
            InsertionSort(ref newStart, newLength, comparison);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InsertionSortOld<TKey>(
             ref TKey keys, int lo, int hi,
             Comparison<TKey> comparison)
        {
            Debug.Assert(lo >= 0);
            Debug.Assert(hi >= lo);

            for (int i = lo; i < hi; ++i)
            {
                int j = i;
                ref var keysAtJ = ref Unsafe.Add(ref keys, j);
                ref var keysAfterJ = ref Unsafe.Add(ref keysAtJ, 1);
                var t = keysAfterJ;
                if (comparison(t, keysAtJ) < 0)
                {
                    do
                    {
                        keysAfterJ = keysAtJ;
                        keysAfterJ = ref keysAtJ;
                        keysAtJ = ref Unsafe.Subtract(ref keysAtJ, 1);
                    }
                    while (--j >= lo && comparison(t, keysAtJ) < 0);
                    keysAfterJ = t;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InsertionSort<TKey>(
             ref TKey keys, int length,
             Comparison<TKey> comparison)
        {
            for (int i = 0; i < length - 1; ++i)
            {
                int j = i;
                ref var keysAtJ = ref Unsafe.Add(ref keys, j);
                ref var keysAfterJ = ref Unsafe.Add(ref keysAtJ, 1);
                var t = keysAfterJ;
                if (comparison(t, keysAtJ) < 0)
                {
                    do
                    {
                        keysAfterJ = keysAtJ;
                        keysAfterJ = ref keysAtJ;
                        keysAtJ = ref Unsafe.Subtract(ref keysAtJ, 1);
                    }
                    while (--j >= 0 && comparison(t, keysAtJ) < 0);
                    keysAfterJ = t;
                }
            }
        }

    }
}