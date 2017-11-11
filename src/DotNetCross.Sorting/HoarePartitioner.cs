using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    public struct HoarePartitioner : IPartitioner
    {
        public int LeftEndOffset => 0;

        public int Partition<T, TComparer>(ref T a, int lo, int hi, TComparer comparer)
            where TComparer : IComparer<T>
        {
            var pivot = Unsafe.Add(ref a, lo);
            var i = lo - 1;
            var j = hi + 1;
            while (true)
            {
                do
                {
                    ++i;
                } while (comparer.Compare(Unsafe.Add(ref a, i), pivot) < 0);
                do
                {
                    --j;
                } while (comparer.Compare(Unsafe.Add(ref a, j), pivot) > 0);

                if (i < j)
                {
                    Swap(ref Unsafe.Add(ref a, i), ref Unsafe.Add(ref a, j));
                }
                else
                {
                    return j;
                }
            }
        }
    }
}
