using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    //
    //[DisassemblyDiagnoser(printAsm: true, printSource: true, recursiveDepth: 4)]
    //[DisassemblyDiagnoser(recursiveDepth: 2)]
    //[SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 3, targetCount: 11)]
    //[RyuJitX64Job()]
    [Config(typeof(SortDisassemblerBenchConfig))]
    public class RandomSort
    {
        const int MaxLength = 3 * 1000 * 1000;
        static readonly int[] _filled = new int[MaxLength];
        int[] _work = new int[MaxLength];

        public RandomSort()
        {
            Length = 1000000;
        }

        //[ParamsSource(nameof(Lengths))]
        public int Length { get; set; }

        //public IEnumerable<int> Lengths => new[] { 1000000 }; //1, 10, 100, 10000, 
        [GlobalSetup]
        public void GlobalSetup()
        {
            //const int Seed = 213718398;
            var filler = new MedianOfThreeKillerSpanFiller();
            Console.WriteLine($"// {nameof(GlobalSetup)} Filling {MaxLength} with {filler.GetType().Name} for {Length} slice run");
            filler.Fill(_filled, Length, i => i);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            Array.Copy(_filled, _work, MaxLength);
        }

        //[Benchmark(Baseline = true)]
        //public void ArraySort()
        //{
        //    for (int i = 0; i <= MaxLength - Length; i += Length)
        //    {
        //        Array.Sort(_work, i, Length);
        //    }
        //}

        [Benchmark]
        public void SpanSort()
        {
            for (int i = 0; i <= MaxLength - Length; i += Length)
            {
                new Span<int>(_work, i, Length).IntroSort();
            }
        }

        //const int Seed = 213718398;
        //private static T[] CreateRandomArray<T>(int length, Func<int, T> toValue)
        //    where T : IComparable<T>
        //{
        //    //var random = new Random(Seed);
        //    //var array = new T[length];
        //    //for (int i = 0; i < array.Length; i++)
        //    //{
        //    //    array[i] = toValue(random.Next());
        //    //}
        //    //return array;
        //    var array = new T[length];
        //    var filler = new MedianOfThreeKillerSpanFiller();
        //    filler.Fill(array, toValue);
        //    return array;
        //}
    }
}
