using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    public class SortConfig : ManualConfig
    {
        public SortConfig()
        {
            Add(new Job(EnvMode.Core, Job.Dry)
            {
                //Env = { Runtime = Runtime.Core },
                Run = { LaunchCount = 1, WarmupCount = 2, TargetCount = 11, RunStrategy = RunStrategy.Monitoring },
                
                //Accuracy = { MaxStdErrRelative = 0.01 }
            });
            Add(DisassemblyDiagnoser.Create(
                new DisassemblyDiagnoserConfig(printAsm: true, printPrologAndEpilog: true, printSource: true, recursiveDepth: 3)));

        }
    }

    //[DisassemblyDiagnoser(printAsm: true, printSource: true, recursiveDepth: 3)]
    //[SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 2, targetCount: 11)]
    [Config(typeof(SortConfig))]
    public class SortBase
    {
        const int Length = 2000000;
        readonly ISpanFiller _filler;
        readonly int[] _random = CreateRandomArray<int>(Length, i => i);
        int[] _work = new int[Length];

        public SortBase(ISpanFiller filler)
        {
            _filler = filler;
        }

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

    public class NewRandomSort : SortBase
    {
        const int Seed = 213718398;

        public NewRandomSort()
            : base(new RandomSpanFiller(Seed))
        { }
    }
}
