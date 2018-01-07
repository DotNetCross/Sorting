using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

namespace DotNetCross.Sorting.Benchmarks
{
    //
    //[DisassemblyDiagnoser(printAsm: true, printSource: true, recursiveDepth: 4)]
    //[DisassemblyDiagnoser(recursiveDepth: 2)]
    //[SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 3, targetCount: 11)]
    //[RyuJitX64Job()]
    [Config(typeof(SortBenchConfig))]
    public class RandomSort
    {
        const int MaxLength = 3 * 1000 * 1000;
        static readonly int[] _random = CreateRandomArray<int>(MaxLength, i => i);
        int[] _work = new int[MaxLength];

        [ParamsSource(nameof(Lengths))]
        public int Length { get; set; }

        public IEnumerable<int> Lengths => new[] { 1, 10, 100, 10000, 1000000 };

        [IterationSetup]
        public void IterationSetup()
        {
            Array.Copy(_random, _work, MaxLength);
        }

        [Benchmark(Baseline = true)]
        public void ArraySort()
        {
            for (int i = 0; i <= MaxLength - Length; i += Length)
            {
                Array.Sort(_work, i, Length);
            }
        }

        [Benchmark]
        public void SpanSort()
        {
            for (int i = 0; i <= MaxLength - Length; i += Length)
            {
                new Span<int>(_work, i, Length).Sort();
            }
        }

        //[Benchmark]
        //public void SpanQuickSort_Hoare()
        //{
        //    QuickSort.Sort(new Span<int>(_work), new HoarePartitioner(), new ComparableComparer<int>());
        //}

        const int Seed = 213718398;
        private static T[] CreateRandomArray<T>(int length, Func<int, T> toValue)
            where T : IComparable<T>
        {
            var random = new Random(Seed);
            var array = new T[length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = toValue(random.Next());
            }
            return array;
        }
    }
}
