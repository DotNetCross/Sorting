using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    public static class SpanFillers
    {
        public const int RandomSeed = 213718398;
        public const double ShuffleFraction = 0.1;
        public const int ShuffleSeed = 931712983;

        public static readonly ISpanFiller[] RandomOnly = new[] 
        { 
            new RandomSpanFiller(RandomSeed) 
        };

        public static ISpanFiller[] All = new ISpanFiller[]
        {
            new RandomSpanFiller(RandomSeed),
            new PartialRandomShuffleSpanFiller(new IncrementingSpanFiller(), ShuffleFraction, ShuffleSeed),
            new MedianOfThreeKillerSpanFiller(),
            new IncrementingSpanFiller(),
            new DecrementingSpanFiller(),
            new ConstantSpanFiller(42),
        };

        public static ISpanFiller[] Default = RandomOnly; // All; // Usually All is default
    }
}
