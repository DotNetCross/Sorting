using System;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting.Benchmarks
{
    public class ComparableClassInt32 : IComparable<ComparableClassInt32>
    {
        public readonly int Value;

        public ComparableClassInt32(int value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ComparableClassInt32 other)
        {
            return this.Value.CompareTo(other.Value);
        }
    }
}
