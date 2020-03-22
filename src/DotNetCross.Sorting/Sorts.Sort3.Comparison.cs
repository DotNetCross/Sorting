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
            internal static void Sort3<TKey>(
                ref TKey r0, ref TKey r1, ref TKey r2,
                Comparison<TKey> comparison)

            {
                Sort2(ref r0, ref r1, comparison);
                Sort2(ref r0, ref r2, comparison);
                Sort2(ref r1, ref r2, comparison);

                // Below works but does not give exactly the same result as Array.Sort
                // i.e. order could be a bit different for keys that are equal
                //if (comparison.LessThanEqual(r0, r1)) 
                //{
                //    // r0 <= r1
                //    if (comparison.LessThanEqual(r1, r2)) 
                //    {
                //        // r0 <= r1 <= r2
                //        return; // Is this return good or bad for perf?
                //    }
                //    // r0 <= r1
                //    // r2 < r1
                //    else if (comparison.LessThanEqual(r0, r2)) 
                //    {
                //        // r0 <= r2 < r1
                //        Swap(ref r1, ref r2);
                //    }
                //    // r0 <= r1
                //    // r2 < r1
                //    // r2 < r0
                //    else
                //    {
                //        // r2 < r0 <= r1
                //        TKey tmp = r0;
                //        r0 = r2;
                //        r2 = r1;
                //        r1 = tmp;
                //    }
                //}
                //else 
                //{
                //    // r1 < r0
                //    if (comparison.LessThan(r2, r1)) 
                //    {
                //        // r2 < r1 < r0
                //        Swap(ref r0, ref r2);
                //    }
                //    // r1 < r0
                //    // r1 <= r2
                //    else if (comparison.LessThan(r2, r0)) 
                //    {
                //        // r1 <= r2 < r0
                //        TKey tmp = r0;
                //        r0 = r1;
                //        r1 = r2;
                //        r2 = tmp;
                //    }
                //    // r1 < r0
                //    // r1 <= r2
                //    // r0 <= r2
                //    else 
                //    {
                //        // r1 < r0 <= r2
                //        Swap(ref r0, ref r1);
                //    }
                //}
            }
        }
    }
}
