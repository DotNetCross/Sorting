using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    [Config(typeof(SortBenchmarkConfig))]
    public class SortBench
    {
        readonly int _maxLength;
        readonly int[] _filled;
        readonly int[] _work;

        public SortBench(int maxLength, ISpanFiller filler)
        {
            _maxLength = maxLength;
            _filled = new int[_maxLength];
            filler.Fill(_filled, i => i);
            _work = new int[_maxLength];
        }

        [ParamsSource(nameof(Lengths))]
        public int Length { get; set; }

        public IEnumerable<int> Lengths => new[] { 3, 10, 100, 10000, 1000000 };

        [IterationSetup]
        public void IterationSetup()
        {
            Array.Copy(_filled, _work, _maxLength);
        }

        [Benchmark(Baseline = true)]
        public void ArraySort()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                Array.Sort(_work, i, Length);
            }
        }

        [Benchmark]
        public void SpanSort()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<int>(_work, i, Length).Sort();
            }
        }

        //[Benchmark]
        //public void SpanQuickSort_Hoare()
        //{
        //    QuickSort.Sort(new Span<int>(_work), new HoarePartitioner(), new ComparableComparer<int>());
        //}
    }
}
