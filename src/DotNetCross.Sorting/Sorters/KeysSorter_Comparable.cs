using System;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysSorter_Comparable<TKey>
        : IKeysSorter<TKey>
        where TKey : IComparable<TKey>
    {
        public void IntroSort(ref TKey keys, int length, Comparison<TKey> comparison)
        {
            // TODO: Check if comparison is Comparer<TKey>.Default.Compare
            //       and if reference type or not
            // TODO: Make member
            ComparisonImpl.IntroSort(ref keys, length, comparison);
        }
    }
}