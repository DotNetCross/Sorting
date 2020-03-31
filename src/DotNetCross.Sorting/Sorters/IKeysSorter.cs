using System;
using System.Collections.Generic;

namespace DotNetCross.Sorting
{
    internal interface IKeysSorter<TKey>
    {
        void Sort(ref TKey keys, int length);
        void Sort(ref TKey keys, int length, Comparison<TKey> comparison);
    }

    internal interface IKeysSorter<TKey, TComparer>
        where TComparer : IComparer<TKey>
    {
        void Sort(ref TKey keys, int length, TComparer comparer);
    }
}
