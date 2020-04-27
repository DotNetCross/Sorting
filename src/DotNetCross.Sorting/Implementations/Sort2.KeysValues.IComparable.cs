using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysValuesSorter_Comparable<TKey, TValue>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort2(
            ref TKey keys, ref TValue values, int i, int j)
        {
            Debug.Assert(i != j);

            ref TKey a = ref Unsafe.Add(ref keys, i);
            ref TKey b = ref Unsafe.Add(ref keys, j);
            Sort2(ref a, ref b, ref values, i, j);
        }

        internal static void Sort2(
            ref TKey a, ref TKey b, ref TValue values, int i, int j)
        {
            if (a != null && a.CompareTo(b) > 0)
            {
                Swap(ref a, ref b);
                Swap(ref values, i, j);
            }
        }
    }
}