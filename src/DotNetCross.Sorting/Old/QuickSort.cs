using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DotNetCross.Sorting
{
    public static class QuickSort
    {
        public static void Sort<T, TComparer>(Span<T> keys, TComparer comparer)
           where TComparer : IComparer<T>
        {
            if (keys.IsEmpty) { return; }
            int s = 0;
            Sort(ref MemoryMarshal.GetReference(keys), 0, keys.Length - 1, 
                new HoarePartitioner(), comparer, ref s);
        }

        public static void Sort<T, TPartioner, TComparer>(Span<T> keys, TPartioner partioner, TComparer comparer)
           where TPartioner : IPartitioner
           where TComparer : IComparer<T>
        {
            if (keys.IsEmpty) { return; }
            int s = 0;
            Sort(ref MemoryMarshal.GetReference(keys), 0, keys.Length - 1,
                 partioner, comparer, ref s);
        }

        // TODO: Extend to IntPtr length(s)
        public static void Sort<T, TPartioner, TComparer, TSortStats>(
            ref T a, int lo, int hi, 
            TPartioner partioner, TComparer comparer, 
            ref TSortStats sortStats)
           where TPartioner : IPartitioner
           where TComparer : IComparer<T>
        {
            if (lo < hi)
            {
                var p = partioner.Partition(ref a, lo, hi, comparer);
                // Recursive so O(lg n) depth, stack space (can be improved)
                //Sort(ref a, lo,    p - 1, partioner, comparer, ref sortStats);
                Sort(ref a, lo, p + partioner.LeftEndOffset, partioner, comparer, ref sortStats);
                Sort(ref a, p + 1, hi,    partioner, comparer, ref sortStats);
            }
        }

    }
}
