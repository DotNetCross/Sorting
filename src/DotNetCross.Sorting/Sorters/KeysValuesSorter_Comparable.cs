using System;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysValuesSorter_Comparable<TKey, TValue>
        : IKeysValuesSorter<TKey, TValue>
        where TKey : IComparable<TKey>
    { }
}