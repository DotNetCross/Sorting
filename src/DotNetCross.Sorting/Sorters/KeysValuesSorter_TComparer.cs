using System.Collections.Generic;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysValuesSorter_TComparer<TKey, TValue, TComparer>
        : IComparerKeysValuesSorter<TKey, TValue, TComparer>
        where TComparer : IComparer<TKey>
    { }
}