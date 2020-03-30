using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal static partial class TComparerImpl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort2<TKey, TComparer>(
        ref TKey keys, int i, int j,
        TComparer comparer)
        where TComparer : IComparer<TKey>
        {
            Debug.Assert(i != j);

            ref TKey a = ref Unsafe.Add(ref keys, i);
            ref TKey b = ref Unsafe.Add(ref keys, j);
            Sort2(ref a, ref b, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort2<TKey, TComparer>(
            ref TKey a, ref TKey b, TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            // This is one of the only places GreaterThan is needed
            // but we need to preserve this due to bogus comparers or similar
            if (comparer.Compare(a, b) > 0)
            {
                TKey temp = a;
                a = b;
                b = temp;
            }
        }
    }
}