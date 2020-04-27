using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysValuesSorter_Comparable<TKey, TValue>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref TKey Sort3(
            ref TKey keys, ref TValue values, int i0, int i1, int i2)
        {
            ref var r0 = ref Unsafe.Add(ref keys, i0);
            ref var r1 = ref Unsafe.Add(ref keys, i1);
            ref var r2 = ref Unsafe.Add(ref keys, i2);
            Sort2(ref r0, ref r1, ref values, i0, i1);
            Sort2(ref r0, ref r2, ref values, i0, i2);
            Sort2(ref r1, ref r2, ref values, i1, i2);

            //ref var r0 = ref Unsafe.Add(ref keys, i0);
            //ref var r1 = ref Unsafe.Add(ref keys, i1);
            //ref var r2 = ref Unsafe.Add(ref keys, i2);

            //if (r0 != null && r0.CompareTo(r1) <= 0) //r0 <= r1)
            //{
            //    if (r1 != null && r1.CompareTo(r2) <= 0) //(r1 <= r2)
            //    {
            //        return ref r1;
            //    }
            //    else if (r0.CompareTo(r2) < 0) //(r0 < r2)
            //    {
            //        Swap(ref r1, ref r2);
            //        ref var v1 = ref Unsafe.Add(ref values, i1);
            //        ref var v2 = ref Unsafe.Add(ref values, i2);
            //        Swap(ref v1, ref v2);
            //    }
            //    else
            //    {
            //        TKey tmp = r0;
            //        r0 = r2;
            //        r2 = r1;
            //        r1 = tmp;
            //        ref var v0 = ref Unsafe.Add(ref values, i0);
            //        ref var v1 = ref Unsafe.Add(ref values, i1);
            //        ref var v2 = ref Unsafe.Add(ref values, i2);
            //        TValue vTemp = v0;
            //        v0 = v2;
            //        v2 = v1;
            //        v1 = vTemp;
            //    }
            //}
            //else
            //{
            //    if (r0 != null && r0.CompareTo(r2) < 0) //(r0 < r2)
            //    {
            //        Swap(ref r0, ref r1);
            //        ref var v0 = ref Unsafe.Add(ref values, i0);
            //        ref var v1 = ref Unsafe.Add(ref values, i1);
            //        Swap(ref v0, ref v1);
            //    }
            //    else if (r2 != null && r2.CompareTo(r1) < 0) //(r2 < r1)
            //    {
            //        Swap(ref r0, ref r2);
            //        ref var v0 = ref Unsafe.Add(ref values, i0);
            //        ref var v2 = ref Unsafe.Add(ref values, i2);
            //        Swap(ref v0, ref v2);
            //    }
            //    else
            //    {
            //        TKey tmp = r0;
            //        r0 = r1;
            //        r1 = r2;
            //        r2 = tmp;
            //        ref var v0 = ref Unsafe.Add(ref values, i0);
            //        ref var v1 = ref Unsafe.Add(ref values, i1);
            //        ref var v2 = ref Unsafe.Add(ref values, i2);
            //        TValue vTemp = v0;
            //        v0 = v1;
            //        v1 = v2;
            //        v2 = vTemp;
            //    }
            //}
            return ref r1;
        }
    }
}