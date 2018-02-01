//#define OUTER_LOOP
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
        // These have not been used for the tests below,
        // instead all tests are based on using Array.Sort for computing the expected.

        const string SortTrait = nameof(SortTrait);
        const string SortTraitValue = nameof(SortTraitValue);

        const int FastMaxLength = 50;
        const int SlowMaxLength = 512;
        public static readonly TheoryData<ISortCases> s_fastSortTests = CreateSortCases(FastMaxLength);
        public static readonly TheoryData<ISortCases> s_slowSortTests = CreateSortCases(SlowMaxLength);

        static TheoryData<ISortCases> CreateSortCases(int maxLength)
        {
            var cases = new ISortCases[] {
                new LengthZeroSortCases(),
                new LengthOneSortCases(),
                new AllLengthTwoSortCases(),
                new AllLengthThreeSortCases(),
                new AllLengthFourSortCases(),
                new FillerSortCases(maxLength, new ConstantSpanFiller(42) ),
                new FillerSortCases(maxLength, new DecrementingSpanFiller() ),
                new FillerSortCases(maxLength, new IncrementingSpanFiller() ),
                new FillerSortCases(maxLength, new MedianOfThreeKillerSpanFiller() ),
                new FillerSortCases(maxLength, new PartialRandomShuffleSpanFiller(new IncrementingSpanFiller(), 0.2, 16281) ),
                new FillerSortCases(maxLength, new RandomSpanFiller(1873318) ),
                // TODO: Add with some -1 that can be replaced with null or NaN or something
            };
            var allCases = cases.Concat(cases.Select(c => new PadAndSliceSortCases(c, 2)))
                .Concat(cases.Select(c => new StepwiseSpecialSortCases(c, 3)));
            var theoryData = new TheoryData<ISortCases>();
            foreach (var c in allCases)
            {
                theoryData.Add(c);
            }
            return theoryData;
        }

        // To run just these tests append to command line:
        // -trait "MyTrait=MyTraitValue"

        // How do we create a not comparable? I.e. something Comparer<TKey>.Default fails on?
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
        [Trait(SortTrait, SortTraitValue)]
        public static void Sort_NullComparerDoesNotThrow()
        {
            new Span<int>(new int[] { 3 }).Sort((IComparer<int>)null);
        }

        [Fact]
        [Trait(SortTrait, SortTraitValue)]
        public static void Sort_NullComparisonThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Span<int>(new int[] { }).Sort((Comparison<int>)null));
            Assert.Throws<ArgumentNullException>(() => new Span<string>(new string[] { }).Sort((Comparison<string>)null));
        }



        [Fact]
        [Trait(SortTrait, SortTraitValue)]
        public static void Sort_Int32_BogusComparer()
        {
            TestSort(new ArraySegment<int>(new int[] { 0, 1 }), new BogusComparer<int>());
        }
        [Fact]
        [Trait(SortTrait, SortTraitValue)]
        public static void Sort_KeysValues_Single_NaN()
        {
            TestSort(new ArraySegment<float>(new [] { float.NaN, 0f, 0f, float.NaN }),
                     new ArraySegment<int>(new[] { 1, 2, 3, 4 }));
            // Array.Sort outputs: double.NaN, double.NaN, 0, 0, 
            //                              1,          4, 2, 3
            // Span.Sort outputs:  double.NaN, double.NaN, 0, 0, 
            //                              1,          4, 3, 2
            // Wouldn't span sort be correct? 2 <-> 4?
            // This should be exactly the same, why is Array.Sort giving this result?
        }
        [Fact]
        [Trait(SortTrait, SortTraitValue)]
        public static void Sort_KeysValues_Double_NaN()
        {
            TestSort(new ArraySegment<double>(new[] { double.NaN, 0.0, 0.0, double.NaN }),
                     new ArraySegment<int>(new[] { 1, 2, 3, 4 }));
            // Array.Sort outputs: double.NaN, double.NaN, 0, 0, 
            //                              1,          4, 2, 3
            // Span.Sort outputs:  double.NaN, double.NaN, 0, 0, 
            //                              1,          4, 3, 2
            // Wouldn't span sort be correct? 2 <-> 4?
            // This should be exactly the same, why is Array.Sort giving this result?
        }


        #region Keys Tests


        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Int8(ISortCases sortCases)
        {
            Test_Keys_Int8(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_UInt8(ISortCases sortCases)
        {
            Test_Keys_UInt8(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Int16(ISortCases sortCases)
        {
            Test_Keys_Int16(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_UInt16(ISortCases sortCases)
        {
            Test_Keys_UInt16(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Int32(ISortCases sortCases)
        {
            Test_Keys_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_UInt32(ISortCases sortCases)
        {
            Test_Keys_UInt32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Int64(ISortCases sortCases)
        {
            Test_Keys_Int64(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_UInt64(ISortCases sortCases)
        {
            Test_Keys_UInt64(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Single(ISortCases sortCases)
        {
            Test_Keys_Single(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Double(ISortCases sortCases)
        {
            Test_Keys_Double(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Boolean(ISortCases sortCases)
        {
            Test_Keys_Boolean(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Char(ISortCases sortCases)
        {
            Test_Keys_Char(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_String(ISortCases sortCases)
        {
            Test_Keys_String(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_ComparableStructInt32(ISortCases sortCases)
        {
            Test_Keys_ComparableStructInt32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_ComparableClassInt32(ISortCases sortCases)
        {
            Test_Keys_ComparableClassInt32(sortCases);
        }

        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_BogusComparable(ISortCases sortCases)
        {
            Test_Keys_BogusComparable(sortCases);
        }
#if OUTER_LOOP
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Int8_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_Int8(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_UInt8_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_UInt8(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Int16_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_Int16(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_UInt16_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_UInt16(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_UInt32_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_UInt32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Int64_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_Int64(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_UInt64_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_UInt64(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Single_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_Single(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Double_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_Double(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Boolean_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_Boolean(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Char_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_Char(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_String_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_String(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_ComparableStructInt32_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_ComparableStructInt32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_ComparableClassInt32_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_ComparableClassInt32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_BogusComparable_OuterLoop(ISortCases sortCases)
        {
            Test_Keys_BogusComparable(sortCases);
        }
#endif

        #endregion

        #region Keys and Values Tests


        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Int8_Int32(ISortCases sortCases)
        {
            Test_KeysValues_Int8_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_UInt8_Int32(ISortCases sortCases)
        {
            Test_KeysValues_UInt8_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Int16_Int32(ISortCases sortCases)
        {
            Test_KeysValues_Int16_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_UInt16_Int32(ISortCases sortCases)
        {
            Test_KeysValues_UInt16_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Int32_Int32(ISortCases sortCases)
        {
            Test_KeysValues_Int32_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_UInt32_Int32(ISortCases sortCases)
        {
            Test_KeysValues_UInt32_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Int64_Int32(ISortCases sortCases)
        {
            Test_KeysValues_Int64_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_UInt64_Int32(ISortCases sortCases)
        {
            Test_KeysValues_UInt64_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Single_Int32(ISortCases sortCases)
        {
            Test_KeysValues_Single_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Double_Int32(ISortCases sortCases)
        {
            Test_KeysValues_Double_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Boolean_Int32(ISortCases sortCases)
        {
            Test_KeysValues_Boolean_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Char_Int32(ISortCases sortCases)
        {
            Test_KeysValues_Char_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_String_Int32(ISortCases sortCases)
        {
            Test_KeysValues_String_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_ComparableStructInt32_Int32(ISortCases sortCases)
        {
            Test_KeysValues_ComparableStructInt32_Int32(sortCases);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_ComparableClassInt32_Int32(ISortCases sortCases)
        {
            Test_KeysValues_ComparableClassInt32_Int32(sortCases);
        }

        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_BogusComparable_Int32(ISortCases sortCases)
        {
            Test_KeysValues_BogusComparable_Int32(sortCases);
        }
#if OUTER_LOOP
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Int8_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_Int8_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_UInt8_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_UInt8_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Int16_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_Int16_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_UInt16_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_UInt16_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Int32_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_Int32_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_UInt32_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_UInt32_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Int64_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_Int64_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_UInt64_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_UInt64_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Single_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_Single_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Double_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_Double_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Boolean_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_Boolean_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Char_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_Char_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_String_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_String_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_ComparableStructInt32_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_ComparableStructInt32_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_ComparableClassInt32_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_ComparableClassInt32_Int32(sortCases);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_BogusComparable_Int32_OuterLoop(ISortCases sortCases)
        {
            Test_KeysValues_BogusComparable_Int32(sortCases);
        }
#endif
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

        static void Test_Keys_Int8(ISortCases sortCases) => 
            Test(sortCases, i => (sbyte)i, sbyte.MinValue);
        static void Test_Keys_UInt8(ISortCases sortCases) => 
            Test(sortCases, i => (byte)i, byte.MaxValue);
        static void Test_Keys_Int16(ISortCases sortCases) => 
            Test(sortCases, i => (short)i, short.MinValue);
        static void Test_Keys_UInt16(ISortCases sortCases) => 
            Test(sortCases, i => (ushort)i, ushort.MaxValue);
        static void Test_Keys_Int32(ISortCases sortCases) => 
            Test(sortCases, i => (int)i, int.MinValue);
        static void Test_Keys_UInt32(ISortCases sortCases) => 
            Test(sortCases, i => (uint)i, uint.MaxValue);
        static void Test_Keys_Int64(ISortCases sortCases) => 
            Test(sortCases, i => (long)i, long.MinValue);
        static void Test_Keys_UInt64(ISortCases sortCases) => 
            Test(sortCases, i => (ulong)i, ulong.MaxValue);
        static void Test_Keys_Single(ISortCases sortCases) => 
            Test(sortCases, i => (float)i, float.NaN);
        static void Test_Keys_Double(ISortCases sortCases) => 
            Test(sortCases, i => (double)i, double.NaN);
        static void Test_Keys_Boolean(ISortCases sortCases) => 
            Test(sortCases, i => i % 2 == 0, false);
        static void Test_Keys_Char(ISortCases sortCases) => 
            Test(sortCases, i => (char)i, char.MaxValue);
        static void Test_Keys_String(ISortCases sortCases) => 
            Test(sortCases, i => i.ToString("D9"), null);
        static void Test_Keys_ComparableStructInt32(ISortCases sortCases) =>
            Test(sortCases, i => new ComparableStructInt32(i), new ComparableStructInt32(int.MinValue));
        static void Test_Keys_ComparableClassInt32(ISortCases sortCases) =>
            Test(sortCases, i => new ComparableClassInt32(i), null);
        static void Test_Keys_BogusComparable(ISortCases sortCases) =>
            Test(sortCases, i => new BogusComparable(i), null);

        static void Test<TKey>(ISortCases sortCase, Func<int, TKey> toKey, TKey specialKey)
            where TKey : IComparable<TKey>
        {
            foreach (var unsorted in sortCase.EnumerateTests(toKey, specialKey))
            {
                TestSortOverloads(unsorted);
            }
        }
        static void TestSortOverloads<TKey>(ArraySegment<TKey> keys)
            where TKey : IComparable<TKey>
        {
            var copy = (TKey[])keys.Array.Clone();

            TestSort(keys);
            TestSort(keys, Comparer<TKey>.Default);
            TestSort(keys, Comparer<TKey>.Default.Compare);
            TestSort(keys, new CustomComparer<TKey>());
            TestSort(keys, (IComparer<TKey>)null);
            // TODO: Should results for a bogus comparer be identical? They are not currently
            //TestSort(keys, new BogusComparer<TKey>());
        }
        static void TestSort<TKey>(
            ArraySegment<TKey> keysToSort)
            where TKey : IComparable<TKey>
        {
            var expected = new ArraySegment<TKey>((TKey[])keysToSort.Array.Clone(),
                keysToSort.Offset, keysToSort.Count);
            Array.Sort(expected.Array, expected.Offset, expected.Count);

            Span<TKey> keysSpan = keysToSort;
            keysSpan.Sort();

            // We assert the full arrays are as expected, to check for possible under/overflow
            Assert.Equal(expected.Array, keysToSort.Array);
        }
        static void TestSort<TKey, TComparer>(
            ArraySegment<TKey> keysToSort,
            TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            var expected = new ArraySegment<TKey>((TKey[])keysToSort.Array.Clone(),
                keysToSort.Offset, keysToSort.Count);
            Array.Sort(expected.Array, expected.Offset, expected.Count, comparer);

            Span<TKey> keysSpan = keysToSort;
            keysSpan.Sort(comparer);

            // We assert the full arrays are as expected, to check for possible under/overflow
            Assert.Equal(expected.Array, keysToSort.Array);
        }
        static void TestSort<TKey>(
            ArraySegment<TKey> keysToSort,
            Comparison<TKey> comparison)
        {
            var expected = new ArraySegment<TKey>((TKey[])keysToSort.Array.Clone(),
                keysToSort.Offset, keysToSort.Count);
            // Array.Sort doesn't have a comparison version for segments
            if (expected.Offset == 0 && expected.Count == expected.Array.Length)
            {
                Array.Sort(expected.Array, comparison);
            }
            else
            {
                Array.Sort(expected.Array, expected.Offset, expected.Count, 
                    new ComparisonComparer<TKey>(comparison));
            }

            Span<TKey> keysSpan = keysToSort;
            keysSpan.Sort(comparison);

            // We assert the full arrays are as expected, to check for possible under/overflow
            Assert.Equal(expected.Array, keysToSort.Array);
        }

        static void Test_KeysValues_Int8_Int32(ISortCases sortCases) =>
            Test(sortCases, i => (sbyte)i, sbyte.MinValue, i => i);
        static void Test_KeysValues_UInt8_Int32(ISortCases sortCases) =>
            Test(sortCases, i => (byte)i, byte.MaxValue, i => i);
        static void Test_KeysValues_Int16_Int32(ISortCases sortCases) =>
            Test(sortCases, i => (short)i, short.MinValue, i => i);
        static void Test_KeysValues_UInt16_Int32(ISortCases sortCases) =>
            Test(sortCases, i => (ushort)i, ushort.MaxValue, i => i);
        static void Test_KeysValues_Int32_Int32(ISortCases sortCases) =>
            Test(sortCases, i => (int)i, int.MinValue, i => i);
        static void Test_KeysValues_UInt32_Int32(ISortCases sortCases) =>
            Test(sortCases, i => (uint)i, uint.MaxValue, i => i);
        static void Test_KeysValues_Int64_Int32(ISortCases sortCases) =>
            Test(sortCases, i => (long)i, long.MinValue, i => i);
        static void Test_KeysValues_UInt64_Int32(ISortCases sortCases) =>
            Test(sortCases, i => (ulong)i, ulong.MaxValue, i => i);
        static void Test_KeysValues_Single_Int32(ISortCases sortCases) =>
            Test(sortCases, i => (float)i, float.NaN, i => i);
        static void Test_KeysValues_Double_Int32(ISortCases sortCases) =>
            Test(sortCases, i => (double)i, double.NaN, i => i);
        static void Test_KeysValues_Boolean_Int32(ISortCases sortCases) =>
            Test(sortCases, i => i % 2 == 0, false, i => i);
        static void Test_KeysValues_Char_Int32(ISortCases sortCases) =>
            Test(sortCases, i => (char)i, char.MaxValue, i => i);
        static void Test_KeysValues_String_Int32(ISortCases sortCases) =>
            Test(sortCases, i => i.ToString("D9"), null, i => i);
        static void Test_KeysValues_ComparableStructInt32_Int32(ISortCases sortCases) =>
            Test(sortCases, i => new ComparableStructInt32(i), new ComparableStructInt32(int.MinValue), i => i);
        static void Test_KeysValues_ComparableClassInt32_Int32(ISortCases sortCases) =>
            Test(sortCases, i => new ComparableClassInt32(i), null, i => i);
        static void Test_KeysValues_BogusComparable_Int32(ISortCases sortCases) =>
            Test(sortCases, i => new BogusComparable(i), null, i => i);

        static void Test<TKey, TValue>(ISortCases sortCase, 
            Func<int, TKey> toKey, TKey specialKey, Func<int, TValue> toValue)
            where TKey : IComparable<TKey>
        {
            foreach (var unsortedKeys in sortCase.EnumerateTests(toKey, specialKey))
            {
                var length = unsortedKeys.Array.Length;
                var values = new TValue[length];
                // Items are always based on "unique" int values
                new IncrementingSpanFiller().Fill(values, length, toValue);
                var unsortedValues = new ArraySegment<TValue>(values, unsortedKeys.Offset, unsortedKeys.Count);
                TestSortOverloads(unsortedKeys, unsortedValues);
            }
        }
        static void TestSortOverloads<TKey, TValue>(ArraySegment<TKey> keys, ArraySegment<TValue> values)
            where TKey : IComparable<TKey>
        {
            var copy = (TKey[])keys.Array.Clone();

            TestSort(keys, values);
            TestSort(keys, values, Comparer<TKey>.Default);
            TestSort(keys, values, Comparer<TKey>.Default.Compare);
            TestSort(keys, values, new CustomComparer<TKey>());
            TestSort(keys, values, (IComparer<TKey>)null);
            // TODO: Should results for a bogus comparer be identical? They are not currently
            //TestSort(keys, values, new BogusComparer<TKey>());
        }
        static void TestSort<TKey, TValue>(
            ArraySegment<TKey> keysToSort, ArraySegment<TValue> valuesToSort)
            where TKey : IComparable<TKey>
        {
            var expectedKeys = new ArraySegment<TKey>((TKey[])keysToSort.Array.Clone(),
                keysToSort.Offset, keysToSort.Count);
            var expectedValues = new ArraySegment<TValue>((TValue[])valuesToSort.Array.Clone(),
                valuesToSort.Offset, valuesToSort.Count);
            Assert.Equal(expectedKeys.Offset, expectedValues.Offset);
            Assert.Equal(expectedKeys.Count, expectedValues.Count);
            Array.Sort(expectedKeys.Array, expectedValues.Array, expectedKeys.Offset, expectedKeys.Count);

            Span<TKey> keysSpan = keysToSort;
            Span<TValue> valuesSpan = valuesToSort;
            keysSpan.Sort(valuesSpan);

            // We assert the full arrays are as expected, to check for possible under/overflow
            Assert.Equal(expectedKeys.Array, keysToSort.Array);
            Assert.Equal(expectedValues.Array, valuesToSort.Array);
        }
        static void TestSort<TKey, TValue, TComparer>(
            ArraySegment<TKey> keysToSort, ArraySegment<TValue> valuesToSort,
            TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            var expectedKeys = new ArraySegment<TKey>((TKey[])keysToSort.Array.Clone(),
                keysToSort.Offset, keysToSort.Count);
            var expectedValues = new ArraySegment<TValue>((TValue[])valuesToSort.Array.Clone(),
                valuesToSort.Offset, valuesToSort.Count);
            Array.Sort(expectedKeys.Array, expectedValues.Array, expectedKeys.Offset, expectedKeys.Count, comparer);

            Span<TKey> keysSpan = keysToSort;
            Span<TValue> valuesSpan = valuesToSort;
            keysSpan.Sort(valuesSpan, comparer);

            // We assert the full arrays are as expected, to check for possible under/overflow
            Assert.Equal(expectedKeys.Array, keysToSort.Array);
        }
        static void TestSort<TKey, TValue>(
            ArraySegment<TKey> keysToSort, ArraySegment<TValue> valuesToSort,
            Comparison<TKey> comparison)
        {
            var expectedKeys = new ArraySegment<TKey>((TKey[])keysToSort.Array.Clone(),
                keysToSort.Offset, keysToSort.Count);
            var expectedValues = new ArraySegment<TValue>((TValue[])valuesToSort.Array.Clone(),
                valuesToSort.Offset, valuesToSort.Count);
            // Array.Sort doesn't have a comparison version for segments
            Array.Sort(expectedKeys.Array, expectedValues.Array, expectedKeys.Offset, expectedKeys.Count,
                new ComparisonComparer<TKey>(comparison));

            Span<TKey> keysSpan = keysToSort;
            Span<TValue> valuesSpan = valuesToSort;
            keysSpan.Sort(valuesSpan, comparison);

            // We assert the full arrays are as expected, to check for possible under/overflow
            Assert.Equal(expectedKeys.Array, keysToSort.Array);
        }

        public interface ISortCases
        {
            IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey, TKey specialKey);
        }
        public class FillerSortCases : ISortCases
        {
            public FillerSortCases(int maxLength, ISpanFiller filler)
            {
                MaxLength = maxLength;
                Filler = filler ?? throw new ArgumentNullException(nameof(filler));
            }

            public int MinLength => 2;
            public int MaxLength { get; }
            public ISpanFiller Filler { get; }

            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey, TKey specialKey)
            {
                for (int length = MinLength; length <= MaxLength; length++)
                {
                    var unsorted = new TKey[length];
                    Filler.Fill(unsorted, length, toKey);
                    yield return unsorted;
                }
            }

            public override string ToString()
            {
                return $"Lengths [{MinLength}, {MaxLength,4}] {nameof(Filler)}={Filler.GetType().Name.Replace("SpanFiller", "")} ";
            }
        }
        public class LengthZeroSortCases : ISortCases
        {
            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey, TKey specialKey)
            {
                yield return Array.Empty<TKey>();
            }

            public override string ToString()
                => GetType().Name.Replace(nameof(ISortCases).Remove(0, 1), string.Empty);
        }
        public class LengthOneSortCases : ISortCases
        {
            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey, TKey specialKey)
            {
                yield return new[] { toKey(-1) };
            }

            public override string ToString()
                => GetType().Name.Replace(nameof(ISortCases).Remove(0, 1), string.Empty);
        }
        public class AllLengthTwoSortCases : ISortCases
        {
            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey, TKey specialKey)
            {
                const int length = 2;
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        yield return new[] { toKey(i), toKey(j) };
                    }
                }
            }

            public override string ToString()
                => GetType().Name.Replace(nameof(ISortCases).Remove(0, 1), string.Empty);
        }
        public class AllLengthThreeSortCases : ISortCases
        {
            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey, TKey specialKey)
            {
                const int length = 3;
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        for (int k = 0; k < length; k++)
                        {
                            yield return new[] { toKey(i), toKey(j), toKey(k) };
                        }
                    }
                }
            }

            public override string ToString()
                => GetType().Name.Replace(nameof(ISortCases).Remove(0, 1), string.Empty);
        }
        public class AllLengthFourSortCases : ISortCases
        {
            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey, TKey specialKey)
            {
                const int length = 4;
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        for (int k = 0; k < length; k++)
                        {
                            for (int l = 0; l < length; l++)
                            {
                                yield return new[] { toKey(i), toKey(j), toKey(k), toKey(l) };
                            }
                        }
                    }
                }
            }

            public override string ToString()
                => GetType().Name.Replace(nameof(ISortCases).Remove(0, 1), string.Empty);
        }
        public class PadAndSliceSortCases : ISortCases
        {
            readonly ISortCases _sortCases;
            readonly int _slicePadding;

            public PadAndSliceSortCases(ISortCases sortCases, int slicePadding)
            {
                _sortCases = sortCases ?? throw new ArgumentNullException(nameof(sortCases));
                _slicePadding = slicePadding;
            }

            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey, TKey specialKey)
            {
                return _sortCases.EnumerateTests(toKey, specialKey).Select(ks =>
                {
                    var newKeys = new TKey[ks.Count + 2 * _slicePadding];
                    Array.Copy(ks.Array, ks.Offset, newKeys, _slicePadding, ks.Count);
                    var padKey = toKey(unchecked((int)0xCECECECE));
                    for (int i = 0; i < _slicePadding; i++)
                    {
                        newKeys[i] = padKey;
                        newKeys[newKeys.Length - i - 1] = padKey;
                    }
                    return new ArraySegment<TKey>(newKeys, _slicePadding, ks.Count);
                });
            }

            public override string ToString()
                => GetType().Name.Replace(nameof(ISortCases).Remove(0, 1), string.Empty) + 
                $":{_slicePadding} " + _sortCases.ToString();
        }
        public class StepwiseSpecialSortCases : ISortCases
        {
            readonly ISortCases _sortCases;
            readonly int _step;

            public StepwiseSpecialSortCases(ISortCases sortCases, int step)
            {
                _sortCases = sortCases ?? throw new ArgumentNullException(nameof(sortCases));
                _step = step;
            }

            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey, TKey specialKey)
            {
                return _sortCases.EnumerateTests(toKey, specialKey).Select(ks =>
                {
                    for (int i = 0; i < ks.Count; i += _step)
                    {
                        ks[i] = specialKey;
                    }
                    return ks;
                });
            }

            public override string ToString()
                => GetType().Name.Replace(nameof(ISortCases).Remove(0, 1), string.Empty) +
                $":{_step} " + _sortCases.ToString();
        }


        internal struct CustomComparer<TKey> : IComparer<TKey>
            where TKey : IComparable<TKey>
        {
            public int Compare(TKey x, TKey y) => object.ReferenceEquals(x, y) ? 0 : (x != null ? x.CompareTo(y) : -1);
        }

        internal struct BogusComparer<TKey> : IComparer<TKey>
            where TKey : IComparable<TKey>
        {
            public int Compare(TKey x, TKey y) => 1; // Always greater
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
                return other != null ? Value.CompareTo(other.Value) : 1;
            }
        }

        public class BogusComparable: IComparable<BogusComparable>
        {
            public readonly int Value;

            public BogusComparable(int value)
            {
                Value = value;
            }

            public int CompareTo(BogusComparable other) => 1;
        }

        public struct ValueIdStruct : IComparable<ValueIdStruct>, IEquatable<ValueIdStruct>
        {
            public ValueIdStruct(int value, int identity)
            {
                Value = value;
                Id = identity;
            }

            public int Value { get; }
            public int Id { get; }

            // Sort by value
            public int CompareTo(ValueIdStruct other) =>
                Value.CompareTo(other.Value);

            // Check equality by both
            public bool Equals(ValueIdStruct other) =>
                Value.Equals(other.Value) && Id.Equals(other.Id);

            public override bool Equals(object obj)
            {
                if (obj is ValueIdStruct)
                {
                    return Equals((ValueIdStruct)obj);
                }
                return false;
            }

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => $"{Value} Id:{Id}";
        }

        public class ValueIdClass : IComparable<ValueIdClass>, IEquatable<ValueIdClass>
        {
            public ValueIdClass(int value, int identity)
            {
                Value = value;
                Id = identity;
            }

            public int Value { get; }
            public int Id { get; }

            // Sort by value
            public int CompareTo(ValueIdClass other) =>
                Value.CompareTo(other.Value);

            // Check equality by both
            public bool Equals(ValueIdClass other) =>
                other != null && Value.Equals(other.Value) && Id.Equals(other.Id);

            public override bool Equals(object obj)
            {
                return Equals(obj as ValueIdClass);
            }

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => $"{Value} Id:{Id}";
        }

        // Used for array sort
        class ComparisonComparer<TKey> : IComparer<TKey>
        {
            readonly Comparison<TKey> _comparison;

            public ComparisonComparer(Comparison<TKey> comparison)
            {
                _comparison = comparison;
            }

            public int Compare(TKey x, TKey y) => _comparison(x, y);
        }
    }
}
