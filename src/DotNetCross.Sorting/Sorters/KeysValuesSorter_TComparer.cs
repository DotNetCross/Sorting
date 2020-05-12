using System;
using System.Collections.Generic;
//using SC = DotNetCross.Sorting.KeysValuesSorter_Comparable;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysValuesSorter_TComparer<TKey, TValue, TComparer>
        : IComparerKeysValuesSorter<TKey, TValue, TComparer>
        where TComparer : IComparer<TKey>
    { }
}