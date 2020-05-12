using System;
using SC = DotNetCross.Sorting.KeysValuesSorter_Comparable;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysValuesSorter_Comparable<TKey, TValue>
        : IKeysValuesSorter<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        public void IntroSort(ref TKey keys, ref TValue values, int length, Comparison<TKey> comparison)
        {
            // TODO: Check if comparison is Comparer<TKey>.Default.Compare

            SC.IntroSort(ref keys, ref values, length, comparison);
        }
    }
}