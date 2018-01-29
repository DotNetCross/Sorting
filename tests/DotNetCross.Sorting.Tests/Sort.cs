#define OUTER_LOOP
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

        const int FastMaxLength = 50;
        const int SlowMaxLength = 512;

        public static readonly TheoryData<SortCase> s_fastSortCases = CreateSortCases(FastMaxLength);
        public static readonly TheoryData<SortCase> s_slowSortCases = CreateSortCases(SlowMaxLength);

        public class SortCase
        {
            public SortCase(int maxLength, ISpanFiller filler)
            {
                if (filler == null) { throw new ArgumentNullException(nameof(filler)); }
                MaxLength = maxLength;
                Filler = filler;
            }

            public int MaxLength { get; }
            public ISpanFiller Filler { get; }

            public override string ToString()
            {
                return $"MaxLength={MaxLength,4} {nameof(Filler)}={Filler.GetType().Name.Replace("SpanFiller", "")} ";
            }
        }

        static TheoryData<SortCase> CreateSortCases(int maxLength)
        {
            return new TheoryData<SortCase> {
                new SortCase(maxLength, new ConstantSpanFiller(42) ),
                new SortCase(maxLength, new DecrementingSpanFiller() ),
                new SortCase(maxLength, new IncrementingSpanFiller() ),
                new SortCase(maxLength, new MedianOfThreeKillerSpanFiller() ),
                new SortCase(maxLength, new PartialRandomShuffleSpanFiller(new IncrementingSpanFiller(), 0.2, 16281) ),
                new SortCase(maxLength, new RandomSpanFiller(1873318) ),
                // TODO: Add with some -1 that can be replaced with null or NaN or something
            };
        }

        // To run just these tests append to command line:
        // -trait "MyTrait=MyTraitValue"

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

        #region Keys Tests

        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_Int16(SortCase sortCase)
        {
            Test(sortCase, i => (short)i);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_UInt16(SortCase sortCase)
        {
            Test(sortCase, i => (ushort)i);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_Int32(SortCase sortCase)
        {
            Test(sortCase, i => i);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_UInt32(SortCase sortCase)
        {
            Test(sortCase, i => (uint)i);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_Int64(SortCase sortCase)
        {
            Test(sortCase, i => (long)i);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_UInt64(SortCase sortCase)
        {
            Test(sortCase, i => (ulong)i);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_Single(SortCase sortCase)
        {
            Test(sortCase, i => (float)i);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_Double(SortCase sortCase)
        {
            Test(sortCase, i => (double)i);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_Boolean(SortCase sortCase)
        {
            Test(sortCase, i => i % 2 == 0);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_Char(SortCase sortCase)
        {
            Test(sortCase, i => (char)i);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_String(SortCase sortCase)
        {
            Test(sortCase, i => i.ToString("D9"));
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_ComparableStructInt32(SortCase sortCase)
        {
            Test(sortCase, i => new ComparableStructInt32(i));
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_Keys_ComparableClassInt32(SortCase sortCase)
        {
            Test(sortCase, i => new ComparableClassInt32(i));
        }
#if OUTER_LOOP
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_Int16_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (short)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_UInt16_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (ushort)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => i);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_UInt32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (uint)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_Int64_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (long)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_UInt64_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (ulong)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_Single_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (float)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_Double_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (double)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_Boolean_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => i % 2 == 0);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_Char_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (char)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_String_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => i.ToString("D9"));
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_ComparableStructInt32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => new ComparableStructInt32(i));
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_Keys_ComparableClassInt32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => new ComparableClassInt32(i));
        }
#endif

        #endregion

        #region Keys and Values Tests
        [Fact]
        public static void Sort_KeysValues_Int16_Int32_SwapsIfSameValuesShouldnt()
        {
            TestSortOverloads(new short[] { 42, 42, 42 }, new int[] { 0, 1, 2 });
        }


        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_Int16_Int32(SortCase sortCase)
        {
            Test(sortCase, i => (short)i, v => v);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_UInt16_Int32(SortCase sortCase)
        {
            Test(sortCase, i => (ushort)i, v => v);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_Int32_Int32(SortCase sortCase)
        {
            Test(sortCase, i => i, v => v);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_UInt32_Int32(SortCase sortCase)
        {
            Test(sortCase, i => (uint)i, v => v);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_Int64_Int32(SortCase sortCase)
        {
            Test(sortCase, i => (long)i, v => v);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_UInt64_Int32(SortCase sortCase)
        {
            Test(sortCase, i => (ulong)i, v => v);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_Single_Int32(SortCase sortCase)
        {
            Test(sortCase, i => (float)i, v => v);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_Double_Int32(SortCase sortCase)
        {
            Test(sortCase, i => (double)i, v => v);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_Boolean_Int32(SortCase sortCase)
        {
            Test(sortCase, i => i % 2 == 0, v => v);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_Char_Int32(SortCase sortCase)
        {
            Test(sortCase, i => (char)i, v => v);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_String_Int32(SortCase sortCase)
        {
            Test(sortCase, i => i.ToString("D9"), v => v);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_ComparableStructInt32_Int32(SortCase sortCase)
        {
            Test(sortCase, i => new ComparableStructInt32(i), v => v);
        }
        
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_fastSortCases))]
        public static void Sort_KeysValues_ComparableClassInt32_Int32(SortCase sortCase)
        {
            Test(sortCase, i => new ComparableClassInt32(i), v => v);
        }
#if OUTER_LOOP
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_Int16_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (short)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_UInt16_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (ushort)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_Int32_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_UInt32_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (uint)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_Int64_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (long)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_UInt64_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (ulong)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_Single_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (float)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_Double_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (double)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_Boolean_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => i % 2 == 0, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_Char_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => (char)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_String_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => i.ToString("D9"), v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_ComparableStructInt32_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => new ComparableStructInt32(i), v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait("MyTrait", "MyTraitValue")]
        [MemberData(nameof(s_slowSortCases))]
        public static void Sort_KeysValues_ComparableClassInt32_Int32_OuterLoop(SortCase sortCase)
        {
            Test(sortCase, i => new ComparableClassInt32(i), v => v);
        }
#endif
        //// TODO: OuterLoop
        //[Fact]
        //[Trait("MyTrait", "MyTraitValue")]
        //public static void SortWithItems_Reverse_Int_Int()
        //{
        //    for (int count = 1; count <= 256 * 1024; count <<= 1)
        //    {
        //        var unsorted = Enumerable.Range(0, count).Reverse().ToArray();
        //        var unsortedItems = Enumerable.Range(0, count).ToArray();
        //        TestSortOverloads(unsorted, unsortedItems);
        //    }
        //}

        //[Theory]
        //[Trait("MyTrait", "MyTraitValue")]
        //[InlineData(17, 1024)]
        //[InlineData(42, 1024)]
        //[InlineData(1873318, 1024)]
        //public static void SortWithItems_Random_Int_Int(int seed, int maxCount)
        //{
        //    var random = new Random(seed);
        //    for (int count = 0; count < maxCount; count++)
        //    {
        //        var unsorted = Enumerable.Range(0, count).Select(i => random.Next()).ToArray();
        //        var unsortedItems = Enumerable.Range(0, count).ToArray();
        //        TestSortOverloads(unsorted, unsortedItems);
        //    }
        //}

        //[Theory]
        //[Trait("MyTrait", "MyTraitValue")]
        //[InlineData(1024)]
        //public static void SortWithItems_MedianOfThreeKiller_Int_Int(int maxCount)
        //{
        //    var filler = new MedianOfThreeKillerSpanFiller();
        //    for (int count = 0; count < maxCount; count++)
        //    {
        //        var unsorted = new int[count];
        //        filler.Fill(unsorted, count, i => i);
        //        var unsortedItems = Enumerable.Range(0, count).ToArray();
        //        TestSortOverloads(unsorted, unsortedItems);
        //    }
        //}

        //[Theory]
        //[Trait("MyTrait", "MyTraitValue")]
        //[InlineData(new uint[] { })]
        //[InlineData(new uint[] { 1 })]
        //[InlineData(new uint[] { 2, 1 })]
        //[InlineData(new uint[] { 3, 1, 2 })]
        //[InlineData(new uint[] { 3, 2, 1 })]
        //[InlineData(new uint[] { 3, 2, 4, 1 })]
        //[InlineData(new uint[] { 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 1, 2, 3, 4, 7, 6, 5 })]
        //public static void SortWithItems_UInt_UInt(uint[] unsorted)
        //{
        //    var unsortedItems = Enumerable.Range(0, unsorted.Length).ToArray();
        //    TestSortOverloads(unsorted, unsortedItems);
        //}

        //[Theory]
        //[Trait("MyTrait", "MyTraitValue")]
        //[InlineData(17, 1024)]
        //[InlineData(42, 1024)]
        //[InlineData(1873318, 1024)]
        //public static void SortWithItems_Random_String_Int(int seed, int maxCount)
        //{
        //    var random = new Random(seed);
        //    for (int count = 0; count < maxCount; count++)
        //    {
        //        var unsorted = Enumerable.Range(0, count).Select(i => random.Next().ToString("D9")).ToArray();
        //        var unsortedItems = Enumerable.Range(0, count).ToArray();
        //        TestSortOverloads(unsorted, unsortedItems);
        //    }
        //}

        //[Theory]
        //[Trait("MyTrait", "MyTraitValue")]
        //[InlineData(17, 1024)]
        //[InlineData(42, 1024)]
        //[InlineData(1873318, 1024)]
        //public static void SortWithItems_Random_Int_String(int seed, int maxCount)
        //{
        //    var random = new Random(seed);
        //    for (int count = 0; count < maxCount; count++)
        //    {
        //        var unsorted = Enumerable.Range(0, count).Select(i => random.Next()).ToArray();
        //        var unsortedItems = Enumerable.Range(0, count).Select(i => i.ToString("D9")).ToArray();
        //        TestSortOverloads(unsorted, unsortedItems);
        //    }
        //}

        #endregion

        // NOTE: Sort_MaxLength_NoOverflow test is constrained to run on Windows and MacOSX because it causes
        //       problems on Linux due to the way deferred memory allocation works. On Linux, the allocation can
        //       succeed even if there is not enough memory but then the test may get killed by the OOM killer at the
        //       time the memory is accessed which triggers the full memory allocation.
        //[Fact]
        //[Trait("MyTrait", "MyTraitValue")]
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
        //            var filler = new DecrementingSpanFiller();
        //            const byte fill = 42;
        //            span.Fill(fill);
        //            span[0] = 255;
        //            span[1] = 254;
        //            span[span.Length - 2] = 1;
        //            span[span.Length - 1] = 0;
        //
        //            span.Sort();
        //
        //            Assert.Equal(span[0], (byte)0);
        //            Assert.Equal(span[1], (byte)1);
        //            Assert.Equal(span[span.Length - 2], (byte)254);
        //            Assert.Equal(span[span.Length - 1], (byte)255);
        //            for (int i = 2; i < length - 2; i++)
        //            {
        //                Assert.Equal(span[i], fill);
        //            }
        //        }
        //        finally
        //        {
        //            AllocationHelper.ReleaseNative(ref memory);
        //        }
        //    }
        //}


        private static void Test<TKey>(SortCase sortCase, Func<int, TKey> toKey)
            where TKey : IComparable<TKey>
        {
            for (int length = 0; length < sortCase.MaxLength; length++)
            {
                var unsorted = new TKey[length];
                sortCase.Filler.Fill(unsorted, length, toKey);
                TestSortOverloads(unsorted);
            }
        }
        private static void Test<TKey, TValue>(SortCase sortCase, Func<int, TKey> toKey, Func<int, TValue> toValue)
            where TKey : IComparable<TKey>
        {
            for (int length = 0; length < sortCase.MaxLength; length++)
            {
                var unsortedKeys = new TKey[length];
                var unsortedValues = new TValue[length];
                sortCase.Filler.Fill(unsortedKeys, length, toKey);
                // Items are always based on "unique" int values
                new IncrementingSpanFiller().Fill(unsortedValues, length, toValue);
                TestSortOverloads(unsortedKeys, unsortedValues);
            }
        }

        private static void TestSortOverloads<T>(T[] array)
            where T : IComparable<T>
        {
            TestSpan(array);
            TestComparerSpan(array);
            TestComparisonSpan(array);
            TestCustomComparerSpan(array);
            TestNullComparerSpan(array);
        }

        private static void TestSpan<T>(T[] array)
            where T : IComparable<T>
        {
            var copy = (T[])array.Clone();
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
        private static void TestCustomComparerSpan<T>(T[] array)
            where T : IComparable<T>
        {
            var span = new Span<T>(array);
            var expected = (T[])array.Clone();
            Array.Sort(expected);

            span.Sort(new CustomComparer<T>());

            Assert.Equal(expected, array);
        }
        private static void TestNullComparerSpan<T>(T[] array)
            where T : IComparable<T>
        {
            var span = new Span<T>(array);
            var expected = (T[])array.Clone();
            Array.Sort(expected);

            span.Sort((IComparer<T>)null);

            Assert.Equal(expected, array);
        }


        private static void TestSortOverloads<TKey, TValue>(TKey[] keys, TValue[] values)
            where TKey : IComparable<TKey>
        {
            TestSpan(keys, values);
            TestComparerSpan(keys, values);
            TestComparisonSpan(keys, values);
            TestCustomComparerSpan(keys, values);
            TestNullComparerSpan(keys, values);
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
        private static void TestCustomComparerSpan<TKey, TValue>(TKey[] keys, TValue[] values)
            where TKey : IComparable<TKey>
        {
            var expectedKeys = (TKey[])keys.Clone();
            var expectedValues = (TValue[])values.Clone();
            Array.Sort(expectedKeys, expectedValues);

            var spanKeys = new Span<TKey>(keys);
            var spanValues = new Span<TValue>(values);
            spanKeys.Sort(spanValues, new CustomComparer<TKey>());

            Assert.Equal(expectedKeys, keys);
            Assert.Equal(expectedValues, values);
        }
        private static void TestNullComparerSpan<TKey, TValue>(TKey[] keys, TValue[] values)
            where TKey : IComparable<TKey>
        {
            var expectedKeys = (TKey[])keys.Clone();
            var expectedValues = (TValue[])values.Clone();
            Array.Sort(expectedKeys, expectedValues);

            var spanKeys = new Span<TKey>(keys);
            var spanValues = new Span<TValue>(values);
            spanKeys.Sort(spanValues, (IComparer<TKey>)null);

            Assert.Equal(expectedKeys, keys);
            Assert.Equal(expectedValues, values);
        }

        internal struct CustomComparer<T> : IComparer<T>
            where T : IComparable<T>
        {
            public int Compare(T x, T y) => x.CompareTo(y);
        }

        public struct ComparableStructInt32 : IComparable<ComparableStructInt32>
        {
            public readonly int Value;

            public ComparableStructInt32(int value)
            {
                Value = value;
            }

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

            public int CompareTo(ComparableClassInt32 other)
            {
                return this.Value.CompareTo(other.Value);
            }
        }
    }
}
