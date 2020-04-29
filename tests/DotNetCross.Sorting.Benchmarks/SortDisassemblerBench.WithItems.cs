using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
//using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
//using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    [DisassemblyDiagnoser(printSource: true, maxDepth: 4)]
    [SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 1, targetCount: 3)]
    //[Config(typeof(SortDisassemblerBenchConfig))]
    public class SortDisassemblerBench<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        readonly int _length;
        readonly TKey[] _filled;
        readonly TKey[] _work;
        readonly TValue[] _filledValues;
        readonly TValue[] _workValues;

        public SortDisassemblerBench(int length, Func<int, TKey> toKey, Func<int, TValue> toValue)
        {
            _length = length;
            _filled = new TKey[_length];
            // We use median of three to ensure heap sort code is hit
            new MedianOfThreeKillerSpanFiller().Fill(_filled, _length, toKey);
            _work = new TKey[_length];

            _filledValues = new TValue[_length];
            // We use incrementing for items to ensure unique and since it is simple
            new IncrementingSpanFiller().Fill(_filledValues, _length, toValue);
            _workValues = new TValue[_length];
        }

        [IterationSetup]
        public void IterationSetup()
        {
            Console.WriteLine($"// {nameof(IterationSetup)} Copy filled to work {_length}");
            Array.Copy(_filled, _work, _length);
            Array.Copy(_filledValues, _workValues, _length);
        }

        [Benchmark(Baseline = true)]
        public void Array_()
        {
            Array.Sort(_work, _workValues);
        }
        [Benchmark]
        public void Array_NullComparer()
        {
            Array.Sort(_work, _workValues, (IComparer<TKey>)null);
        }
        [Benchmark]
        public void Array_ClassComparableComparer()
        {
            Array.Sort(_work, _workValues, ClassComparableComparer<TKey>.Instance);
        }
        // Overload doesn't exist
        //[Benchmark]
        //public void Array_Comparison()
        //{
        //    Array.Sort(_work, _workValues, ComparableComparison<TKey>.Instance);
        //}

        [Benchmark]
        public void Span_()
        {
            new Span<TKey>(_work).IntroSort(new Span<TValue>(_workValues));
        }
        [Benchmark]
        public void Span_NullComparer()
        {
            new Span<TKey>(_work).IntroSort(new Span<TValue>(_workValues), (IComparer<TKey>)null);
        }
        [Benchmark]
        public void Span_ClassComparableComparer()
        {
            new Span<TKey>(_work).IntroSort(new Span<TValue>(_workValues), ClassComparableComparer<TKey>.Instance);
        }
        [Benchmark]
        public void Span_StructComparableComparer()
        {
            new Span<TKey>(_work).IntroSort(new Span<TValue>(_workValues), new StructComparableComparer<TKey>());
        }
        [Benchmark]
        public void Span_Comparison()
        {
            new Span<TKey>(_work).IntroSort(new Span<TValue>(_workValues), ComparableComparison<TKey>.Instance);
        }
    }
}
