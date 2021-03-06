﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal partial class KeysValuesSorter_Comparison<TKey, TValue>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InsertionSort(
            ref TKey keys, ref TValue values, int length,
            Comparison<TKey> comparison)

        {
            for (int i = 0; i < length - 1; ++i)
            {
                int j = i;
                //t = keys[i + 1];
                var t = Unsafe.Add(ref keys, j + 1);
                // TODO: Would be good to be able to update local ref here
                if (j >= 0 && comparison(t, Unsafe.Add(ref keys, j)) < 0)
                {
                    var v = Unsafe.Add(ref values, j + 1);
                    do
                    {
                        Unsafe.Add(ref keys, j + 1) = Unsafe.Add(ref keys, j);
                        Unsafe.Add(ref values, j + 1) = Unsafe.Add(ref values, j);
                        --j;
                    }
                    while (j >= 0 && comparison(t, Unsafe.Add(ref keys, j)) < 0);

                    Unsafe.Add(ref keys, j + 1) = t;
                    Unsafe.Add(ref values, j + 1) = v;
                }
            }
        }
    }
}