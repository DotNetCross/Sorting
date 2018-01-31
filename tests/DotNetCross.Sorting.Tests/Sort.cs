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
            var allCases = cases.Concat(cases.Select(c => new PadAndSliceSortCases(c, 2)));
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

        #region Keys Tests

        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Int16(ISortCases sortCases)
        {
            Test(sortCases, i => (short)i);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_UInt16(ISortCases sortCases)
        {
            Test(sortCases, i => (ushort)i);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => i);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_UInt32(ISortCases sortCases)
        {
            Test(sortCases, i => (uint)i);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Int64(ISortCases sortCases)
        {
            Test(sortCases, i => (long)i);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_UInt64(ISortCases sortCases)
        {
            Test(sortCases, i => (ulong)i);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Single(ISortCases sortCases)
        {
            Test(sortCases, i => (float)i);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Double(ISortCases sortCases)
        {
            Test(sortCases, i => (double)i);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Boolean(ISortCases sortCases)
        {
            Test(sortCases, i => i % 2 == 0);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_Char(ISortCases sortCases)
        {
            Test(sortCases, i => (char)i);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_String(ISortCases sortCases)
        {
            Test(sortCases, i => i.ToString("D9"));
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_ComparableStructInt32(ISortCases sortCases)
        {
            Test(sortCases, i => new ComparableStructInt32(i));
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_Keys_ComparableClassInt32(ISortCases sortCases)
        {
            Test(sortCases, i => new ComparableClassInt32(i));
        }
#if OUTER_LOOP
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Int16_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (short)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_UInt16_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (ushort)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => i);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_UInt32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (uint)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Int64_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (long)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_UInt64_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (ulong)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Single_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (float)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Double_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (double)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Boolean_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => i % 2 == 0);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_Char_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (char)i);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_String_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => i.ToString("D9"));
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_ComparableStructInt32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => new ComparableStructInt32(i));
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_Keys_ComparableClassInt32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => new ComparableClassInt32(i));
        }
#endif

        #endregion

        #region Keys and Values Tests

        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Int16_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => (short)i, v => v);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_UInt16_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => (ushort)i, v => v);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Int32_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => i, v => v);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_UInt32_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => (uint)i, v => v);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Int64_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => (long)i, v => v);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_UInt64_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => (ulong)i, v => v);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Single_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => (float)i, v => v);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Double_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => (double)i, v => v);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Boolean_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => i % 2 == 0, v => v);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_Char_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => (char)i, v => v);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_String_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => i.ToString("D9"), v => v);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_ComparableStructInt32_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => new ComparableStructInt32(i), v => v);
        }
        
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_fastSortTests))]
        public static void Sort_KeysValues_ComparableClassInt32_Int32(ISortCases sortCases)
        {
            Test(sortCases, i => new ComparableClassInt32(i), v => v);
        }
