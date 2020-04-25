using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal partial class KeysSorter_Comparable<TKey>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort2(ref TKey keys, int i, int j)
        {
            Debug.Assert(i != j);

            ref TKey a = ref Unsafe.Add(ref keys, i);
            ref TKey b = ref Unsafe.Add(ref keys, j);
            Sort2(ref a, ref b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort2(ref TKey a, ref TKey b)
        {
            if (a != null && a.CompareTo(b) > 0)
            {
                TKey temp = a;
                a = b;
                b = temp;
            }
        }
    }
}