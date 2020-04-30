using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    public class Int32SortBench : SortBench<int>
    {
        public Int32SortBench()
            : base(maxLength: 3000000, new[] { 100, 1000, 10000, 1000000 },
                   SpanFillers.RandomOnly, i => i)
        { }

        [Benchmark]
        public void DNX_CustomStructComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<int>(_work, i, Length).IntroSort(new CustomStructComparer());
            }
        }

        struct CustomStructComparer : IComparer<int>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(int x, int y) => x.CompareTo(y);
        }

        //int[] a = new int[3];
        //public void SortTest()
        //{
        //    TComparerImpl.Sort3(ref a[0], ref a[1], ref a[2],
        //        new CustomStructComparer());
        //}

    }
    public class SingleSortBench : SortBench<float>
    {
        public SingleSortBench()
            : base(maxLength: 3000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i)
        { }
    }
    public class StringSortBench : SortBench<string>
    {
        public StringSortBench()
            : base(maxLength: 100000, new[] { 2, 3, 10, 100, 1000, 10000 },
                   SpanFillers.Default, i => i.ToString("D9"))
        { }
    }
    public class ComparableStructInt32SortBench : SortBench<ComparableStructInt32>
    {
        public ComparableStructInt32SortBench()
            : base(maxLength: 3000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => new ComparableStructInt32(i))
        { }
    }
    public class ComparableClassInt32SortBench : SortBench<ComparableClassInt32>
    {
        public ComparableClassInt32SortBench()
            : base(maxLength: 400000, new[] { 2, 3, 10, 100, 10000, 100000 },
                   SpanFillers.Default, i => new ComparableClassInt32(i))
        { }
    }

    public class Int32Int32SortBench : SortBench<int, int>
    {
        public Int32Int32SortBench()
            : base(maxLength: 2000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i, i => i)
        { }
    }
    public class Int32SingleSortBench : SortBench<int, float>
    {
        public Int32SingleSortBench()
            : base(maxLength: 2000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i, i => i)
        { }
    }
    public class SingleInt32SortBench : SortBench<float, int>
    {
        public SingleInt32SortBench()
            : base(maxLength: 2000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i, i => i)
        { }
    }
    public class SingleSingleSortBench : SortBench<float, float>
    {
        public SingleSingleSortBench()
            : base(maxLength: 2000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i, i => i)
        { }
    }
    public class Int32StringSortBench : SortBench<int, string>
    {
        public Int32StringSortBench()
            : base(maxLength: 2000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i, i => i.ToString("D9"))
        { }
    }
    public class StringInt32SortBench : SortBench<string, int>
    {
        public StringInt32SortBench()
            : base(maxLength: 100000, new[] { 2, 3, 10, 100, 1000, 10000 }, 
                   SpanFillers.Default, i => i.ToString("D9"), i => i)
        { }
    }
    public class ComparableClassInt32Int32SortBench : SortBench<ComparableClassInt32, int>
    {
        public ComparableClassInt32Int32SortBench()
            : base(maxLength: 100000, new[] { 2, 3, 10, 100, 1000, 10000 },
                   SpanFillers.Default, i => new ComparableClassInt32(i), i => i)
        { }
    }
    public class ComparableStructInt32Int32SortBench : SortBench<ComparableClassInt32, int>
    {
        public ComparableStructInt32Int32SortBench()
            : base(maxLength: 100000, new[] { 2, 3, 10, 100, 1000, 10000 },
                   SpanFillers.Default, i => new ComparableClassInt32(i), i => i)
        { }
    }


    public class Int32SortDisassemblerBench : SortDisassemblerBench<int>
    {
        public Int32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => i)
        { }
    }
    public class SingleSortDisassemblerBench : SortDisassemblerBench<float>
    {
        public SingleSortDisassemblerBench()
            : base(length: 1024 * 1024, i => i)
        { }
    }
    public class ComparableStructInt32SortDisassemblerBench : SortDisassemblerBench<ComparableStructInt32>
    {
        public ComparableStructInt32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => new ComparableStructInt32(i))
        { }
    }
    public class ComparableClassInt32SortDisassemblerBench : SortDisassemblerBench<ComparableClassInt32>
    {
        public ComparableClassInt32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => new ComparableClassInt32(i))
        { }
    }
    public class StringSortDisassemblerBench : SortDisassemblerBench<string>
    {
        public StringSortDisassemblerBench()
            : base(length: 1024 * 1024, i => i.ToString("D9"))
        { }
    }

    public class Int32Int32SortDisassemblerBench : SortDisassemblerBench<int, int>
    {
        public Int32Int32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => i, i => i)
        { }
    }
    public class Int32SingleSortDisassemblerBench : SortDisassemblerBench<int, float>
    {
        public Int32SingleSortDisassemblerBench()
            : base(length: 1024 * 1024, i => i, i => i)
        { }
    }
    public class SingleInt32SortDisassemblerBench : SortDisassemblerBench<float, int>
    {
        public SingleInt32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => i, i => i)
        { }
    }
    public class StringInt32SortDisassemblerBench : SortDisassemblerBench<string, int>
    {
        public StringInt32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => i.ToString("D9"), i => i)
        { }
    }
    public class Int32StringSortDisassemblerBench : SortDisassemblerBench<int, string>
    {
        public Int32StringSortDisassemblerBench()
            : base(length: 1024 * 1024, i => i, i => i.ToString("D9"))
        { }
    }
    public class ComparableClassInt32Int32SortDisassemblerBench : SortDisassemblerBench<ComparableClassInt32, int>
    {
        public ComparableClassInt32Int32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => new ComparableClassInt32(i), i => i)
        { }
    }
    public class ComparableStructInt32Int32SortDisassemblerBench : SortDisassemblerBench<ComparableClassInt32, int>
    {
        public ComparableStructInt32Int32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => new ComparableClassInt32(i), i => i)
        { }
    }

    class Program
    {
        enum Do { Focus, Full, Micro, Loop1, Loop2 }

        static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            // TODO: Refactor to switch/case and methods perhaps, less flexible though
            // TODO: Add argument parsing for this perhaps
            var d = Debugger.IsAttached ? Do.Loop1 : Do.Focus;
            if (d == Do.Focus)
            {
                BenchmarkRunner.Run<Int32SortBench>();
                BenchmarkRunner.Run<SingleSortBench>();
                BenchmarkRunner.Run<ComparableStructInt32SortBench>();
                BenchmarkRunner.Run<ComparableClassInt32SortBench>();
                BenchmarkRunner.Run<StringSortBench>();
                //BenchmarkRunner.Run<StringInt32SortBench>();
                //BenchmarkRunner.Run<ComparableClassInt32Int32SortBench>();
                //BenchmarkRunner.Run<ComparableStructInt32Int32SortBench>();

                // Custom benchs as seen elsewhere
                //BenchmarkRunner.Run<SortDictionary>();
            }
            else if( d == Do.Full)
            {
                // TKey benchs
                BenchmarkRunner.Run<Int32SortBench>();
                BenchmarkRunner.Run<SingleSortBench>();
                BenchmarkRunner.Run<ComparableStructInt32SortBench>();
                BenchmarkRunner.Run<ComparableClassInt32SortBench>();
                BenchmarkRunner.Run<StringSortBench>();
                // TKey,TValue benchs
                BenchmarkRunner.Run<Int32Int32SortBench>();
                BenchmarkRunner.Run<Int32SingleSortBench>();
                BenchmarkRunner.Run<SingleInt32SortBench>();
                BenchmarkRunner.Run<SingleSingleSortBench>();
                BenchmarkRunner.Run<Int32StringSortBench>();
                BenchmarkRunner.Run<StringInt32SortBench>();
                BenchmarkRunner.Run<ComparableClassInt32Int32SortBench>();
                BenchmarkRunner.Run<ComparableStructInt32Int32SortBench>();
                // Disassemblers work poorly due to generic code
                // TKey disassemblers
                //BenchmarkRunner.Run<Int32SortDisassemblerBench>();
                //BenchmarkRunner.Run<SingleSortDisassemblerBench>();
                //BenchmarkRunner.Run<ComparableStructInt32SortDisassemblerBench>();
                //BenchmarkRunner.Run<ComparableClassInt32SortDisassemblerBench>();
                //BenchmarkRunner.Run<StringSortDisassemblerBench>();
                // TKey,TValue disassemblers
                //BenchmarkRunner.Run<Int32Int32SortDisassemblerBench>();
                //BenchmarkRunner.Run<Int32SingleSortDisassemblerBench>();
                //BenchmarkRunner.Run<SingleInt32SortDisassemblerBench>();
                //BenchmarkRunner.Run<Int32StringSortDisassemblerBench>();
                //BenchmarkRunner.Run<StringInt32SortDisassemblerBench>();
                //BenchmarkRunner.Run<ComparableClassInt32Int32SortDisassemblerBench>();
                //BenchmarkRunner.Run<ComparableStructInt32Int32SortDisassemblerBench>();
            }
            else if (d == Do.Micro)
            { 
                // Micro benchmarks
                //BenchmarkRunner.Run<IntPtrHelperBenchmark>();
                //BenchmarkRunner.Run<CompareAsm>();
                BenchmarkRunner.Run<ComparableInt32ClassCompareToLessThanBench>();
                //var b = new ComparableInt32ClassCompareToLessThanBench();
                //b.ComparerOpenDelegate();
            }
            else if (d == Do.Loop1)
            {
                //var sut = new ComparableClassInt32SortBench();
                var sut = new StringSortBench();
                sut.Filler = new RandomSpanFiller(SpanFillers.RandomSeed);
                sut.Length = 10000; // 1000000;
                sut.GlobalSetup();
                sut.IterationSetup();
                sut.DNX_();
                sut.IterationSetup();
                sut.CLR_();

                //Console.WriteLine("Enter key...");
                //Console.ReadKey();

                for (int i = 0; i < 200; i++)
                {
                    sut.IterationSetup();
                    sut.DNX_();
                    sut.IterationSetup();
                    sut.CLR_();
                }
            }
            else if (d == Do.Loop2)
            {
                var sut = new ComparableClassInt32SortBench();
                //var sut = new ComparableClassInt32Int32SortBench();
                //var sut = new StringInt32SortBench();
                sut.Filler = new RandomSpanFiller(SpanFillers.RandomSeed);
                sut.Length = 10000; // 1000000;

                sut.GlobalSetup();
                sut.IterationSetup();
                sut.DNX_StructComparableComparer();
                sut.IterationSetup();
                sut.CLR_StructComparableComparer();

                //Console.WriteLine("Enter key...");
                //Console.ReadKey();

                for (int i = 0; i < 200; i++)
                {
                    sut.IterationSetup();
                    sut.DNX_StructComparableComparer();
                    sut.IterationSetup();
                    sut.CLR_StructComparableComparer();
                }
            }
        }
    }
}