#if OUTER_LOOP
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Int16_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (short)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_UInt16_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (ushort)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Int32_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_UInt32_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (uint)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Int64_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (long)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_UInt64_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (ulong)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Single_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (float)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Double_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (double)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Boolean_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => i % 2 == 0, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_Char_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => (char)i, v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_String_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => i.ToString("D9"), v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_ComparableStructInt32_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => new ComparableStructInt32(i), v => v);
        }
        //[OuterLoop]
        [Theory]
        [Trait(SortTrait, SortTraitValue)]
        [MemberData(nameof(s_slowSortTests))]
        public static void Sort_KeysValues_ComparableClassInt32_Int32_OuterLoop(ISortCases sortCases)
        {
            Test(sortCases, i => new ComparableClassInt32(i), v => v);
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


        private static void Test<TKey>(ISortCases sortCase, Func<int, TKey> toKey)
            where TKey : IComparable<TKey>
        {
            foreach (var unsorted in sortCase.EnumerateTests(toKey))
            {
                TestSortOverloads(unsorted);
            }
        }
        private static void TestSortOverloads<TKey>(ArraySegment<TKey> keys)
            where TKey : IComparable<TKey>
        {
            TestSort(keys, s => ((Span<TKey>)s).Sort());
            TestSort(keys, s => ((Span<TKey>)s).Sort(Comparer<TKey>.Default));
            TestSort(keys, s => ((Span<TKey>)s).Sort(Comparer<TKey>.Default.Compare));
            TestSort(keys, s => ((Span<TKey>)s).Sort(new CustomComparer<TKey>()));
            TestSort(keys, s => ((Span<TKey>)s).Sort((IComparer<TKey>)null));
        }
        private static void TestSort<TKey>(
            ArraySegment<TKey> keysToSort, 
            Action<ArraySegment<TKey>> sort)
            where TKey : IComparable<TKey>
        {
            var expected = new ArraySegment<TKey>((TKey[])keysToSort.Array.Clone(),
                keysToSort.Offset, keysToSort.Count);
            Array.Sort(expected.Array, expected.Offset, expected.Count);

            sort(keysToSort);

            // We assert the full arrays are as expected, to check for possible under/overflow
            Assert.Equal(expected.Array, keysToSort.Array);
        }

        private static void Test<TKey, TValue>(ISortCases sortCase, Func<int, TKey> toKey, Func<int, TValue> toValue)
            where TKey : IComparable<TKey>
        {
            foreach (var unsortedKeys in sortCase.EnumerateTests(toKey))
            {
                var length = unsortedKeys.Array.Length;
                var values = new TValue[length];
                // Items are always based on "unique" int values
                new IncrementingSpanFiller().Fill(values, length, toValue);
                var unsortedValues = new ArraySegment<TValue>(values, unsortedKeys.Offset, unsortedKeys.Count);
                TestSortOverloads(unsortedKeys, unsortedValues);
            }
        }
        private static void TestSortOverloads<TKey, TValue>(ArraySegment<TKey> keys, ArraySegment<TValue> values)
            where TKey : IComparable<TKey>
        {
            TestSort(keys, values, (ks, vs) => ((Span<TKey>)ks).Sort((Span<TValue>)vs));
            TestSort(keys, values, (ks, vs) => ((Span<TKey>)ks).Sort((Span<TValue>)vs, Comparer<TKey>.Default));
            TestSort(keys, values, (ks, vs) => ((Span<TKey>)ks).Sort((Span<TValue>)vs, Comparer<TKey>.Default.Compare));
            TestSort(keys, values, (ks, vs) => ((Span<TKey>)ks).Sort((Span<TValue>)vs, new CustomComparer<TKey>()));
            TestSort(keys, values, (ks, vs) => ((Span<TKey>)ks).Sort((Span<TValue>)vs, (IComparer<TKey>)null));
        }
        private static void TestSort<TKey, TValue>(
            ArraySegment<TKey> keysToSort, ArraySegment<TValue> valuesToSort, 
            Action<ArraySegment<TKey>, ArraySegment<TValue>> sort)
            where TKey : IComparable<TKey>
        {
            var expectedKeys = new ArraySegment<TKey>((TKey[])keysToSort.Array.Clone(),
                keysToSort.Offset, keysToSort.Count);
            var expectedValues = new ArraySegment<TValue>((TValue[])valuesToSort.Array.Clone(),
                valuesToSort.Offset, valuesToSort.Count);
            Assert.Equal(expectedKeys.Offset, expectedValues.Offset);
            Assert.Equal(expectedKeys.Count, expectedValues.Count);
            Array.Sort(expectedKeys.Array, expectedValues.Array, expectedKeys.Offset, expectedKeys.Count);

            sort(keysToSort, valuesToSort);

            // We assert the full arrays are as expected, to check for possible under/overflow
            Assert.Equal(expectedKeys.Array, keysToSort.Array);
            Assert.Equal(expectedValues.Array, valuesToSort.Array);
        }

        public interface ISortCases
        {
            IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey);
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

            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey)
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
        public class AllLengthTwoSortCases : ISortCases
        {
            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey)
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
            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey)
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
            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey)
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

            public IEnumerable<ArraySegment<TKey>> EnumerateTests<TKey>(Func<int, TKey> toKey)
            {
                return _sortCases.EnumerateTests(toKey).Select(ks =>
                {
                    var newKeys = new TKey[ks.Count + 2 * _slicePadding];
                    Array.Copy(ks.Array, ks.Offset, newKeys, _slicePadding, ks.Count);
                    var padKey = toKey(unchecked((int)0xCDCDCDCD));
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


        internal struct CustomComparer<TKey> : IComparer<TKey>
            where TKey : IComparable<TKey>
        {
            public int Compare(TKey x, TKey y) => x.CompareTo(y);
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
    }
}
