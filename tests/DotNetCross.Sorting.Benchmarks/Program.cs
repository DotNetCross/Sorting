using System;
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

    public struct ComparableStructInt32 : IComparable<ComparableStructInt32>
    {
        public readonly int Value;

        public ComparableStructInt32(int value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ComparableStructInt32 other)
        {
            return this.Value.CompareTo(other.Value);
        }
    }

    public class Int32SortBench : SortBench<int>
    {
        public Int32SortBench()
            : base(maxLength: 3000000, SpanFillers.Default, i => i)
        { }
    }
    public class SingleSortBench : SortBench<float>
    {
        public SingleSortBench()
            : base(maxLength: 3000000, SpanFillers.Default, i => i)
        { }
    }
    public class StringSortBench : SortBench<string>
    {
        public StringSortBench()
            : base(maxLength: 3000000, SpanFillers.Default, i => i.ToString("D9"))
        { }
    }
    public class ComparableStructInt32SortBench : SortBench<ComparableStructInt32>
    {
        public ComparableStructInt32SortBench()
            : base(maxLength: 3000000, SpanFillers.Default, i => new ComparableStructInt32(i))
        { }
    }

    public class Int32Int32SortBench : SortBench<int, int>
    {
        public Int32Int32SortBench()
            : base(maxLength: 3000000, SpanFillers.Default, i => i, i => i)
        { }
    }
    public class Int32SingleSortBench : SortBench<int, float>
    {
        public Int32SingleSortBench()
            : base(maxLength: 3000000, SpanFillers.Default, i => i, i => i)
        { }
    }
    public class Int32StringSortBench : SortBench<int, string>
    {
        public Int32StringSortBench()
            : base(maxLength: 3000000, SpanFillers.Default, i => i, i => i.ToString("D9"))
        { }
    }

    // BDN fails
    //public class Int32SortDisassemblerBenchNotWorking : SortDisassemblerBench<int>
    //{
    //    public Int32SortDisassemblerBenchNotWorking()
    //        : base(length: 1024 * 1024, i => i)
    //    { }
    //}

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Int32SortBench>();
            BenchmarkRunner.Run<SingleSortBench>();
            BenchmarkRunner.Run<StringSortBench>();
            BenchmarkRunner.Run<ComparableStructInt32SortBench>();

            BenchmarkRunner.Run<Int32Int32SortBench>();
            BenchmarkRunner.Run<Int32SingleSortBench>();
            BenchmarkRunner.Run<Int32StringSortBench>();

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
