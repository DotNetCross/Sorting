using System;
using System.Collections.Generic;

namespace DotNetCross.Sorting
{
    internal interface IKeysSorter<TKey>
    {
        void IntroSort(ref TKey keys, int length);
    }

    internal interface IComparisonKeysSorter<TKey>
    {
        void IntroSort(ref TKey keys, int length, Comparison<TKey> comparison);
    }

    internal interface IComparerKeysSorter<TKey, TComparer>
        where TComparer : IComparer<TKey>
    {
        void IntroSort(ref TKey keys, int length, TComparer comparer);
    }
}
