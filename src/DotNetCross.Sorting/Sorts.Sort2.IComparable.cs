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
            internal static void Sort2<TKey>(
            ref TKey keys, int i, int j)
            where TKey : IComparable<TKey>
            {
                Debug.Assert(i != j);

                ref TKey a = ref Unsafe.Add(ref keys, i);
                ref TKey b = ref Unsafe.Add(ref keys, j);
                Sort2(ref a, ref b);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Sort2<TKey>(ref TKey a, ref TKey b)
                where TKey : IComparable<TKey>
            {
                if (a != null && a.CompareTo(b) > 0)
                {
                    TKey temp = a;
                    a = b;
                    b = temp;
                }
            }
        }
    }
}
