using System;

namespace DotNetCross.Sorting.Sequences
{
    public struct RandomSpanFiller : ISpanFiller
    {
        readonly Random _random;

        public RandomSpanFiller(int seed)
        {
            _random = new Random(seed);
        }

        public void Fill<T>(Span<T> span, Func<int, T> toValue)
        {
            RandomFill(_random, span, toValue);
        }

        public static void RandomFill<T>(Random random, Span<T> span, Func<int, T> toValue)
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = toValue(random.Next());
            }
        }
    }
}
