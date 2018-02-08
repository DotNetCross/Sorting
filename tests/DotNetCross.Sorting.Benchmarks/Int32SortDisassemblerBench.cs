using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    // Why does this work but not generic SortDissassemblerBench
    [Config(typeof(SortDisassemblerBenchConfig))]
    public class Int32SortDisassemblerBenchSpecial
    {
        const int MaxLength = 3 * 1000 * 1000;
        static readonly int[] _filled = new int[MaxLength];
        int[] _work = new int[MaxLength];

        public Int32SortDisassemblerBenchSpecial()
        {
            Length = 100000;
        }

        public int Length { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var filler = new MedianOfThreeKillerSpanFiller();
            Console.WriteLine($"// {nameof(GlobalSetup)} Filling {MaxLength} with {filler.GetType().Name} for {Length} slice run");
            filler.Fill(_filled, Length, i => i);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            Console.WriteLine($"// {nameof(IterationSetup)} Copy filled to work {Length}");
            Array.Copy(_filled, _work, MaxLength);
        }

        [Benchmark]
        public void SpanSort()
        {
            //int i = 0;
            // NOTE: IF FOR LOOP REMOVED CODE-GEN IS COMPLETELY DIFFERENT
            for (int i = 0; i <= MaxLength - Length; i += Length)
            {
                new Span<int>(_work, i, Length).Sort();
            }
        }
    }
}
