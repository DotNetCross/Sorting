using System;
using System.Collections.Generic;

namespace DotNetCross.Sorting
{
    internal interface IKeysValuesSorter<TKey, TValue>
    {
        void Sort(ref TKey keys, ref TValue values, int length);
        void Sort(ref TKey keys, ref TValue values, int length, Comparison<TKey> comparison);
    }

    internal interface IKeysValuesSorter<TKey, TValue, TComparer>
        where TComparer : IComparer<TKey>
    {
        void Sort(ref TKey keys, ref TValue values, int length, TComparer comparer);
    }
}
