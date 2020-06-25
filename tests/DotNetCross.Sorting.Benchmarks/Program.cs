using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    unsafe class C<T>
    {
        public static void Log(T t) { }

        void Use()
        {
            delegate*< T, void> ptr1 = &Log;
            ptr1(default);
        }
    }

    class Program
    {
        enum Do { Focus, Full, Micro, Keys1, Keys2, KeysValues1 }

        static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            // TODO: Refactor to switch/case and methods perhaps, less flexible though
            // TODO: Add argument parsing for this perhaps
            var d = Debugger.IsAttached ? Do.Keys1 : Do.KeysValues1;
            if (d == Do.Focus)
            {
                //BenchmarkRunner.Run<Int32StringPartitionBench>();

                //BenchmarkRunner.Run<Int32SortBench>();
                //BenchmarkRunner.Run<SingleSortBench>();
                BenchmarkRunner.Run<ComparableStructInt32SortBench>();
                //BenchmarkRunner.Run<ComparableClassInt32SortBench>();
                //BenchmarkRunner.Run<StringSortBench>();

                //BenchmarkRunner.Run<Int32SingleSortBench>();
                //BenchmarkRunner.Run<Int32Int32SortBench>();
                //BenchmarkRunner.Run<SingleInt32SortBench>();
                //BenchmarkRunner.Run<SingleSingleSortBench>();
                //BenchmarkRunner.Run<Int32StringSortBench>();
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
            else if (d == Do.Keys1)
            {
                //var sut = new ComparableClassInt32SortBench();
                var sut = new ComparableStructInt32SortBench();
                //var sut = new StringSortBench();
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
            else if (d == Do.Keys2)
            {
                var sut = new ComparableClassInt32SortBench();
                //var sut = new ComparableClassInt32Int32SortBench();
                //var sut = new StringInt32SortBench();
                sut.Filler = new RandomSpanFiller(SpanFillers.RandomSeed);
                sut.Length = 10000; // 1000000;

                sut.GlobalSetup();
                sut.IterationSetup();
                sut.DNX_Comparison_IComparable_OpenDelegate();
                sut.IterationSetup();
                sut.DNX_OpenDelegateObjectComparer();

                //Console.WriteLine("Enter key...");
                //Console.ReadKey();

                for (int i = 0; i < 200; i++)
                {
                    sut.IterationSetup();
                    sut.DNX_Comparison_IComparable_OpenDelegate();
                    sut.IterationSetup();
                    sut.DNX_OpenDelegateObjectComparer();
                }
            }
            else if (d == Do.KeysValues1)
            {
                var sut = new Int32StringPartitionBench();
                //var sut = new ComparableClassInt32Int32SortBench();
                //var sut = new StringInt32SortBench();
                sut.Filler = new RandomSpanFiller(SpanFillers.RandomSeed);
                sut.Length = 1000000;

                sut.GlobalSetup();
                sut.IterationSetup();
                sut.DNX_();
                sut.IterationSetup();
                sut.CLR_();

                //Console.WriteLine("Enter key...");
                //Console.ReadKey();

                for (int i = 0; i < 40; i++)
                {
                    sut.IterationSetup();
                    sut.DNX_();
                    sut.IterationSetup();
                    sut.CLR_();
                }
            }
        }
    }
}
