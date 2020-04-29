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
    [ShortRunJob]// SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 1, targetCount: 3, invocationCount: 3)]
    //[Config(typeof(SortDisassemblerBenchConfig))]
    public class SortDisassemblerBench<TKey>
        where TKey : IComparable<TKey>
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
        public void Array_()
        {
            Array.Sort(_work);
        }
        [Benchmark]
        public void Array_NullComparer()
        {
            Array.Sort(_work, (IComparer<TKey>)null);
        }
        [Benchmark]
        public void Array_ClassComparableComparer()
        {
            Array.Sort(_work, ClassComparableComparer<TKey>.Instance);
        }
        [Benchmark]
        public void Array_Comparison()
        {
            Array.Sort(_work, ComparableComparison<TKey>.Instance);
        }

#if !NETCOREAPP3_1
        [Benchmark]
        public void CLR_Span_()
        {
            new Span<TKey>(_work).Sort();
        }

        [Benchmark]
        public void CLR_Span_StructComparableComparer()
        {
            new Span<TKey>(_work).Sort(new StructComparableComparer<TKey>());
        }
        [Benchmark]
        public void CLR_Span_Comparison()
        {
            var span = new Span<TKey>(_work);
            span.Sort(ComparableComparison<TKey>.Instance);
        }
#endif

        [Benchmark]
        public void DNX_Span_()
        {
            new Span<TKey>(_work).IntroSort();
        }
        [Benchmark]
        public void DNX_Span_NullComparer()
        {
            new Span<TKey>(_work).IntroSort((IComparer<TKey>)null);
        }
        [Benchmark]
        public void DNX_Span_ClassComparableComparer()
        {
            new Span<TKey>(_work).IntroSort(ClassComparableComparer<TKey>.Instance);
        }
        [Benchmark]
        public void DNX_Span_StructComparableComparer()
        {
            new Span<TKey>(_work).IntroSort(new StructComparableComparer<TKey>());
        }
        [Benchmark]
        public void DNX_Span_Comparison()
        {
            new Span<TKey>(_work).IntroSort(ComparableComparison<TKey>.Instance);
        }
    }
}
