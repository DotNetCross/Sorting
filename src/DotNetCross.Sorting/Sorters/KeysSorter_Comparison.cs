using System;

namespace DotNetCross.Sorting
{
    internal sealed partial class KeysSorter_Comparison<TKey>
        : IKeysSorter<TKey>
    {
        // TODO: Add new interface and remove this
        public void IntroSort(ref TKey keys, int length)
        {
            throw new NotImplementedException();
        }
    }
}