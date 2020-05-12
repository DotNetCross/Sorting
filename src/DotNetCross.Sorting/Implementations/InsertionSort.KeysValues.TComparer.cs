using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal static partial class KeysValuesSorter_TComparer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InsertionSort<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values, int length,
            TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            for (int i = 0; i < length - 1; ++i)
            {
                int j = i;
                //t = keys[i + 1];
                var t = Unsafe.Add(ref keys, j + 1);
                // TODO: Would be good to be able to update local ref here
                if (j >= 0 && comparer.Compare(t, Unsafe.Add(ref keys, j)) < 0)
                {
                    var v = Unsafe.Add(ref values, j + 1);
                    do
                    {
                        Unsafe.Add(ref keys, j + 1) = Unsafe.Add(ref keys, j);
                        Unsafe.Add(ref values, j + 1) = Unsafe.Add(ref values, j);
                        --j;
                    }
                    while (j >= 0 && comparer.Compare(t, Unsafe.Add(ref keys, j)) < 0);

                    Unsafe.Add(ref keys, j + 1) = t;
                    Unsafe.Add(ref values, j + 1) = v;
                }
            }
        }
    }
}