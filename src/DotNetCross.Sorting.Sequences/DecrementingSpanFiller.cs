using System;

namespace DotNetCross.Sorting.Sequences
{
    public struct DecrementingSpanFiller : ISpanFiller
    {
        public void Fill<T>(Span<T> span, Func<int, T> toValue)
        {
            DecrementingFill(span, toValue);
        }

        public static void DecrementingFill<T>(Span<T> span, Func<int, T> toValue)
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = toValue(span.Length - i - 1);
            }
        }
    }
}
