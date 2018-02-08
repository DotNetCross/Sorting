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

        public static readonly ClassComparableComparer<TKey> Instance = new ClassComparableComparer<TKey>();
    }
    public struct StructComparableComparer<T> : IComparer<T>
        where T : IComparable<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(T x, T y) => x.CompareTo(y);
    }
    public static class ComparableComparison<TKey>
        where TKey : IComparable<TKey>
    {
        public static readonly Comparison<TKey> Instance = (x, y) => x.CompareTo(y);
    }
}
