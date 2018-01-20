// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using DotNetCross.Sorting.Sequences;
using Xunit;

namespace System.SpanTests
{
    public static partial class SpanTests
    {
        // Existing coreclr tests seem to be in here:
        // https://github.com/dotnet/coreclr/tree/master/tests/src/CoreMangLib/cti/system/array
        // E.g. arraysort1.cs etc.

        //public static readonly TheoryData<uint[]> s_sortCasesUInt =
        //new TheoryData<uint[]> {
        //    ,
        //};

        // How do we create a not comparable? I.e. something Comparer<T>.Default fails on?
        //struct NotComparable { int i; string s; IntPtr p; }
        //[Fact]
        //[Trait("MyTrait", "MyTraitValue")]
        //public static void Sort_NotComparableThrows()
        //{
        //    var comparer = Comparer<NotComparable>.Default;
        //    Assert.Throws<ArgumentNullException>(() => new Span<NotComparable>(new NotComparable[16])
        //        .Sort());
        //    Assert.Throws<ArgumentNullException>(() => new Span<NotComparable>(new NotComparable[16])
        //        .Sort(comparer));
        //}

        [Fact]
        [Trait("MyTrait", "MyTraitValue")]
        public static void Sort_NullComparerDoesNotThrow()
        {
            new Span<int>(new int[] { 3 }).Sort((IComparer<int>)null);
        }

