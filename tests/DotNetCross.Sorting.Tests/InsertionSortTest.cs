using System;
using System.Collections.Generic;
using Xunit;

namespace DotNetCross.Sorting.Tests
{
    public class InsertionSortTest
    {
        [Fact]
        public void Sort()
        {
            var a = new int[] { 6, 4, 5, 1, 2, 1, 3, 4, 1 };
            InsertionSort.Sort(new Span<int>(a), Comparer<int>.Default);
            var b = new int[a.Length];
            Array.Copy(a, b, a.Length);
            Array.Sort(b);
            Assert.Equal(b, a);
        }
    }
}
