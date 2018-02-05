using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    // Why does this work but not generic SortDissassemblerBench
    [Config(typeof(SortDisassemblerBenchConfig))]
    public class StringSortDisassemblerBench
    {
        const int MaxLength = 6 * 100 * 100;
        static readonly string[] _filled = new string[MaxLength];
        string[] _work = new string[MaxLength];

        public StringSortDisassemblerBench()
        {
            Length = 1000;
        }

        public int Length { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var filler = new MedianOfThreeKillerSpanFiller();
            Console.WriteLine($"// {nameof(GlobalSetup)} Filling {MaxLength} with {filler.GetType().Name} for {Length} slice run");
            filler.Fill(_filled, Length, i => i.ToString("D9"));
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
                new Span<string>(_work, i, Length).Sort();
            }
        }
    }
}
