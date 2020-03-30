using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotNetCross.Sorting
{
    public static class InsertionSort
    {
        public static void Sort<T, TComparer>(Span<T> span, TComparer comparer)
           where TComparer : IComparer<T>
        {
            int s = 0;
            Sort(ref MemoryMarshal.GetReference(span), span.Length, comparer, ref s);
        }

        // TODO: Extend to IntPtr length(s)
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
