using System;

namespace DotNetCross.Sorting.Sequences
{
    public interface ISpanFiller
    {
        void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue);
    }

    public abstract class SpanFillerBase : ISpanFiller
    {
        public abstract void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue);

        public override string ToString() => 
            $"{this.GetType().Name.Replace("SpanFiller", "")}";
    }

    public class ConstantSpanFiller : SpanFillerBase
    {
        readonly int _fill;

        public ConstantSpanFiller(int fill)
        {
            _fill = fill;
        }

        public override void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue)
        {
            span.Fill(toValue(_fill));
        }
    }
    public class DecrementingSpanFiller : SpanFillerBase
    {
        public override void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue)
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
    public class ModuloDecrementingSpanFiller : SpanFillerBase
    {
        readonly int _modulo;

        public ModuloDecrementingSpanFiller(int modulo)
        {
            _modulo = modulo;
        }

        public override void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue)
        {
            ModuloFill(span, _modulo, toValue);
        }

        public static void ModuloFill<T>(Span<T> span, int modulo, Func<int, T> toValue)
        {
            for (int i = 0; i < span.Length; i++)
            {
                int v = (span.Length - i - 1) % modulo;
                span[i] = toValue(v);
            }
        }
    }
    public class IncrementingSpanFiller : SpanFillerBase
    {
        public override void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue)
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
    public class MedianOfThreeKillerSpanFiller : SpanFillerBase
    {
        public override void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue)
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
            var m = length >> 1;
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
    public class PartialRandomShuffleSpanFiller : SpanFillerBase
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

        public override void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue)
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
    public class RandomSpanFiller : SpanFillerBase
    {
        readonly int _seed;

        public RandomSpanFiller(int seed)
        {
            _seed = seed;
        }

        public override void Fill<T>(Span<T> span, int sliceLength, Func<int, T> toValue)
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
