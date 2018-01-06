using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

namespace DotNetCross.Sorting.Benchmarks
{
    //
    [DisassemblyDiagnoser(printAsm: true, printSource: true, recursiveDepth: 4)]
    //[DisassemblyDiagnoser(recursiveDepth: 2)]
    [SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 2, targetCount: 21)]
    //[RyuJitX64Job()]
    public class RandomSort
    {
        const int Length = 2000000;
        static readonly int[] _random = CreateRandomArray<int>(Length, i => i);
        int[] _work = new int[Length];

        [IterationSetup]
        public void IterationSetup()
        {
            //Console.WriteLine(nameof(IterationSetup));
            Array.Copy(_random, _work, Length);
        }

        [Benchmark(Baseline = true)]
        public void ArraySort()
        {
            Array.Sort(_work);
        }

        [Benchmark]
        public void SpanSort()
        {
            new Span<int>(_work).Sort();
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
