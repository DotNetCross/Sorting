using System;
using System.Collections.Generic;

namespace DotNetCross.Sorting
{
    internal interface IKeysSorter<TKey>
    {
        void IntroSort(ref TKey keys, int length);
        void IntroSort(ref TKey keys, int length, Comparison<TKey> comparison);
    }

    // TODO
    //internal interface IComparisonKeysSorter<TKey>
    //{
    //    void IntroSort(ref TKey keys, int length, Comparison<TKey> comparison);
    //}

    internal interface IKeysSorter<TKey, TComparer>
        where TComparer : IComparer<TKey>
    {
        void IntroSort(ref TKey keys, int length, TComparer comparer);
    }
}
