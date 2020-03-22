using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        internal static partial class TComparer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void InsertionSort<TKey, TComparer>(
                ref TKey keys, int lo, int hi,
                TComparer comparer)
                where TComparer : IComparer<TKey>
            {
                Debug.Assert(lo >= 0);
                Debug.Assert(hi >= lo);

                for (int i = lo; i < hi; ++i)
                {
                    int j = i;
                    //t = keys[i + 1];
                    var t = Unsafe.Add(ref keys, j + 1);
                    // TODO: Would be good to be able to update local ref here
                    if (j >= lo && comparer.Compare(t, Unsafe.Add(ref keys, j)) < 0)
                    {
                        do
                        {
                            Unsafe.Add(ref keys, j + 1) = Unsafe.Add(ref keys, j);
                            --j;
                        }
                        while (j >= lo && comparer.Compare(t, Unsafe.Add(ref keys, j)) < 0);

                        Unsafe.Add(ref keys, j + 1) = t;
                    }
                }
            }
        }
    }
}
