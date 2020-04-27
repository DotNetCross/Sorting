using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal partial class KeysSorter_Comparable<TKey>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InsertionSort(ref TKey keys, int length)
        {
            for (int i = 0; i < length - 1; ++i)
            {
                int j = i;
                ref var keysAtJ = ref Unsafe.Add(ref keys, j);
                ref var keysAfterJ = ref Unsafe.Add(ref keysAtJ, 1);
                var t = keysAfterJ;
                if (t == null || t.CompareTo(keysAtJ) < 0)
                {
                    do
                    {
                        keysAfterJ = keysAtJ;
                        keysAfterJ = ref keysAtJ;
                        keysAtJ = ref Unsafe.Subtract(ref keysAtJ, 1);
                    }
                    while (--j >= 0 && (t == null || t.CompareTo(keysAtJ) < 0));
                    keysAfterJ = t;
                }
            }
        }
    }
}