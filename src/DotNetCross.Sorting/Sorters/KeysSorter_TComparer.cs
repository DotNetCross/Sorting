using System.Collections.Generic;

namespace DotNetCross.Sorting
{
    internal partial struct KeysSorter_TComparer<TKey, TComparer>
        : IComparerKeysSorter<TKey, TComparer>
        where TComparer : IComparer<TKey>
    { }
}