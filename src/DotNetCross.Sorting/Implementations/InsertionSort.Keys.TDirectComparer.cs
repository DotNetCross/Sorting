using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal static partial class KeysSorter_TDirectComparer<TKey, TComparer>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InsertionSort(
            ref TKey keys, int length,
            TComparer comparer)
        {
            for (int i = 0; i < length - 1; ++i)
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
                    while (--j >= 0 && comparer.LessThan(t, keysAtJ));
                    keysAfterJ = t;
                }
            }
        }
    }
}