using System.Collections.Generic;

namespace DotNetCross.Sorting
{
    // Instead of IComparer better to have simple bool returning condition as would be faster and simpler
    // Depending on JIT/AOT

    // Normal single partioner
    public interface IPartitioner
    {
        int LeftEndOffset { get; }

        int Partition<T, TComparer>(ref T a, int lo, int hi, TComparer comparer)
            where TComparer : IComparer<T>;
    }

    // Dutch national flag/dual partioner TODO

}
