using System;

namespace DotNetCross.Sorting.Sequences
{
    public struct MedianOfThreeKillerSpanFiller : ISpanFiller
    {
        public void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue)
        {
            // Each slice must be median of three!
            int i = 0;
            for (; i < span.Length - sliceLength; i += sliceLength)
            {
                InitializeMedianOfThreeKiller(span.Slice(i, sliceLength), toValue);
            }
            // Fill remainder just to be sure
            InitializeMedianOfThreeKiller(span.Slice(i, span.Length - i), toValue);
        }

        public static void InitializeMedianOfThreeKiller<T>(Span<T> span, Func<int, T> toValue)
        {
            var length = span.Length;
            // if n is odd, set the last element to n-1, and proceed
            // with n decremented by 1
            if (length % 2 != 0)
            {
                span[length - 1] = toValue(length);
                --length;
            }
            var m = length / 2;
            for (int i = 0; i < m; ++i)
            {
                // first half of array (even indices)
                if (i % 2 == 0) span[i] = toValue(i + 1);
                // first half of array (odd indices)
                else span[i] = toValue(m + i + (m % 2 != 0 ? 1 : 0));
                // second half of array
                span[m + i] = toValue((i + 1) * 2);
            }
        }
    }
}
