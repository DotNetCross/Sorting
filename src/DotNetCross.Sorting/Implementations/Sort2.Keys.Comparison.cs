using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal partial class KeysSorter_Comparison<TKey>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort2(
        ref TKey keys, int i, int j,
        Comparison<TKey> comparison)
        {
            Debug.Assert(i != j);

            ref TKey a = ref Unsafe.Add(ref keys, i);
            ref TKey b = ref Unsafe.Add(ref keys, j);
            Sort2(ref a, ref b, comparison);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort2(
            ref TKey a, ref TKey b,
            Comparison<TKey> comparison)
        {
            // This is one of the only places GreaterThan is needed
            // but we need to preserve this due to bogus comparers or similar
            if (comparison(a, b) > 0)
            {
                TKey temp = a;
                a = b;
                b = temp;
            }
        }
    }
}