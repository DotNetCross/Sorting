using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal partial class IntroKeysSortersComparable<TKey>
        : IKeysSorter<TKey>
        where TKey : IComparable<TKey>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InsertionSort(ref TKey keys, int lo, int hi)
        {
            Debug.Assert(lo >= 0);
            Debug.Assert(hi >= lo);

            for (int i = lo; i < hi; ++i)
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
                    while (--j >= lo && (t == null || t.CompareTo(keysAtJ) < 0));
                    keysAfterJ = t;
                }
            }
        }
    }
}