using System;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting.Benchmarks
{
    public struct ComparableStructInt32 : IComparable<ComparableStructInt32>
    {
        public readonly int Value;

        public ComparableStructInt32(int value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ComparableStructInt32 other)
        {
            return this.Value.CompareTo(other.Value);
        }
    }
}
