using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.SpanSortHelpersCommon;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        internal static partial class TDirectComparer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void InsertionSort<TKey, TComparer>(
                ref TKey keys, int lo, int hi,
                TComparer comparer)
                where TComparer : IDirectComparer<TKey>
            {
                Debug.Assert(lo >= 0);
                Debug.Assert(hi >= lo);

                for (int i = lo; i < hi; ++i)
                {
                    int j = i;
                    //t = keys[i + 1];

                    //ref var keysAtJ = ref Unsafe.Add(ref keys, j);
                    //ref var keysAfterJ = ref Unsafe.Add(ref keysAtJ, 1);
                    //var t = keysAfterJ;
                    //while (j >= lo && comparer.LessThan(t, keysAtJ))
                    //{
                    //    keysAfterJ = keysAtJ;
                    //    keysAfterJ = ref keysAtJ;
                    //    keysAtJ = ref Unsafe.Subtract(ref keysAtJ, 1);
                    //    --j;
                    //}
                    //keysAfterJ = t;

                    // 3./4.
                    //ref var keysAtJ = ref Unsafe.Add(ref keys, j);
                    //ref var keysAfterJ = ref Unsafe.Add(ref keysAtJ, 1);
                    //var t = keysAfterJ;
                    //if (comparer.LessThan(t, keysAtJ))
                    //{
                    //    do
                    //    {
                    //        --j;
                    //        keysAfterJ = keysAtJ;
                    //        keysAfterJ = ref keysAtJ;
                    //        keysAtJ = ref Unsafe.Subtract(ref keysAtJ, 1);
                    //    }
                    //    while (j >= lo && comparer.LessThan(t, keysAtJ));
                    //    keysAfterJ = t;
                    //}

                    // 2.
                    ref var keysAtJ = ref Unsafe.Add(ref keys, j);
                    ref var keysAfterJ = ref Unsafe.Add(ref keysAtJ, 1);
                    var t = keysAfterJ;
                    if (comparer.LessThan(t, keysAtJ))
                    {
                        do
                        {
                            keysAfterJ = keysAtJ;
                            keysAfterJ = ref keysAtJ;
                            keysAtJ = ref Unsafe.Subtract(ref keysAtJ, 1);
                        }
                        while (--j >= lo && comparer.LessThan(t, keysAtJ));
                        keysAfterJ = t;
                    }

                    // 1.
                    //var t = Unsafe.Add(ref keys, j + 1);
                    //if (j >= lo && comparer.LessThan(t, Unsafe.Add(ref keys, j)))
                    //{
                    //    do
                    //    {
                    //        Unsafe.Add(ref keys, j + 1) = Unsafe.Add(ref keys, j);
                    //        --j;
                    //    }
                    //    while (j >= lo && comparer.LessThan(t, Unsafe.Add(ref keys, j)));
                    //    Unsafe.Add(ref keys, j + 1) = t;
                    //}
                }
            }
        }
    }
}
