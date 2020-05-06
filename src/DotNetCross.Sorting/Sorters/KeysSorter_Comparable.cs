using System;

namespace DotNetCross.Sorting
{
    internal partial class KeysSorter_Comparable<TKey>
        : IKeysSorter<TKey>
        where TKey : IComparable<TKey>
    { }
}