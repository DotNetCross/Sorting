using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    //
    // API
    //
    //public static void Sort<T>(this Span<T> span);

    //public static void Sort<T, TComparer>(this Span<T> span, TComparer comparer)
    //   where TComparer : IComparer<T>;

    //public static void Sort<T>(this Span<T> span, System.Comparison<T> comparison);

    //public static void Sort<TKey, TValue>(this Span<TKey> keys, Span<TValue> items);

    //public static void Sort<TKey, TValue, TComparer>(this Span<TKey> keys,
    //   Span<TValue> items, TComparer comparer)
    //   where TComparer : IComparer<TKey>;

    //public static void Sort<TKey, TValue>(this Span<TKey> keys,
    //   Span<TValue> items, System.Comparison<TKey> comparison);

    public static class InsertionSort
    {
        public static void Sort<T, TComparer>(this Span<T> span, TComparer comparer)
           where TComparer : IComparer<T>
        {
            int s = 0;
            Sort(ref span.DangerousGetPinnableReference(), span.Length, comparer, ref s);
        }

        // TODO: Extend to IntPtr
        public static TSortStats Sort<T, TComparer, TSortStats>(ref T a, int length, TComparer comparer, ref TSortStats sortStats)
           where TComparer : IComparer<T>
        {
            if (length <= 1) { return sortStats; }
            var localLength = length;
            for (int j = 1; j < localLength; j++)
            {
                var key = Unsafe.Add(ref a, j);
                var i = j - 1;
                while (i >= 0 && comparer.Compare(Unsafe.Add(ref a, i), key) > 0)
                {
                    // Move (TODO: Add stats)
                    Unsafe.Add(ref a, i + 1) = Unsafe.Add(ref a, i);
                    --i;
                }
                // Assign
                Unsafe.Add(ref a, i + 1) = key;
            }
            return sortStats;
        }
    }
}
