using System;
using System.Collections.Generic;

namespace DotNetCross.Sorting
{
    internal interface IKeysValuesSorter<TKey, TValue>
    {
        void IntroSort(ref TKey keys, ref TValue values, int length);
    }

    internal interface IComparisonKeysValuesSorter<TKey, TValue>
    {
        void IntroSort(ref TKey keys, ref TValue values, int length, Comparison<TKey> comparison);
    }

    internal interface IComparerKeysValuesSorter<TKey, TValue, TComparer>
        where TComparer : IComparer<TKey>
    {
        void IntroSort(ref TKey keys, ref TValue values, int length, TComparer comparer);
    }
}
