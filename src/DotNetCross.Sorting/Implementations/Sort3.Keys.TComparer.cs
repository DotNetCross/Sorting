﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    internal partial struct KeysSorter_TComparer<TKey, TComparer>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort3(
            ref TKey r0, ref TKey r1, ref TKey r2,
            TComparer comparer)
        {
            Sort2(ref r0, ref r1, comparer);
            Sort2(ref r0, ref r2, comparer);
            Sort2(ref r1, ref r2, comparer);

            // Below works but does not give exactly the same result as Array.Sort
            // i.e. order could be a bit different for keys that are equal or
            // order different for for BogusComparer
            //if (comparer.LessThanEqual(r0, r1))
            //{
            //    // r0 <= r1
            //    if (comparer.LessThanEqual(r1, r2))
            //    {
            //        // r0 <= r1 <= r2
            //    }
            //    // r0 <= r1
            //    // r2 < r1
            //    else if (comparer.LessThanEqual(r0, r2))
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
            //    if (comparer.LessThan(r2, r1))
            //    {
            //        // r2 < r1 < r0
            //        Swap(ref r0, ref r2);
            //    }
            //    // r1 < r0
            //    // r1 <= r2
            //    else if (comparer.LessThan(r2, r0))
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