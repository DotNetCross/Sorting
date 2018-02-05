using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;
//using T = System.Int32;

namespace DotNetCross.Sorting.Benchmarks
{
    [DisassemblyDiagnoser(printAsm: true, printSource: true, recursiveDepth: 4)]
    [SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 1, targetCount: 3)]
    //[Config(typeof(SortDisassemblerBenchConfig))]
    public class SortDisassemblerBench<TKey>
    {
        readonly int _length;
        readonly TKey[] _filled;
        readonly TKey[] _work;

        public SortDisassemblerBench(int length, Func<int, TKey> toValue)
        {
            _length = length;
            _filled = new TKey[_length];
            // We use median of three to ensure heap sort code is hit
            new MedianOfThreeKillerSpanFiller().Fill(_filled, _length, toValue);
            _work = new TKey[_length];
        }

        [IterationSetup]
        public void IterationSetup()
        {
            Console.WriteLine($"// {nameof(IterationSetup)} Copy filled to work {_length}");
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
            new Span<TKey>(_work).Sort();
        }
    }
}
