using System;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysSorter_Comparable<TKey>
        : IKeysSorter<TKey>
        where TKey : IComparable<TKey>
    {
        internal static readonly KeysSorter_Comparison<TKey> ComparisonInstance = new KeysSorter_Comparison<TKey>();

        // TODO: Remove here, make new interfaces etc.
        public void IntroSort(ref TKey keys, int length, Comparison<TKey> comparison)
        {
            // TODO: Check if comparison is Comparer<TKey>.Default.Compare
            //       and if reference type or not
            ComparisonInstance.IntroSort(ref keys, length, comparison);
        }
    }
}