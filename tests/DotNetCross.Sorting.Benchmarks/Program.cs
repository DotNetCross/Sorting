using System.Runtime.CompilerServices;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    public static class SpanFillers
    {
        const int RandomSeed = 213718398;
        const double ShuffleFraction = 0.1;
        const int ShuffleSeed = 931712983;

        public static ISpanFiller[] Default = new ISpanFiller[]{
                new RandomSpanFiller(RandomSeed),
                new PartialRandomShuffleSpanFiller(new IncrementingSpanFiller(), ShuffleFraction, ShuffleSeed),
                new MedianOfThreeKillerSpanFiller(),
                new IncrementingSpanFiller(),
                new DecrementingSpanFiller(),
                new ConstantSpanFiller(42),
            };
    }

    public class Int32SortBench : SortBench<int>
    {
        public Int32SortBench()
            : base(maxLength: 3000000, SpanFillers.Default, i => i)
        { }
    }

    public class Int32Int32SortBench : SortBench<int, int>
    {
        public Int32Int32SortBench()
            : base(maxLength: 3000000, SpanFillers.Default, i => i, i => i)
        { }
    }

    public class Int32SortDisassemblerBenchNotWorking : SortDisassemblerBench<int>
    {
        public Int32SortDisassemblerBenchNotWorking()
            : base(length: 1024 * 1024, i => i)
        { }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<Int32SortBench>();

            //BenchmarkRunner.Run<Int32Int32SortBench>();

            BenchmarkRunner.Run<Int32SortDisassemblerBench>();

            //BenchmarkRunner.Run<Int32SortDisassemblerBenchNotWorking>(); // Fails?!
            //BenchmarkRunner.Run<RandomSort>();

            //BenchmarkRunner.Run<RandomShuffleSortBench>();
            //BenchmarkRunner.Run<RandomSortBench>();
            //BenchmarkRunner.Run<MedianOfThreeSortBench>();
            //BenchmarkRunner.Run<IncrementingSortBench>();
            //BenchmarkRunner.Run<DecrementingSortBench>();
            //BenchmarkRunner.Run<ConstantSortBench>();

            //BenchmarkRunner.Run<IntPtrHelperBenchmark>();
            //BenchmarkRunner.Run<CompareAsm>();

            //SomeMethod();
            //var sut = new RandomSort();
            //for (int i = 0; i < 150; i++)
            //{
            //    sut.IterationSetup();
            //    sut.SpanSort();
            //}
        }
    }
}
