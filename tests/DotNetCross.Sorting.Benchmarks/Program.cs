﻿using System.Runtime.CompilerServices;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    public class RandomSortBench : SortBench
    {
        const int Seed = 213718398;

        public RandomSortBench()
            : base(maxLength: 3000000, new RandomSpanFiller(Seed))
        { }
    }

    public class RandomShuffleSortBench : SortBench
    {
        const double Fraction = 0.1;
        const int Seed = 931712983;
        public RandomShuffleSortBench()
            : base(maxLength: 3000000,
                  new PartialRandomShuffleSpanFiller(new IncrementingSpanFiller(), Fraction, Seed))
        { }
    }

    public class MedianOfThreeSortBench : SortBench
    {
        public MedianOfThreeSortBench()
            : base(maxLength: 6000000, new MedianOfThreeKillerSpanFiller())
        { }
    }

    public class IncrementingSortBench : SortBench
    {
        public IncrementingSortBench()
            : base(maxLength: 6000000, new IncrementingSpanFiller())
        { }
    }

    public class DecrementingSortBench : SortBench
    {
        public DecrementingSortBench()
            : base(maxLength: 6000000, new DecrementingSpanFiller())
        { }
    }

    public class ConstantSortBench : SortBench
    {
        public ConstantSortBench()
            : base(maxLength: 6000000, new ConstantSpanFiller(42))
        { }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<RandomShuffleSortBench>();
            BenchmarkRunner.Run<RandomSortBench>();
            BenchmarkRunner.Run<MedianOfThreeSortBench>();
            BenchmarkRunner.Run<IncrementingSortBench>();
            BenchmarkRunner.Run<DecrementingSortBench>();
            BenchmarkRunner.Run<ConstantSortBench>();

            //BenchmarkRunner.Run<IntPtrHelperBenchmark>();
            //BenchmarkRunner.Run<RandomSort>();
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
            var s = new RandomSort();
            s.SpanSort();
        }
    }
}
