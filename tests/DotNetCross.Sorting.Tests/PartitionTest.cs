using System;
using System.Collections.Generic;
using Xunit;

namespace DotNetCross.Sorting.Tests
{
    public class PartitionTest
    {
        [Fact]
        public void Hoare()
        {
            var p = new HoarePartitioner();
            Test(HoareCases, (a, c) => p.Partition(ref a[0], 0, a.Length - 1, c));
        }

        // NOTE: How Hoares partition scheme does NOT guarantee that returned index is the pivot index!
        static readonly (int[] unpartioned, int[] partitioned, int index)[] HoareCases = {
            ( new int[] { 6, 4, 5, 2, 1 }, new int[] { 1, 4, 5, 2, 6 }, 3 ),
            ( new int[] { 1, 4, 5, 2 }, new int[] { 1, 4, 5, 2 }, 0 ),
            ( new int[] { 4, 5, 2 }, new int[] { 2, 5, 4 }, 0 ), 
            ( new int[] { 5, 4 }, new int[] { 4, 5 }, 0 ),
        };

        public static void Test((int[] unpartioned, int[] partitioned, int index)[] cases, Func<int[], Comparer<int>, int> partition)
        {
            foreach (var c in cases)
            {
                var unpartitioned = c.unpartioned;
                var expected = c.partitioned;
                var actual = new int[unpartitioned.Length];
                Array.Copy(unpartitioned, actual, unpartitioned.Length);

                var actualIndex = partition(actual, Comparer<int>.Default);

                Assert.Equal(c.index, actualIndex);
                Assert.Equal(expected, actual);
            }
        }
    }
}
