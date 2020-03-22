using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        internal static partial class IComparable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void InsertionSort<TKey>(
                ref TKey keys, int lo, int hi)
                where TKey : IComparable<TKey>
            {
                Debug.Assert(lo >= 0);
                Debug.Assert(hi >= lo);

                for (int i = lo; i < hi; ++i)
                {
                    int j = i;
                    //t = keys[i + 1];
                    var t = Unsafe.Add(ref keys, j + 1);
                    // TODO: Would be good to be able to update local ref here
                    if (j >= lo && (t == null || t.CompareTo(Unsafe.Add(ref keys, j)) < 0))
                    {
                        do
                        {
                            Unsafe.Add(ref keys, j + 1) = Unsafe.Add(ref keys, j);
                            --j;
                        }
                        while (j >= lo && (t == null || t.CompareTo(Unsafe.Add(ref keys, j)) < 0));
                        //while (j >= lo && (t == null || t.CompareTo(keys[j]) < 0))

                        Unsafe.Add(ref keys, j + 1) = t;
                    }
                }
            }
        }
    }
}
