using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting.Benchmarks
{
    public sealed class ClassComparableComparer<TKey> : IComparer<TKey>
        where TKey : IComparable<TKey>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(TKey x, TKey y) => x.CompareTo(y);
    }
    public struct ComparableComparer<T> : IComparer<T>
        where T : IComparable<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(T x, T y) => x.CompareTo(y);
    }
}
