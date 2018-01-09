using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;
using T = System.Int32;

namespace DotNetCross.Sorting.Benchmarks
{
    [DisassemblyDiagnoser(printAsm: true, printSource: true, recursiveDepth: 4)]
    [SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 1, targetCount: 3)]
    //[Config(typeof(SortDisassemblerBenchConfig))]
    public class SortDisassemblerBench//<T>
    {
        readonly int _length;
        readonly T[] _filled;
        readonly T[] _work;

        public SortDisassemblerBench(int length, Func<int, T> toValue)
        {
            _length = length;
            _filled = new T[_length];
            // We use median of three to ensure heap sort code is hit
            new MedianOfThreeKillerSpanFiller().Fill(_filled, toValue);
            _work = new T[_length];
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine($"// {nameof(GlobalSetup)} Filling {_length}");
        }

        [IterationSetup]
        public void IterationSetup()
        {
            Array.Copy(_filled, _work, _length);
        }

        [Benchmark(Baseline = true)]
        public void ArraySort()
        {
            Array.Sort(_work);
        }

        [Benchmark]
        public void SpanSort()
        {
            new Span<T>(_work).Sort();
        }
    }
}
