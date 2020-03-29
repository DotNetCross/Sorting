using System.Diagnostics;
using System.Runtime.CompilerServices;

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
                }
            }
        }
    }
}
