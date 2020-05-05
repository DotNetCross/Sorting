using System;
using System.Collections.Generic;

namespace DotNetCross.Sorting
{
    internal partial class KeysSorter_TComparer<TKey, TComparer>
        : IComparerKeysSorter<TKey, TComparer>
        where TComparer : IComparer<TKey>
    { }
}