using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

namespace DotNetCross.Sorting.Benchmarks
{
    [DisassemblyDiagnoser(printAsm: true, printSource: true, recursiveDepth: 3)]
    [SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 2, targetCount: 7)]
    public class MedianOfThreeKillerSort
    {
        const int Length = 2000000;
        static readonly int[] _medianOfThree = CreateMedianOfThreeKillerArray<int>(Length, i => i);
        int[] _work = new int[Length];

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine("// GlobalSetup");
            IterationSetup();
            ArraySort();
            CheckWork();
            IterationSetup();
            SpanSort(); // SpanSort fails for this pattern, so must be porting bug
            CheckWork();
        }

        private void CheckWork()
        {
            for (int i = 0; i < _work.Length - 1; i++)
            {
                var before = _work[i];
                var after = _work[i + 1];
                if (before > after)
                {
                    throw new InvalidOperationException($"Failed: {i} {before} {after}");
                }
            }
        }

        [IterationSetup]
        public void IterationSetup()
        {
            Array.Copy(_medianOfThree, _work, Length);
        }

        [Benchmark(Baseline = true)]
        public void ArraySort()
        {
            Array.Sort(_work);
        }

        [Benchmark]
        public void SpanSort()
        {
            new Span<int>(_work).Sort();
        }

        //[Benchmark]
        //public void SpanQuickSort_Hoare()
        //{
        //    QuickSort.Sort(new Span<int>(_work), new HoarePartitioner(), new ComparableComparer<int>());
        //}

        const int Seed = 213718398;
        private static T[] CreateMedianOfThreeKillerArray<T>(int length, Func<int, T> toValue)
            where T : IComparable<T>
        {
            var array = new T[length];
            InitializeMedianOfThreeKiller(array, toValue);
            return array;
        }

        // David Musser: Introspective Sorting and Selection Algorithms
        // https://programmingpraxis.com/2016/11/08/a-median-of-three-killer-sequence/
        private static void InitializeMedianOfThreeKiller<T>(Span<T> span, Func<int, T> toValue)
        {
            var length = span.Length;
            // if n is odd, set the last element to n-1, and proceed
            // with n decremented by 1
            if (length % 2 != 0)
            {
                span[length - 1] = toValue(length);
                --length;
            }
            var m = length / 2;
            for (int i = 0; i < m; ++i)
            {
                // first half of array (even indices)
                if (i % 2 == 0) span[i] = toValue(i + 1);
                // first half of array (odd indices)
                else span[i] = toValue(m + i + (m % 2 != 0 ? 1 : 0));
                // second half of array
                span[m + i] = toValue((i + 1) * 2);
            }
        }
    }
}