        [Fact]
        [Trait("MyTrait", "MyTraitValue")]
        public static void Sort_NullComparisonThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Span<int>(new int[] { }).Sort((Comparison<int>)null));
            Assert.Throws<ArgumentNullException>(() => new Span<string>(new string[] { }).Sort((Comparison<string>)null));
        }

        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [InlineData(new uint[] { })]
        [InlineData(new uint[] { 1 })]
        [InlineData(new uint[] { 2, 1})]
        [InlineData(new uint[] { 3, 1, 2})]
        [InlineData(new uint[] { 3, 2, 1})]
        [InlineData(new uint[] { 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 1, 2, 3, 4, 7, 6, 5 })]
        public static void Sort_UInt(uint[] unsorted)
        {
            TestSortOverloads(unsorted);
        }

        // TODO: OuterLoop
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [InlineData(17, 1024)]
        [InlineData(42, 1024)]
        [InlineData(1873318, 1024)]
        public static void Sort_Random_Int(int seed, int maxCount)
        {
            var random = new Random(seed);
            for (int count = 0; count < maxCount; count++)
            {
                var unsorted = Enumerable.Range(0, count).Select(i => random.Next()).ToArray();
                TestSortOverloads(unsorted);
            }
        }

        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [InlineData(17, 1024)]
        [InlineData(42, 1024)]
        [InlineData(1873318, 1024)]
        public static void Sort_Random_Float(int seed, int maxCount)
        {
            var random = new Random(seed);
            for (int count = 0; count < maxCount; count++)
            {
                var unsorted = Enumerable.Range(0, count).Select(i => (float)random.Next()).ToArray();
                TestSortOverloads(unsorted);
            }
        }

        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [InlineData(17, 512)]
        [InlineData(42, 512)]
        [InlineData(1873318, 512)]
        public static void Sort_Random_String(int seed, int maxCount)
        {
            var random = new Random(seed);
            for (int count = 0; count < maxCount; count++)
            {
                var unsorted = Enumerable.Range(0, count).Select(i => random.Next().ToString("D9")).ToArray();
                TestSortOverloads(unsorted);
            }
        }

        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [InlineData(1024)]
        public static void Sort_MedianOfThreeKiller_Int(int maxCount)
        {
            var filler = new MedianOfThreeKillerSpanFiller();
            for (int count = 0; count < maxCount; count++)
            {
                var unsorted = new int[count];
                filler.Fill(unsorted, count, i => i);
                TestSortOverloads(unsorted);
            }
        }

        // TODO: OuterLoop
        [Fact]
        [Trait("MyTrait", "MyTraitValue")]
        public static void Sort_Reverse_Int()
        {
            for (int count = 1; count <= 256 * 1024; count <<= 1)
            {
                var unsorted = Enumerable.Range(0, count).Reverse().ToArray();
                TestSortOverloads(unsorted);
            }
        }

        // TODO: OuterLoop
        [Fact]
        [Trait("MyTrait", "MyTraitValue")]
        public static void Sort_AlreadySorted_Int()
        {
            for (int count = 1; count <= 256 * 1024; count <<= 1)
            {
                var unsorted = Enumerable.Range(0, count).ToArray();
                TestSortOverloads(unsorted);
            }
        }

        // TODO: OuterLoop
        [Fact]
        [Trait("MyTrait", "MyTraitValue")]
        public static void SortWithItems_Reverse_Int()
        {
            for (int count = 1; count <= 256 * 1024; count <<= 1)
            {
                var unsorted = Enumerable.Range(0, count).Reverse().ToArray();
                var unsortedItems = Enumerable.Range(0, count).ToArray();
                TestSortOverloads(unsorted, unsortedItems);
            }
        }

        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [InlineData(17, 1024)]
        [InlineData(42, 1024)]
        [InlineData(1873318, 1024)]
        public static void SortWithItems_Random_Int(int seed, int maxCount)
        {
            var random = new Random(seed);
            for (int count = 0; count < maxCount; count++)
            {
                var unsorted = Enumerable.Range(0, count).Select(i => random.Next()).ToArray();
                var unsortedItems = Enumerable.Range(0, count).ToArray();
                TestSortOverloads(unsorted, unsortedItems);
            }
        }

        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [InlineData(1024)]
        public static void SortWithItems_MedianOfThreeKiller_Int(int maxCount)
        {
            var filler = new MedianOfThreeKillerSpanFiller();
            for (int count = 0; count < maxCount; count++)
            {
                var unsorted = new int[count];
                filler.Fill(unsorted, count, i => i);
                var unsortedItems = Enumerable.Range(0, count).ToArray();
                TestSortOverloads(unsorted, unsortedItems);
            }
        }

        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [InlineData(new uint[] { })]
        [InlineData(new uint[] { 1 })]
        [InlineData(new uint[] { 2, 1 })]
        [InlineData(new uint[] { 3, 1, 2 })]
        [InlineData(new uint[] { 3, 2, 1 })]
        [InlineData(new uint[] { 3, 2, 4, 1 })]
        [InlineData(new uint[] { 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 1, 2, 3, 4, 7, 6, 5 })]
        public static void SortWithItems_UInt(uint[] unsorted)
        {
            var unsortedItems = Enumerable.Range(0, unsorted.Length).ToArray();
            TestSortOverloads(unsorted, unsortedItems);
        }

        //[Fact]
        //public static void Sort_Slice()
        //{
        //    var array = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        //    var span = new ReadOnlySpan<int>(array, 1, array.Length - 2);

        //    Assert.Equal(-1, span.Sort(1));
        //    Assert.Equal(0, span.Sort(2));
        //    Assert.Equal(3, span.Sort(5));
        //    Assert.Equal(6, span.Sort(8));
        //    Assert.Equal(-8, span.Sort(9));
        //}

        //[Fact]
        //public static void Sort_NullComparableThrows()
        //{
        //    Assert.Throws<ArgumentNullException>(() => new Span<int>(new int[] { }).Sort<int>(null));
        //    Assert.Throws<ArgumentNullException>(() => new ReadOnlySpan<int>(new int[] { }).Sort<int>(null));
        //    Assert.Throws<ArgumentNullException>(() => new Span<int>(new int[] { }).Sort<int, IComparable<int>>(null));
        //    Assert.Throws<ArgumentNullException>(() => new ReadOnlySpan<int>(new int[] { }).Sort<int, IComparable<int>>(null));
        //}

        //// TODO: Revise whether this should actually throw
        //[Fact]
        //public static void Sort_NullComparerThrows()
        //{
        //    Assert.Throws<ArgumentNullException>(() => new Span<int>(new int[] { }).Sort<int, IComparer<int>>(0, null));
        //    Assert.Throws<ArgumentNullException>(() => new ReadOnlySpan<int>(new int[] { }).Sort<int, IComparer<int>>(0, null));
        //}

        // NOTE: Sort_MaxLength_NoOverflow test is constrained to run on Windows and MacOSX because it causes
        //       problems on Linux due to the way deferred memory allocation works. On Linux, the allocation can
        //       succeed even if there is not enough memory but then the test may get killed by the OOM killer at the
        //       time the memory is accessed which triggers the full memory allocation.
        //[Fact]
        //[OuterLoop]
        //[PlatformSpecific(TestPlatforms.Windows | TestPlatforms.OSX)]
        //public unsafe static void Sort_MaxLength_NoOverflow()
        //{
        //    if (sizeof(IntPtr) == sizeof(long))
        //    {
        //        // Allocate maximum length span native memory
        //        var length = int.MaxValue;
        //        if (!AllocationHelper.TryAllocNative(new IntPtr(length), out IntPtr memory))
        //        {
        //            Console.WriteLine($"Span.Sort test {nameof(Sort_MaxLength_NoOverflow)} skipped (could not alloc memory).");
        //            return;
        //        }
        //        try
        //        {
        //            var span = new Span<byte>(memory.ToPointer(), length);
        //            //span.Fill(0);
        //            //// Fill last two elements
        //            //span[int.MaxValue - 2] = 2;
        //            //span[int.MaxValue - 1] = 3;

        //            //Assert.Equal(int.MaxValue / 2, span.Sort((byte)0));
        //            //// Search at end and assert no overflow
        //            //Assert.Equal(~(int.MaxValue - 2), span.Sort((byte)1));
        //            //Assert.Equal(int.MaxValue - 2, span.Sort((byte)2));
        //            //Assert.Equal(int.MaxValue - 1, span.Sort((byte)3));
        //            //Assert.Equal(int.MinValue, span.Sort((byte)4));
        //        }
        //        finally
        //        {
        //            AllocationHelper.ReleaseNative(ref memory);
        //        }
        //    }
        //}

        private static void TestSortOverloads<T>(T[] array)
            where T : IComparable<T>
        {
            TestSpan(array);
            TestComparerSpan(array);
            TestComparisonSpan(array);
        }

        private static void TestSpan<T>(T[] array)
            where T : IComparable<T>
        {
            var span = new Span<T>(array);
            var expected = (T[])array.Clone();
            Array.Sort(expected);

            span.Sort();

            Assert.Equal(expected, array);
        }
        private static void TestComparerSpan<T>(T[] array)
            where T : IComparable<T>
        {
            var span = new Span<T>(array);
            var expected = (T[])array.Clone();
            Array.Sort(expected);

            span.Sort(Comparer<T>.Default);

            Assert.Equal(expected, array);
        }
        private static void TestComparisonSpan<T>(T[] array)
            where T : IComparable<T>
        {
            var span = new Span<T>(array);
            var expected = (T[])array.Clone();
            Array.Sort(expected);

            span.Sort(Comparer<T>.Default.Compare);

            Assert.Equal(expected, array);
        }


        private static void TestSortOverloads<TKey, TValue>(TKey[] keys, TValue[] values)
            where TKey : IComparable<TKey>
        {
            TestSpan(keys, values);
            TestComparerSpan(keys, values);
            TestComparisonSpan(keys, values);
        }

        private static void TestSpan<TKey, TValue>(TKey[] keys, TValue[] values)
            where TKey : IComparable<TKey>
        {
            var expectedKeys = (TKey[])keys.Clone();
            var expectedValues = (TValue[])values.Clone();
            Array.Sort(expectedKeys, expectedValues);

            var spanKeys = new Span<TKey>(keys);
            var spanValues = new Span<TValue>(values);
            spanKeys.Sort(spanValues);

            Assert.Equal(expectedKeys, keys);
            Assert.Equal(expectedValues, values);
        }
        private static void TestComparerSpan<TKey, TValue>(TKey[] keys, TValue[] values)
            where TKey : IComparable<TKey>
        {
            var expectedKeys = (TKey[])keys.Clone();
            var expectedValues = (TValue[])values.Clone();
            Array.Sort(expectedKeys, expectedValues);

            var spanKeys = new Span<TKey>(keys);
            var spanValues = new Span<TValue>(values);
            spanKeys.Sort(spanValues, Comparer<TKey>.Default);

            Assert.Equal(expectedKeys, keys);
            Assert.Equal(expectedValues, values);
        }
        private static void TestComparisonSpan<TKey, TValue>(TKey[] keys, TValue[] values)
            where TKey : IComparable<TKey>
        {
            var expectedKeys = (TKey[])keys.Clone();
            var expectedValues = (TValue[])values.Clone();
            Array.Sort(expectedKeys, expectedValues);

            var spanKeys = new Span<TKey>(keys);
            var spanValues = new Span<TValue>(values);
            spanKeys.Sort(spanValues, Comparer<TKey>.Default.Compare);

            Assert.Equal(expectedKeys, keys);
            Assert.Equal(expectedValues, values);
        }
    }
}
