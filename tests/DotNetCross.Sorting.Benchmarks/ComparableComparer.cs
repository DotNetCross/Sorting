using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting.Benchmarks
{
    public struct ComparableComparer<T> : IComparer<T>
        where T : IComparable<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(T x, T y) => x.CompareTo(y);
    }
}
