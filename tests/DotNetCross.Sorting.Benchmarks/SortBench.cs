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
    public class SortBench<T>
    {
        readonly int _maxLength;
        readonly IParam[] _paramFillers;
        readonly Func<int, T> _toValue;
        readonly T[] _filled;
        readonly T[] _work;

        public SortBench(int maxLength, ISpanFiller[] fillers, Func<int, T> toValue)
        {
            _maxLength = maxLength;
            _paramFillers = fillers.Select(f => new SpanFillerParam(f)).ToArray();
            _toValue = toValue;
            _filled = new T[_maxLength];
            _work = new T[_maxLength];
        }

        [ParamsSource(nameof(Fillers))]
        public ISpanFiller Filler { get; set; }

        public IEnumerable<IParam> Fillers() => _paramFillers;

        [ParamsSource(nameof(Lengths))]
        public int Length { get; set; }

        public IEnumerable<int> Lengths => new[] { 2, 3, 10, 100, 10000, 1000000 };

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine($"// {nameof(GlobalSetup)} Filling {_maxLength} with {Filler.GetType().Name} for {Length} slice run");
            Filler.Fill(_filled, _toValue);
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
        public void SpanSort()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<T>(_work, i, Length).Sort();
            }
        }
    }
}
