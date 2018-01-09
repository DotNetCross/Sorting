using System.Runtime.CompilerServices;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    //public class RandomSortBench : SortBench<int>
    //{
    //    const int Seed = 213718398;

    //    public RandomSortBench()
    //        : base(maxLength: 3000000, new RandomSpanFiller(Seed), i => i)
    //    { }
    //}

    //public class RandomShuffleSortBench : SortBench<int>
    //{
    //    const double Fraction = 0.1;
    //    const int Seed = 931712983;
    //    public RandomShuffleSortBench()
    //        : base(maxLength: 3000000,
    //              new PartialRandomShuffleSpanFiller(new IncrementingSpanFiller(), Fraction, Seed), i => i)
    //    { }
    //}

    //public class MedianOfThreeSortBench : SortBench<int>
    //{
    //    public MedianOfThreeSortBench()
    //        : base(maxLength: 6000000, new MedianOfThreeKillerSpanFiller(), i => i)
    //    { }
    //}

    //public class IncrementingSortBench : SortBench<int>
    //{
    //    public IncrementingSortBench()
    //        : base(maxLength: 6000000, new IncrementingSpanFiller(), i => i)
    //    { }
    //}

    //public class DecrementingSortBench : SortBench<int>
    //{
    //    public DecrementingSortBench()
    //        : base(maxLength: 6000000, new DecrementingSpanFiller(), i => i)
    //    { }
    //}

    //public class ConstantSortBench : SortBench<int>
    //{
    //    public ConstantSortBench()
    //        : base(maxLength: 6000000, new ConstantSpanFiller(42), i => i)
    //    { }
    //}

    public class Int32SortBench : SortBench<int>
    {
        const int RandomSeed = 213718398;
        const double ShuffleFraction = 0.1;
        const int ShuffleSeed = 931712983;

        public Int32SortBench()
            : base(maxLength: 3000000, new ISpanFiller[]{
                new RandomSpanFiller(RandomSeed),
                new PartialRandomShuffleSpanFiller(new IncrementingSpanFiller(), ShuffleFraction, ShuffleSeed),
                // MEDIAN OF THREE REQUIRES SIZE FOR STEP! CAN'T JUST FILL MAXLENGTH!
                new MedianOfThreeKillerSpanFiller(),
                new IncrementingSpanFiller(),
                new DecrementingSpanFiller(),
                new ConstantSpanFiller(42),
            }, i => i)
        { }
    }

    public class Int32SortDisassemblerBenchNotWorking : SortDisassemblerBench//<int>
    {
        public Int32SortDisassemblerBenchNotWorking()
            : base(length: 1024 * 1024, i => i)
        { }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Int32SortBench>();

            //BenchmarkRunner.Run<Int32SortDisassemblerBenchNotWorking>(); // Fails?!
            BenchmarkRunner.Run<Int32SortDisassemblerBench>(); // Dissassembly isn't full...

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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void SomeMethod()
        {
            var s = new Int32SortDisassemblerBench();
            s.SpanSort();
        }
    }
}
