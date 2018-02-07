using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Code;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    [Config(typeof(SortBenchConfig))]
    public class SortBench<TKey>
        where TKey : IComparable<TKey>
    {
        static readonly ClassComparableComparer<TKey> _classComparer = new ClassComparableComparer<TKey>();
        readonly int _maxLength;
        readonly IParam[] _paramFillers;
        readonly Func<int, TKey> _toValue;
        readonly TKey[] _filled;
        readonly TKey[] _work;

        public SortBench(int maxLength, int[] sliceLengths, ISpanFiller[] fillers, Func<int, TKey> toValue)
        {
            _maxLength = maxLength;
            Lengths = sliceLengths;
            _paramFillers = fillers.Select(f => new SpanFillerParam(f)).ToArray();
            _toValue = toValue;
            _filled = new TKey[_maxLength];
            _work = new TKey[_maxLength];
        }

        [ParamsSource(nameof(Fillers))]
        public ISpanFiller Filler { get; set; }

        public IEnumerable<IParam> Fillers() => _paramFillers;

        [ParamsSource(nameof(Lengths))]
        public int Length { get; set; }

        public IEnumerable<int> Lengths { get; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine($"// {nameof(GlobalSetup)} Filling {_maxLength} with {Filler.GetType().Name} for {Length} slice run");
            Filler.Fill(_filled, Length, _toValue);
        }

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
        public void ArraySort_ClassComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                Array.Sort(_work, i, Length, _classComparer);
            }
        }

        [Benchmark]
        public void SpanSort()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort();
            }
        }
        [Benchmark]
        public void SpanSort_ClassComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort(_classComparer);
            }
        }
        [Benchmark]
        public void SpanSort_StructComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort(new ComparableComparer<TKey>());
            }
        }
    }
}
