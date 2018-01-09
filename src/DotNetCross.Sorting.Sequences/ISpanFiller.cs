using System;

namespace DotNetCross.Sorting.Sequences
{
    public interface ISpanFiller
    {
        void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue);
    }
}
