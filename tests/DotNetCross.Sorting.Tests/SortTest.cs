using System;
using System.Collections.Generic;
using Xunit;

namespace DotNetCross.Sorting.Tests
{
    public class SortTest
    {
        [Fact]
        public void Insertion()
        {
            Test((a, c) => InsertionSort.Sort(new Span<int>(a), c));
        }

        [Fact]
        public void Quick_Lomuto()
        {
            Test((a, c) => QuickSort.Sort(new Span<int>(a), new LomutoPartitioner(), c));
        }

        [Fact]
        public void Quick_Hoare()
        {
            Test((a, c) => QuickSort.Sort(new Span<int>(a), new HoarePartitioner(), c));
        }

        [Fact]
        public void SpanSortExtension()
        {
            Test((a, c) => new Span<int>(a).Sort(c));
        }


        static readonly (int[] Unsorted, int[] Sorted)[] Cases = {
            Create(new int[] { 6, 4, 5, 2, 1 }),
            Create(new int[] { 6, 4, 5, 1, 2, 1, 3, 4, 1 }),
            Create(new int[] { }),
            Create(new int[] { 0 }),
            Create(new int[] { 0, 0, 0 }),
            Create(new int[] { 0, 0, 0, 1, 1, 1, 2, 2, 2 }),
            Create(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
            Create(new int[] { 10, 7, 8, 9, 1, 5 }),
        };

        static (int[] Unsorted, int[] Sorted) Create(int[] unsorted)
        {
            var sorted = new int[unsorted.Length];
            Array.Copy(unsorted, sorted, unsorted.Length);
            Array.Sort(sorted);
            return (unsorted, sorted);
        }

        static void Test(Action<int[], Comparer<int>> sort)
        {
            foreach (var c in Cases)
            {
                var unsorted = c.Unsorted;
                var expected = c.Sorted;
                var actual = new int[unsorted.Length];
                Array.Copy(unsorted, actual, unsorted.Length);

                sort(actual, Comparer<int>.Default);

                Assert.Equal(expected, actual);
            }
        }
    }
}
