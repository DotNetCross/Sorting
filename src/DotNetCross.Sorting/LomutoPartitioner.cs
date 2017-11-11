using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    // TODO: Make possible to inject pivot selector...
    public struct LomutoPartitioner : IPartitioner
    {
        public int LeftEndOffset => -1;

        public int Partition<T, TComparer>(ref T a, int lo, int hi, TComparer comparer)
            where TComparer : IComparer<T>
        {
            ref var a_hi = ref Unsafe.Add(ref a, hi);
            var pivot = a_hi;
            var i = lo;
            for (int j = lo; j < hi; j++)
            {
                ref var a_j = ref Unsafe.Add(ref a, j);
                var c = comparer.Compare(a_j, pivot);
                // Some say just less than, but text is less than or equal
                if (c <= 0)
                {
                    ref var a_i = ref Unsafe.Add(ref a, i);
                    Swap(ref a_i, ref a_j);
                    ++i;
                }
            }
            var partition = i;
            ref var a_p = ref Unsafe.Add(ref a, partition);
            // Some sources say a compare is needed here, but not needed
            //if (comparer.Compare(a_hi, a_p) <= 0)
            {
                Swap(ref a_p, ref a_hi);
            }
            return partition;
        }
    }
}
