﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    //[DisassemblyDiagnoser(printAsm: true, printSource: true, recursiveDepth: 4)]
    //[DisassemblyDiagnoser(recursiveDepth: 2)]
    //[SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 3, targetCount: 3)]
    //[RyuJitX64Job()]
    // Why does this work but not SortDissassemblerBench
    [Config(typeof(SortDisassemblerBenchConfig))]
    public class Int32SortDisassemblerBench
    {
        const int MaxLength = 3 * 1000 * 1000;
        static readonly int[] _filled = new int[MaxLength];
        int[] _work = new int[MaxLength];

        public Int32SortDisassemblerBench()
        {
            Length = 1000000;
        }

        //[ParamsSource(nameof(Lengths))]
        public int Length { get; set; }

        //public IEnumerable<int> Lengths => new[] { 1000000 }; //1, 10, 100, 10000, 

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

        // Can't really use this for much for ints since it is native code and not disassembled...
        //[Benchmark(Baseline = true)]
        //public void ArraySort()
        //{
        //    //int i = 0;
        //    // NOTE: IF FOR LOOP REMOVED CODE-GEN IS COMPLETELY DIFFERENT IN FACT BDN DOES NOT FULLY DISASM IT
        //    for (int i = 0; i <= MaxLength - Length; i += Length)
        //    {
        //        Array.Sort(_work, i, Length);
        //    }
        //}

        [Benchmark]
        public void SpanSort()
        {
            //int i = 0;
            // NOTE: IF FOR LOOP REMOVED CODE-GEN IS COMPLETELY DIFFERENT IN FACT BDN DOES NOT FULLY DISASM IT
            for (int i = 0; i <= MaxLength - Length; i += Length)
            {
                new Span<int>(_work, i, Length).Sort();
            }
        }
        //int _length = 1024 * 1024;
        //int[] _filled;
        //int[] _work;

        //public Int32SortDisassemblerBench()
        //{
        //    _filled = CreateArray<int>(_length, i => i);
        //    _work = new int[_length];
        //}

        //[IterationSetup]
        //public void IterationSetup()
        //{
        //    Array.Copy(_filled, _work, _length);
        //}

        //[Benchmark(Baseline = true)]
        //public void ArraySort()
        //{
        //    Array.Sort(_work, 0, _length);
        //}

        //[Benchmark]
        //public void SpanSort()
        //{
        //    new Span<int>(_work, 0, _length).Sort();
        //}

        private static T[] CreateArray<T>(int length, Func<int, T> toValue)
            where T : IComparable<T>
        {
            var array = new T[length];
            return array;
        }
    }
}