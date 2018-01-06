using System;

namespace DotNetCross.Sorting.Sequences
{
    public struct PartialRandomShuffleSpanFiller : ISpanFiller
    {
        readonly ISpanFiller _spanFiller;
        readonly double _fractionRandomShuffles;
        readonly Random _random;

        public PartialRandomShuffleSpanFiller(ISpanFiller spanFiller, double fractionRandomShuffles, int seed)
        {
            _spanFiller = spanFiller;
            _fractionRandomShuffles = fractionRandomShuffles;
            _random = new Random(seed);
        }

        public void Fill<T>(Span<T> span, Func<int, T> toValue)
        {
            _spanFiller.Fill(span, toValue);

            RandomShuffle(span, _fractionRandomShuffles);
        }

        private void RandomShuffle<T>(Span<T> span, double fractionRandomShuffles)
        {
            int shuffleCount = Math.Max(0, (int)(span.Length * fractionRandomShuffles));
            for (int i = 0; i < shuffleCount; i++)
            {
                var a = _random.Next(span.Length);
                var b = _random.Next(span.Length);
                var temp = span[a];
                span[a] = span[b];
                span[b] = temp;
            }
        }
    }
}
