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
        readonly SpanFillerParam[] _paramFillers;
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
        public SpanFillerParam Filler { get; set; }

        public IEnumerable<SpanFillerParam> Fillers() => _paramFillers;

        [ParamsSource(nameof(Lengths))]
        public int Length { get; set; }

        public IEnumerable<int> Lengths { get; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine($"// {nameof(GlobalSetup)} Filling {_maxLength} with {Filler.GetType().Name} for {Length} slice run");
            Filler.Value.Fill(_filled, Length, _toValue);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            Array.Copy(_filled, _work, _maxLength);
        }

        [Benchmark(Baseline = true)]
        public void Array_()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                Array.Sort(_work, i, Length);
            }
        }
        [Benchmark]
        public void Array_NullComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                Array.Sort(_work, i, Length, (IComparer<TKey>)null);
            }
        }
        [Benchmark]
        public void Array_ClassComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                Array.Sort(_work, i, Length, ClassComparableComparer<TKey>.Instance);
            }
        }
        // Comparison overload only exists for full array?
        //[Benchmark]
        //public void Array_Comparison()
        //{
        //    for (int i = 0; i <= _maxLength - Length; i += Length)
        //    {
        //        Array.Sort(_work, i, Length, ComparableComparison<TKey>.Instance);
        //    }
        //}

        [Benchmark]
        public void Span_()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort();
            }
        }
        [Benchmark]
        public void Span_NullComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort((IComparer<TKey>)null);
            }
        }
        [Benchmark]
        public void Span_ClassComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort(ClassComparableComparer<TKey>.Instance);
            }
        }
        [Benchmark]
        public void Span_StructComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort(new StructComparableComparer<TKey>());
            }
        }
        [Benchmark]
        public void Span_Comparison()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort(ComparableComparison<TKey>.Instance);
            }
        }
    }
}
