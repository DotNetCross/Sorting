﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal static partial class KeysValuesSorter_TDirectComparer<TKey, TValue, TComparer>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort2(
            ref TKey keys, ref TValue values, int i, int j,
            TComparer comparer)

        {
            Debug.Assert(i != j);

            ref TKey a = ref Unsafe.Add(ref keys, i);
            ref TKey b = ref Unsafe.Add(ref keys, j);
            Sort2(ref a, ref b, comparer, ref values, i, j);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort2(
            ref TKey a, ref TKey b, TComparer comparer,
            ref TValue values, int i, int j)

        {
            // This is one of the only places GreaterThan is needed
            // but we need to preserve this due to bogus comparers or similar
            if (comparer.GreaterThan(a, b))
            {
                Swap(ref a, ref b);
                Swap(ref values, i, j);
            }
        }
    }
}