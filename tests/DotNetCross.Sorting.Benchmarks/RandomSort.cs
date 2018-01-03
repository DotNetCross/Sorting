using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

namespace DotNetCross.Sorting.Benchmarks
{
    public static class IntPtrHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static IntPtr SameAssemblyMultiply(this IntPtr a, int factor)
        {
            return (sizeof(IntPtr) == sizeof(int))
                ? new IntPtr((int)a * factor)
                : new IntPtr((long)a * factor);
        }
    }

    [DisassemblyDiagnoser(recursiveDepth: 2)]
    [SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 2, targetCount: 11)]
    public class IntPtrHelperBenchmark
    {
        [Benchmark]
        public IntPtr Ctor()
        {
            return new IntPtr(42).SameAssemblyMultiply(4);
        }
    }

    //printSource: true, 
    //[DisassemblyDiagnoser(printAsm: true, recursiveDepth: 3)]
    [DisassemblyDiagnoser(recursiveDepth: 2)]
    [SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 2, targetCount: 11)]
    //[RyuJitX64Job()]
    public class RandomSort
    {
        const int Length = 2000000;
        static readonly int[] _random = CreateRandomArray<int>(Length, i => i);
        int[] _work = new int[Length];

        [IterationSetup]
        public void IterationSetup()
        {
            //Console.WriteLine(nameof(IterationSetup));
            Array.Copy(_random, _work, Length);
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
        private static T[] CreateRandomArray<T>(int length, Func<int, T> toValue)
            where T : IComparable<T>
        {
            var random = new Random(Seed);
            var array = new T[length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = toValue(random.Next());
            }
            return array;
        }
    }
}
