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
        where TKey : IComparable<TKey>
    {
        readonly int _maxLength;
        readonly ISpanFiller[] _fillers;
        readonly Func<int, TKey> _toKey;
        readonly TKey[] _filled;
        readonly TKey[] _work;
        readonly Func<int, TValue> _toValue;
        readonly TValue[] _filledValues;
        readonly TValue[] _workValues;

        public SortBench(int maxLength, int[] sliceLengths, ISpanFiller[] fillers, 
            Func<int, TKey> toKey, Func<int, TValue> toValue)
        {
            _maxLength = maxLength;
            Lengths = sliceLengths;
            _fillers = fillers;
            _toKey = toKey;
            _filled = new TKey[_maxLength];
            _work = new TKey[_maxLength];
            _toValue = toValue;
            _filledValues = new TValue[_maxLength];
            _workValues = new TValue[_maxLength];
        }

        [ParamsSource(nameof(Fillers))]
        public ISpanFiller Filler { get; set; }

        public IEnumerable<ISpanFiller> Fillers() => _fillers;

        [ParamsSource(nameof(Lengths))]
        public int Length { get; set; }

        public IEnumerable<int> Lengths { get; }


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
        public void CLR_Array_()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                Array.Sort(_work, _workValues, i, Length);
            }
        }
        [Benchmark]
        public void CLR_Array_NullComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                Array.Sort(_work, _workValues, i, Length, (IComparer<TKey>)null);
            }
        }
        //[Benchmark]
        public void CLR_Array_ClassComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                Array.Sort(_work, _workValues, i, Length, ClassComparableComparer<TKey>.Instance);
            }
        }
#if !NETCOREAPP3_1
        [Benchmark]
        public void CLR_()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort(new Span<TValue>(_workValues, i, Length));
            }
        }
        //[Benchmark]
        public void CLR_ClassComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort(new Span<TValue>(_workValues, i, Length), ClassComparableComparer<TKey>.Instance);
            }
        }
        //[Benchmark]
        public void CLR_StructComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort(new Span<TValue>(_workValues, i, Length), new StructComparableComparer<TKey>());
            }
        }
        //[Benchmark]
        public void CLR_Comparison()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                var workSpan = new Span<TKey>(_work, i, Length);
                var workValuesSpan = new Span<TValue>(_workValues, i, Length);
                workSpan.Sort(workValuesSpan, ComparableComparison<TKey>.Instance);
            }
        }
#endif

        [Benchmark]
        public void DNX_()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort(new Span<TValue>(_workValues, i, Length));
            }
        }
        [Benchmark]
        public void DNX_NullComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort(new Span<TValue>(_workValues, i, Length), (IComparer<TKey>)null);
            }
        }
        //[Benchmark]
        public void DNX_ClassComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort(new Span<TValue>(_workValues, i, Length), ClassComparableComparer<TKey>.Instance);
            }
        }
        //[Benchmark]
        public void DNX_StructComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort(new Span<TValue>(_workValues, i, Length), new StructComparableComparer<TKey>());
            }
        }
        //[Benchmark]
        public void DNX_Comparison()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort(new Span<TValue>(_workValues, i, Length), ComparableComparison<TKey>.Instance);
            }
        }
    }
}
