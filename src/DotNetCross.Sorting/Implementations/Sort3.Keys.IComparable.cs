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
            internal static void Sort3<TKey>(
                ref TKey r0, ref TKey r1, ref TKey r2)
                where TKey : IComparable<TKey>
            {
                Sort2(ref r0, ref r1);
                Sort2(ref r0, ref r2);
                Sort2(ref r1, ref r2);

                // Below works but does not give exactly the same result as Array.Sort
                // i.e. order could be a bit different for keys that are equal
                //if (r0 != null && r0.CompareTo(r1) <= 0) //r0 <= r1)
                //{
                //    if (r1 != null && r1.CompareTo(r2) <= 0) //(r1 <= r2)
                //    {
                //        return;
                //    }
                //    else if (r0.CompareTo(r2) < 0) //(r0 < r2)
                //    {
                //        Swap(ref r1, ref r2);
                //    }
                //    else
                //    {
                //        TKey tmp = r0;
                //        r0 = r2;
                //        r2 = r1;
                //        r1 = tmp;
                //    }
                //}
                //else
                //{
                //    if (r0 != null && r0.CompareTo(r2) < 0) //(r0 < r2)
                //    {
                //        Swap(ref r0, ref r1);
                //    }
                //    else if (r2 != null && r2.CompareTo(r1) < 0) //(r2 < r1)
                //    {
                //        Swap(ref r0, ref r2);
                //    }
                //    else
                //    {
                //        TKey tmp = r0;
                //        r0 = r1;
                //        r1 = r2;
                //        r2 = tmp;
                //    }
                //}
            }
        }
    }
}
