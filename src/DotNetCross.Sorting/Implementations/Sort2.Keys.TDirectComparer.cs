using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal static partial class KeysSorter_TDirectComparer<TKey, TComparer>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort2(
            ref TKey keys, int i, int j,
            TComparer comparer)
        {
            Debug.Assert(i != j);

            ref TKey a = ref Unsafe.Add(ref keys, i);
            ref TKey b = ref Unsafe.Add(ref keys, j);
            Sort2(ref a, ref b, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort2(
            ref TKey a, ref TKey b, TComparer comparer)
        {
            // This is one of the only places GreaterThan is needed
            // but we need to preserve this due to bogus comparers or similar
            if (comparer.GreaterThan(a, b))
            {
                TKey temp = a;
                a = b;
                b = temp;
            }
        }
    }
}