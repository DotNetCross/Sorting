using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

    public struct ComparableStructInt32 : IComparable<ComparableStructInt32>
    {
        public readonly int Value;

        public ComparableStructInt32(int value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ComparableStructInt32 other)
        {
            return this.Value.CompareTo(other.Value);
        }
    }

    public class ComparableClassInt32 : IComparable<ComparableClassInt32>
    {
        public readonly int Value;

        public ComparableClassInt32(int value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ComparableClassInt32 other)
        {
            return this.Value.CompareTo(other.Value);
        }
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
            : base(maxLength: 100000, new[] { 2, 3, 10, 100, 1000, 10000, 50000 },
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
            : base(maxLength: 100000, new[] { 2, 3, 10, 100, 1000, 10000, 50000 },
                   SpanFillers.Default, i => new ComparableClassInt32(i))
        { }
    }
    public class Int32Int32SortBench : SortBench<int, int>
    {
        public Int32Int32SortBench()
            : base(maxLength: 2000000, SpanFillers.Default, i => i, i => i)
        { }
    }
    public class Int32SingleSortBench : SortBench<int, float>
    {
        public Int32SingleSortBench()
            : base(maxLength: 2000000, SpanFillers.Default, i => i, i => i)
        { }
    }
    public class Int32StringSortBench : SortBench<int, string>
    {
        public Int32StringSortBench()
            : base(maxLength: 2000000, SpanFillers.Default, i => i, i => i.ToString("D9"))
        { }
    }

    // BDN fails
    public class Int32SortDisassemblerBenchNotWorking : SortDisassemblerBench<int>
    {
        public Int32SortDisassemblerBenchNotWorking()
            : base(length: 1024 * 1024, i => i)
        { }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (true && !Debugger.IsAttached)
            {
                //BenchmarkRunner.Run<CompareToLessThanBench>();

                //BenchmarkRunner.Run<Int32SortBench>();
                //BenchmarkRunner.Run<SingleSortBench>();
                //BenchmarkRunner.Run<ComparableStructInt32SortBench>();
                BenchmarkRunner.Run<ComparableClassInt32SortBench>();
                BenchmarkRunner.Run<StringSortBench>();
                
                //BenchmarkRunner.Run<Int32StringSortBench>();
                //BenchmarkRunner.Run<Int32SingleSortBench>();
                //BenchmarkRunner.Run<Int32Int32SortBench>();

                //BenchmarkRunner.Run<Int32SortDisassemblerBench>();
                //BenchmarkRunner.Run<StringSortDisassemblerBench>();
                //BenchmarkRunner.Run<Int32SortDisassemblerBench>();

                //BenchmarkRunner.Run<Int32SortDisassemblerBenchNotWorking>(); // Fails, probably due to generics...

                // Micro benchmarks
                //BenchmarkRunner.Run<IntPtrHelperBenchmark>();
                //BenchmarkRunner.Run<CompareAsm>();
            }
            else if (true)
            {
                var sut = new ComparableClassInt32SortBench();
                //var sut = new StringSortBench();
                sut.Filler = new RandomSpanFiller(SpanFillers.RandomSeed);
                sut.Length = 1000; // 1000000;
                sut.GlobalSetup();
                sut.IterationSetup();
                sut.SpanSort();
                sut.IterationSetup();
                sut.SpanSort();

                Console.WriteLine("Enter key...");
                Console.ReadKey();

                for (int i = 0; i < 1000; i++)
                {
                    sut.IterationSetup();
                    sut.SpanSort();
                }
            }
            else
            {
                var sut = new CompareToLessThanBench();
                sut.OpenDelegate();
                //var filler = new RandomSpanFiller(SpanFillers.RandomSeed);
                //const int length = 1000000;
                //var strings = new string[length];
                //var keys = new Span<string>(strings);
                //filler.Fill(keys, length, i => i.ToString("D9"));
                //ref var keysRef = ref keys.DangerousGetPinnableReference();
                //int count = Test.CountLessThan(ref Unsafe.As<string, Reference<string>>(ref keysRef),
                //    length, new Test.IComparableLessThanComparer<string>());
                //Console.WriteLine(count);
                //Console.WriteLine("Enter key...");
                //Console.ReadKey();
                //int totalSum = 0;
                //for (int i = 0; i < 1000; i++)
                //{
                //    totalSum += Test.CountLessThan(ref Unsafe.As<string, Reference<string>>(ref keysRef),
                //        length, new Test.IComparableLessThanComparer<string>());

                //}
                //Console.WriteLine(totalSum);
            }
        }



        internal struct Reference<T>
        {
            internal object o;
        }

        internal static class Test
        {
            internal interface ILessThanComparer<in T>
            {
                bool LessThan(T x, T y);
            }

            internal struct IComparableLessThanComparer<T>
                : ILessThanComparer<Reference<T>>
                where T : IComparable<T>
            {
                public bool LessThan(Reference<T> x, Reference<T> y)
                {
                    return Unsafe.As<IComparable<T>>(x.o).CompareTo(Unsafe.As<Reference<T>, T>(ref y)) < 0;
                }
            }

            public static int CountLessThan<TReference, TComparer>(ref TReference keys, int length, TComparer comparer)
                where TComparer : ILessThanComparer<TReference>
            {
                int count = 0;
                for (int i = 0; i < length - 1; i++)
                {
                    ref var a = ref Unsafe.Add(ref keys, i);
                    ref var b = ref Unsafe.Add(ref keys, i + 1);
                    if (comparer.LessThan(a, b))
                    {
                        ++count;
                    }
                }
                return count;
            }
        }
    }
}
