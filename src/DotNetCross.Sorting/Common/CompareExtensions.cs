using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DotNetCross.Sorting
{
    public static class CompareExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool LessThanEqual<TKey, TComparer>(
                this TComparer comparer, in TKey x, in TKey y)
                where TComparer : IComparer<TKey>
        {
            return comparer.Compare(x, y) <= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool LessThan<TKey, TComparer>(
                this TComparer comparer, in TKey x, in TKey y)
                where TComparer : IComparer<TKey>
        {
            return comparer.Compare(x, y) < 0;
        }
    }
}
