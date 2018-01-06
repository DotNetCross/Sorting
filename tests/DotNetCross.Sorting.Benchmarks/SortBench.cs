using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    [Config(typeof(SortBenchmarkConfig))]
    public class SortBench<T>
    {
        readonly int _maxLength;
        readonly T[] _filled;
        readonly T[] _work;

        public SortBench(int maxLength, ISpanFiller filler, Func<int, T> toValue)
        {
            _maxLength = maxLength;
            _filled = new T[_maxLength];
            filler.Fill(_filled, toValue);
            _work = new T[_maxLength];
        }

        [ParamsSource(nameof(Lengths))]
        public int Length { get; set; }

        public IEnumerable<int> Lengths => new[] { 2, 3, 10, 100, 10000, 1000000 };

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
                new Span<T>(_work, i, Length).Sort();
            }
        }

        //[Benchmark]
        //public void SpanQuickSort_Hoare()
        //{
        //    QuickSort.Sort(new Span<int>(_work), new HoarePartitioner(), new ComparableComparer<int>());
        //}
    }
}
