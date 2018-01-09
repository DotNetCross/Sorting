using System;

namespace DotNetCross.Sorting.Sequences
{
    public struct IncrementingSpanFiller : ISpanFiller
    {
        public void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue)
        {
            IncrementingFill(span, toValue);
        }

        public static void IncrementingFill<T>(Span<T> span, Func<int, T> toValue)
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = toValue(i);
            }
        }
    }
}
