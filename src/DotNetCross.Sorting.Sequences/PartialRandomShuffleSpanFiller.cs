using System;

namespace DotNetCross.Sorting.Sequences
{
    public class PartialRandomShuffleSpanFiller : ISpanFiller
    {
        readonly ISpanFiller _spanFiller;
        readonly double _fractionRandomShuffles;
        readonly int _seed;

        public PartialRandomShuffleSpanFiller(ISpanFiller spanFiller, double fractionRandomShuffles, int seed)
        {
            _spanFiller = spanFiller;
            _fractionRandomShuffles = fractionRandomShuffles;
            _seed = seed;
        }

        public void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue)
        {
            _spanFiller.Fill(span, sliceLength, toValue);

            RandomShuffle(span, _fractionRandomShuffles);
        }

        private void RandomShuffle<T>(Span<T> span, double fractionRandomShuffles)
        {
            var random = new Random(_seed);
            int shuffleCount = Math.Max(0, (int)(span.Length * fractionRandomShuffles));
            for (int i = 0; i < shuffleCount; i++)
            {
                var a = random.Next(span.Length);
                var b = random.Next(span.Length);
                var temp = span[a];
                span[a] = span[b];
                span[b] = temp;
            }
        }
    }
}
