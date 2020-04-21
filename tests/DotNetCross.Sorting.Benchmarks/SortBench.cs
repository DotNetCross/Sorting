using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    [Config(typeof(SortBenchConfig))]
    [MemoryDiagnoser]
    public class SortBench<TKey>
        where TKey : IComparable<TKey>
    {
        static readonly ClassComparableComparer<TKey> _classComparer = new ClassComparableComparer<TKey>();
        protected readonly int _maxLength;
        protected readonly ISpanFiller[] _fillers;
        protected readonly Func<int, TKey> _toValue;
        protected readonly TKey[] _filled;
        protected readonly TKey[] _work;

        public SortBench(int maxLength, int[] sliceLengths, ISpanFiller[] fillers, Func<int, TKey> toValue)
        {
            _maxLength = maxLength;
            Lengths = sliceLengths;
            _fillers = fillers;
            _toValue = toValue;
            _filled = new TKey[_maxLength];
            _work = new TKey[_maxLength];
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
            Filler.Fill(_filled, Length, _toValue);
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
        //[Benchmark]
        public void Array_NullComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                Array.Sort(_work, i, Length, (IComparer<TKey>)null);
            }
        }
        //[Benchmark]
        public void Array_ClassComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                Array.Sort(_work, i, Length, ClassComparableComparer<TKey>.Instance);
            }
        }
#if !NETCOREAPP3_1
        [Benchmark]
        public void CLR_Span_()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort();
            }
        }

        //[Benchmark]
        public void CLR_Span_StructComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort(new StructComparableComparer<TKey>());
            }
        }
        //[Benchmark]
        public void CLR_Span_Comparison()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                // Using span from .NET Core 3.1
                var span = new Span<TKey>(_work, i, Length);
                span.Sort(ComparableComparison<TKey>.Instance);
            }
        }
#endif

        [Benchmark]
        public void DNX_Span_()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort();
            }
        }
        //[Benchmark]
        public void DNX_Span_NullComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort((IComparer<TKey>)null);
            }
        }
        //[Benchmark]
        public void DNX_Span_ClassComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort(ClassComparableComparer<TKey>.Instance);
            }
        }
        //[Benchmark]
        public void DNX_Span_StructComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort(new StructComparableComparer<TKey>());
            }
        }
        //[Benchmark]
        public void DNX_Span_Comparison()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort(ComparableComparison<TKey>.Instance);
            }
        }
    }
}
