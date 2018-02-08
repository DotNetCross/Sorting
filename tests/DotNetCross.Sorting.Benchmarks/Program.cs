using System;
using System.Diagnostics;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    public static class SpanFillers
    {
        public const int RandomSeed = 213718398;
        public const double ShuffleFraction = 0.1;
        public const int ShuffleSeed = 931712983;

        public static ISpanFiller[] Default = new ISpanFiller[]{
                new RandomSpanFiller(RandomSeed),
                new PartialRandomShuffleSpanFiller(new IncrementingSpanFiller(), ShuffleFraction, ShuffleSeed),
                new MedianOfThreeKillerSpanFiller(),
                new IncrementingSpanFiller(),
                new DecrementingSpanFiller(),
                new ConstantSpanFiller(42),
            };
    }

    public class Int32SortBench : SortBench<int>
    {
        public Int32SortBench()
            : base(maxLength: 3000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i)
        { }
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
            : base(maxLength: 100000, new[] { 2, 3, 10, 100, 10000, 100000 },
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
        static void Main(string[] args)
        {
            if (true && !Debugger.IsAttached)
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
                BenchmarkRunner.Run<Int32StringSortBench>();
                BenchmarkRunner.Run<StringInt32SortBench>();
                BenchmarkRunner.Run<ComparableClassInt32Int32SortBench>();
                BenchmarkRunner.Run<ComparableStructInt32Int32SortBench>();

                // TKey disassemblers
                BenchmarkRunner.Run<Int32SortDisassemblerBench>();
                BenchmarkRunner.Run<SingleSortDisassemblerBench>();
                BenchmarkRunner.Run<ComparableStructInt32SortDisassemblerBench>();
                BenchmarkRunner.Run<ComparableClassInt32SortDisassemblerBench>();
                BenchmarkRunner.Run<StringSortDisassemblerBench>();
                // TKey,TValue disassemblers
                BenchmarkRunner.Run<Int32Int32SortDisassemblerBench>();
                BenchmarkRunner.Run<Int32SingleSortDisassemblerBench>();
                BenchmarkRunner.Run<SingleInt32SortDisassemblerBench>();
                BenchmarkRunner.Run<Int32StringSortDisassemblerBench>();
                BenchmarkRunner.Run<StringInt32SortDisassemblerBench>();
                BenchmarkRunner.Run<ComparableClassInt32Int32SortDisassemblerBench>();
                BenchmarkRunner.Run<ComparableStructInt32Int32SortDisassemblerBench>();

                // Micro benchmarks
                //BenchmarkRunner.Run<IntPtrHelperBenchmark>();
                //BenchmarkRunner.Run<CompareAsm>();
                //BenchmarkRunner.Run<CompareToLessThanBench>();
            }
            else if (true)
            {
                var sut = new ComparableClassInt32SortBench();
                //var sut = new StringSortBench();
                sut.Filler = new RandomSpanFiller(SpanFillers.RandomSeed);
                sut.Length = 1000; // 1000000;
                sut.GlobalSetup();
                sut.IterationSetup();
                sut.Span_();
                sut.IterationSetup();
                sut.Span_();

                Console.WriteLine("Enter key...");
                Console.ReadKey();

                for (int i = 0; i < 1000; i++)
                {
                    sut.IterationSetup();
                    sut.Span_();
                }
            }
            else
            {
                var sut = new CompareToLessThanBench();
                sut.OpenDelegate();
            }
        }
    }
}
