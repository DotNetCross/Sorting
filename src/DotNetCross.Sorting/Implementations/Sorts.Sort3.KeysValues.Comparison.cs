using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        internal static partial class Comparison
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ref TKey Sort3<TKey, TValue>(
                ref TKey keys, ref TValue values, int i0, int i1, int i2,
                Comparison<TKey> comparison)

            {
                ref var r0 = ref Unsafe.Add(ref keys, i0);
                ref var r1 = ref Unsafe.Add(ref keys, i1);
                ref var r2 = ref Unsafe.Add(ref keys, i2);
                Sort2(ref r0, ref r1, comparison, ref values, i0, i1);
                Sort2(ref r0, ref r2, comparison, ref values, i0, i2);
                Sort2(ref r1, ref r2, comparison, ref values, i1, i2);
                return ref r1;
            }
        }
    }
}