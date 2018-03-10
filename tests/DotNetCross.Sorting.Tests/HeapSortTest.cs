using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace DotNetCross.Sorting.Tests
{
    public class HeapSortTest
    {
        // 153  169
        //const int lo = 153;
        //const int hi = 169;

        byte[] Keys() => new byte[] { 81, 83, 82, 81, 80, 79, 78, 77, 76, 82, 83, 84, 76, 77, 78, 79, 80 };
        int[] Values() => new int[] { 412, 154, 155, 156, 157, 158, 159, 160, 161, 411, 410, 409, 417, 416, 415, 414, 413 };

        byte[] ExpectedKeys() => new byte[] { 76, 76, 77, 77, 78, 78, 79, 79, 80, 80, 81, 81, 82, 82, 83, 83, 84 };
        int[] ExpectedValues() => new int[] { 161, 417, 416, 160, 415, 159, 158, 414, 157, 413, 412, 156, 155, 411, 154, 410, 409 };

        // "161, 417, 416, 160, 415, 159, 414, 158, 413, 157, 156, 412, 411, 155, 410, 154, 409"

        // "161, 417, 416, 160, 415, 159, 414, 158, 413, 157, 156, 412, 411, 155, 410, 154, 409"

        // 191 207 (offset 2, so 193 209)
        // k 0 v 512
        //byte[] Keys02() => new byte[] { 102, 103, 95, 101, 100, 99, 98, 97, 96, 97, 96, 98, 103, 102, 101, 100, 99 };
        //int[] Values02() => new int[] { 410, 409, 161, 411, 412, 413, 414, 415, 160, 159, 416, 158, 153, 154, 155, 156, 157 };

        //byte[] ExpectedKeys02() => new byte[] { 95, 96, 96, 97, 97, 98, 98, 99, 99, 100, 100, 101, 101, 102, 102, 103, 103 };
        //int[] ExpectedValues02() => new int[] { 161, 160, 416, 159, 415, 158, 414, 157, 413, 412, 156, 155, 411, 154, 410, 153, 409 };

        // 116 133 (offset 2, so 118 135)
        byte[] Keys02() => new byte[] { 62, 61, 60, 59, 58, 66, 64, 64, 63, 62, 61, 60, 59, 65, 58, 66, 63, 65 };
        int[] Values02() => new int[] { 194, 195, 196, 197, 198, 446, 192, 448, 449, 450, 451, 452, 453, 191, 454, 190, 193, 447 };

        byte[] ExpectedKeys02() => new byte[] { 58, 58, 59, 59, 60, 60, 61, 61, 62, 62, 63, 63, 64, 64, 65, 65, 66, 66 };
        int[] ExpectedValues02() => new int[] { 454, 198, 197, 453, 452, 196, 195, 451, 450, 194, 449, 193, 192, 448, 191, 447, 446, 190 };

        [Fact]
        public void ArrayTest()
        {
            var keys = Keys();
            var values = Values();
            Heapsort(keys, values, 0, keys.Length - 1);
            Assert.Equal(ExpectedKeys(), keys);
            Assert.Equal(ExpectedValues(), values);
        }

        [Fact]
        public void SpanTest()
        {
            var keys = Keys();
            var values = Values();
            HeapSort(ref keys[0], ref values[0], 0, keys.Length - 1);
            Assert.Equal(ExpectedKeys(), keys);
            Assert.Equal(ExpectedValues(), values);
        }

        [Fact]
        public void ArrayTest02()
        {
            var keys = Keys02();
            var values = Values02();
            Heapsort(keys, values, 0, keys.Length - 1);
            Assert.Equal(ExpectedKeys02(), keys);
            Assert.Equal(ExpectedValues02(), values);
        }

        [Fact]
        public void SpanTest02()
        {
            var keys = Keys02();
            var values = Values02();
            HeapSort(ref keys[0], ref values[0], 0, keys.Length - 1);
            Assert.Equal(ExpectedKeys02(), keys);
            Assert.Equal(ExpectedValues02(), values);
        }

        private static void Heapsort<TKey, TValue>(TKey[] keys, TValue[] values, int lo, int hi)
            where TKey : IComparable<TKey>
        {
            //Contract.Requires(keys != null);
            //Contract.Requires(values != null);
            //Contract.Requires(lo >= 0);
            //Contract.Requires(hi > lo);
            //Contract.Requires(hi < keys.Length);

            int n = hi - lo + 1;
            for (int i = n / 2; i >= 1; i = i - 1)
            {
                DownHeap(keys, values, i, n, lo);
            }
            for (int i = n; i > 1; i = i - 1)
            {
                Swap(keys, values, lo, lo + i - 1);
                DownHeap(keys, values, 1, i - 1, lo);
            }
        }

        private static void DownHeap<TKey, TValue>(TKey[] keys, TValue[] values, int i, int n, int lo)
            where TKey : IComparable<TKey>
        {
            //Contract.Requires(keys != null);
            //Contract.Requires(lo >= 0);
            //Contract.Requires(lo < keys.Length);

            TKey d = keys[lo + i - 1];
            TValue dValue = (values != null) ? values[lo + i - 1] : default(TValue);
            int child;
            while (i <= n / 2)
            {
                child = 2 * i;
                if (child < n && (keys[lo + child - 1] == null || keys[lo + child - 1].CompareTo(keys[lo + child]) < 0))
                {
                    child++;
                }
                if (keys[lo + child - 1] == null || keys[lo + child - 1].CompareTo(d) < 0)
                    break;
                keys[lo + i - 1] = keys[lo + child - 1];
                if (values != null)
                    values[lo + i - 1] = values[lo + child - 1];
                i = child;
            }
            keys[lo + i - 1] = d;
            if (values != null)
                values[lo + i - 1] = dValue;
        }

        private static void Swap<TKey, TValue>(TKey[] keys, TValue[] values, int i, int j)
        {
            if (i != j)
            {
                TKey k = keys[i];
                keys[i] = keys[j];
                keys[j] = k;
                if (values != null)
                {
                    TValue v = values[i];
                    values[i] = values[j];
                    values[j] = v;
                }
            }
        }

        private static void HeapSort<TKey, TValue>(
            ref TKey keys, ref TValue values, int lo, int hi
            )
            where TKey : IComparable<TKey>
        {

            Debug.Assert(lo >= 0);
            Debug.Assert(hi > lo);

            int n = hi - lo + 1;
            for (int i = n / 2; i >= 1; --i)
            {
                DownHeap(ref keys, ref values, i, n, lo);
            }
            for (int i = n; i > 1; --i)
            {
                Swap(ref keys, lo, lo + i - 1);
                Swap(ref values, lo, lo + i - 1);
                DownHeap(ref keys, ref values, 1, i - 1, lo);
            }
        }

        private static void DownHeap<TKey, TValue>(
    ref TKey keys, ref TValue values, int i, int n, int lo)
    where TKey : IComparable<TKey>
        {

            Debug.Assert(lo >= 0);

            //TKey d = keys[lo + i - 1];
            ref TKey keysAtLo = ref Unsafe.Add(ref keys, lo);
            ref TKey keysAtLoMinus1 = ref Unsafe.Add(ref keysAtLo, -1); // No Subtract??

            ref TValue valuesAtLoMinus1 = ref Unsafe.Add(ref values, lo - 1);

            TKey d = Unsafe.Add(ref keysAtLoMinus1, i);
            TValue dValue = Unsafe.Add(ref valuesAtLoMinus1, i);

            var nHalf = n / 2;
            while (i <= nHalf)
            {
                int child = i << 1;

                //if (child < n && (keys[lo + child - 1] == null || keys[lo + child - 1].CompareTo(keys[lo + child]) < 0))
                if (child < n &&
                    (Unsafe.Add(ref keysAtLoMinus1, child) == null ||
                     Unsafe.Add(ref keysAtLoMinus1, child).CompareTo(Unsafe.Add(ref keysAtLo, child)) < 0))
                {
                    ++child;
                }

                //if (keys[lo + child - 1] == null || keys[lo + child - 1].CompareTo(d) < 0)
                if (Unsafe.Add(ref keysAtLoMinus1, child) == null ||
                    Unsafe.Add(ref keysAtLoMinus1, child).CompareTo(d) < 0)
                    break;

                // keys[lo + i - 1] = keys[lo + child - 1]
                Unsafe.Add(ref keysAtLoMinus1, i) = Unsafe.Add(ref keysAtLoMinus1, child);
                Unsafe.Add(ref valuesAtLoMinus1, i) = Unsafe.Add(ref valuesAtLoMinus1, child);

                i = child;
            }
            //keys[lo + i - 1] = d;
            Unsafe.Add(ref keysAtLoMinus1, i) = d;
            Unsafe.Add(ref valuesAtLoMinus1, i) = dValue;
        }

        //private static void DownHeap<TKey, TValue>(
        //    ref TKey keys, ref TValue values, int i, int n, int lo)
        //    where TKey : IComparable<TKey>
        //{

        //    Debug.Assert(lo >= 0);

        //    //TKey d = keys[lo + i - 1];
        //    //ref TKey keysAtLo = ref Unsafe.Add(ref keys, lo);
        //    //ref TKey keysAtLoMinus1 = ref Unsafe.Add(ref keysAtLo, -1); // No Subtract??

        //    //ref TValue valuesAtLoMinus1 = ref Unsafe.Add(ref values, lo - 1);

        //    TKey d = Unsafe.Add(ref keys, lo + i - 1);
        //    TValue dValue = Unsafe.Add(ref values, lo + i - 1);

        //    while (i <= n / 2)
        //    {
        //        int child = 2 * i;

        //        //if (child < n && (keys[lo + child - 1] == null || keys[lo + child - 1].CompareTo(keys[lo + child]) < 0))
        //        if (child < n &&
        //            (Unsafe.Add(ref keys, lo + child - 1) == null ||
        //             Unsafe.Add(ref keys, lo + child - 1).CompareTo(Unsafe.Add(ref keys, lo + child)) < 0))
        //        {
        //            ++child;
        //        }

        //        //if (keys[lo + child - 1] == null || keys[lo + child - 1].CompareTo(d) < 0)
        //        if (Unsafe.Add(ref keys, lo + child - 1) == null ||
        //            Unsafe.Add(ref keys, lo + child - 1).CompareTo(d) < 0)
        //            break;

        //        // keys[lo + i - 1] = keys[lo + child - 1]
        //        Unsafe.Add(ref keys, lo + i - 1) = Unsafe.Add(ref keys, lo + child - 1);
        //        Unsafe.Add(ref values, lo + i - 1) = Unsafe.Add(ref values, lo + child - 1);

        //        i = child;
        //    }
        //    //keys[lo + i - 1] = d;
        //    Unsafe.Add(ref keys, lo + i - 1) = d;
        //    Unsafe.Add(ref values, lo + i - 1) = dValue;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Swap<T>(ref T items, int i, int j)
        {
            Debug.Assert(i != j);
            Swap(ref Unsafe.Add(ref items, i), ref Unsafe.Add(ref items, j));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }
}
