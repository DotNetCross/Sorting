using System;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysSorter_Comparable<TKey>
        : IKeysSorter<TKey>
        where TKey : IComparable<TKey>
    { }
}