using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        internal static partial class Comparison
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Sort2<TKey, TValue>(
                ref TKey keys, ref TValue values, int i, int j,
                Comparison<TKey> comparison)
            {
                Debug.Assert(i != j);

                ref TKey a = ref Unsafe.Add(ref keys, i);
                ref TKey b = ref Unsafe.Add(ref keys, j);
                Sort2(ref a, ref b, comparison, ref values, i, j);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Sort2<TKey, TValue>(
                ref TKey a, ref TKey b, Comparison<TKey> comparison,
                ref TValue values, int i, int j)

            {
                // This is one of the only places GreaterThan is needed
                // but we need to preserve this due to bogus comparers or similar
                if (comparison(a, b) > 0)
                {
                    Swap(ref a, ref b);
                    Swap(ref values, i, j);
                }
            }
        }
    }
}
