using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysValuesSorter_Comparable<TKey, TValue>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InsertionSort(
            ref TKey keys, ref TValue values, int length)
        {
            for (int i = 0; i < length - 1; ++i)
            {
                int j = i;
                ref var keysAtJ = ref Unsafe.Add(ref keys, j);
                ref var keysAfterJ = ref Unsafe.Add(ref keysAtJ, 1);
                ref var valuesAtJ = ref Unsafe.Add(ref values, j);
                ref var valuesAfterJ = ref Unsafe.Add(ref valuesAtJ, 1);
                var t = keysAfterJ;
                var v = valuesAfterJ;
                if (t == null || t.CompareTo(keysAtJ) < 0)
                {
                    do
                    {
                        keysAfterJ = keysAtJ;
                        keysAfterJ = ref keysAtJ;
                        keysAtJ = ref Unsafe.Subtract(ref keysAtJ, 1);
                        valuesAfterJ = valuesAtJ;
                        valuesAfterJ = ref valuesAtJ;
                        valuesAtJ = ref Unsafe.Subtract(ref valuesAtJ, 1);
                    }
                    while (--j >= 0 && (t == null || t.CompareTo(keysAtJ) < 0));
                    keysAfterJ = t;
                    valuesAfterJ = v;
                }
            }
        }
    }
}