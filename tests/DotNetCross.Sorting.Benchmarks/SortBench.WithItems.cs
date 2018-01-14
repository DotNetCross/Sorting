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
    public class SortBench<TKey, TValue>
    {
        readonly int _maxLength;
        readonly IParam[] _paramFillers;
        readonly Func<int, TKey> _toKey;
        readonly TKey[] _filled;
        readonly TKey[] _work;
        readonly Func<int, TValue> _toValue;
        readonly TValue[] _filledValues;
        readonly TValue[] _workValues;

        public SortBench(int maxLength, ISpanFiller[] fillers, 
            Func<int, TKey> toKey, Func<int, TValue> toValue)
        {
            _maxLength = maxLength;
            _paramFillers = fillers.Select(f => new SpanFillerParam(f)).ToArray();
            _toKey = toKey;
            _filled = new TKey[_maxLength];
            _work = new TKey[_maxLength];
            _toValue = toValue;
            _filledValues = new TValue[_maxLength];
            _workValues = new TValue[_maxLength];
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
            Filler.Fill(_filled, Length, _toKey);
            new IncrementingSpanFiller().Fill(_filledValues, Length, _toValue);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            Array.Copy(_filled, _work, _maxLength);
            Array.Copy(_filledValues, _workValues, _maxLength);
        }

        [Benchmark(Baseline = true)]
        public void ArraySort()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                Array.Sort(_work, _workValues, i, Length);
            }
        }

        [Benchmark]
        public void SpanSort()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort(new Span<TValue>(_workValues, i, Length));
            }
        }
    }
}
