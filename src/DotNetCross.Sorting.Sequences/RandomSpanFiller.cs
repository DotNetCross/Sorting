using System;

namespace DotNetCross.Sorting.Sequences
{
    public struct RandomSpanFiller : ISpanFiller
    {
        readonly int _seed;

        public RandomSpanFiller(int seed)
        {
            _seed = seed;
        }

        public void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue)
        {
            var random = new Random(_seed);
            RandomFill(random, span, toValue);
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
