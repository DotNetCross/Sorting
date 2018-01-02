using System;

namespace DotNetCross.Sorting.Sequences
{
    public interface ISpanFiller
    {
        void Fill<T>(Span<T> span, Func<int, T> toValue);
    }
}
