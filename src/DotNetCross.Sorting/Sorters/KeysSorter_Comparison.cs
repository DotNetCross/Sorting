using System;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysSorter_Comparison<TKey>
        : IKeysSorter<TKey>
    {
        public void IntroSort(ref TKey keys, int length)
        {
            throw new NotImplementedException();
        }

        //public void IntroSort(ref TKey keys, int length, Comparison<TKey> comparison)
        //{
        //    // TODO: Make member
        //    ComparisonImpl.IntroSort(ref keys, length, comparison);
        //}
    }
}