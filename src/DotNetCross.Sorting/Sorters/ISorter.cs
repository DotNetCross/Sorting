using System;
using System.Collections.Generic;

namespace DotNetCross.Sorting
{
    internal interface ISorter<TKey>
    {
        void Sort(ref TKey keys, int length);
        void Sort(ref TKey keys, int length, Comparison<TKey> comparison);
    }

    // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
    internal interface ISorter<TKey, TComparer>
        where TComparer : IComparer<TKey>
    {
        void Sort(ref TKey keys, int length, TComparer comparer);
    }
}
