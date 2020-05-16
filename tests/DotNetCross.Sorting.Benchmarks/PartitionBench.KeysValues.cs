using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Code;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using DotNetCross.Sorting.Sequences;
using TKey = System.Int32;
using TValue = System.String;

namespace DotNetCross.Sorting.Benchmarks
{
    public class Int32StringPartitionBench : PartitionBench//<int, string>
    {
        public Int32StringPartitionBench()
            : base(maxLength: 20*1000*1000, new[] { 1000000 }, //2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i, i => i.ToString("D9"))
        { }
    }

    [Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
    [Config(typeof(SortBenchConfig))]
    [MemoryDiagnoser]
    public class PartitionBench//<TKey, TValue>
        //where TKey : IComparable<TKey>
    {
        readonly int _maxLength;
        readonly ISpanFiller[] _fillers;
        readonly Func<int, TKey> _toKey;
        readonly TKey[] _filled;
        readonly TKey[] _work;
        readonly Func<int, TValue> _toValue;
        readonly TValue[] _filledValues;
        readonly TValue[] _workValues;

        public PartitionBench(int maxLength, int[] sliceLengths, ISpanFiller[] fillers, 
            Func<int, TKey> toKey, Func<int, TValue> toValue)
        {
            _maxLength = maxLength;
            Lengths = sliceLengths;
            _fillers = fillers;
            _toKey = toKey;
            _filled = new TKey[_maxLength];
            _work = new TKey[_maxLength];
            _toValue = toValue;
            _filledValues = new TValue[_maxLength];
            _workValues = new TValue[_maxLength];
        }

        [ParamsSource(nameof(Fillers))]
        public ISpanFiller Filler { get; set; }

        public IEnumerable<ISpanFiller> Fillers() => _fillers;

        [ParamsSource(nameof(Lengths))]
        public int Length { get; set; }

        public IEnumerable<int> Lengths { get; }


        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine($"// {nameof(GlobalSetup)} Filling {_maxLength} with {Filler.GetType().Name} for {Length} slice run");
            Filler.Fill(_filled, Length, _toKey);
            new IncrementingSpanFiller().Fill(_filledValues, Length, _toValue);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            Array.Copy(_filled, _work, _maxLength);
            Array.Copy(_filledValues, _workValues, _maxLength);
        }

#if !NETCOREAPP3_1
        [Benchmark(Baseline = true)]
        public void CLR_()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                var keys = new Span<TKey>(_work, i, Length);
                var values = new Span<TValue>(_workValues, i, Length);
                CLR.PickPivotAndPartition(keys, values);
            }
        }
        //[Benchmark]
        public void CLR_ClassComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort(new Span<TValue>(_workValues, i, Length), ClassComparableComparer<TKey>.Instance);
            }
        }
        //[Benchmark]
        public void CLR_StructComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).Sort(new Span<TValue>(_workValues, i, Length), new StructComparableComparer<TKey>());
            }
        }
        //[Benchmark]
        public void CLR_Comparison()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                var workSpan = new Span<TKey>(_work, i, Length);
                var workValuesSpan = new Span<TValue>(_workValues, i, Length);
                workSpan.Sort(workValuesSpan, ComparableComparison<TKey>.Instance);
            }
        }
#endif

        [Benchmark]
        public void DNX_()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                var keys = new Span<TKey>(_work, i, Length);
                var values = new Span<TValue>(_workValues, i, Length);
                DNX.PickPivotAndPartition(ref MemoryMarshal.GetReference(keys), ref MemoryMarshal.GetReference(values),
                    keys.Length);
            }
        }
        //[Benchmark]
        public void DNX_NullComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort(new Span<TValue>(_workValues, i, Length), (IComparer<TKey>)null);
            }
        }
        //[Benchmark]
        public void DNX_ClassComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort(new Span<TValue>(_workValues, i, Length), ClassComparableComparer<TKey>.Instance);
            }
        }
        //[Benchmark]
        public void DNX_StructComparableComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort(new Span<TValue>(_workValues, i, Length), new StructComparableComparer<TKey>());
            }
        }
        //[Benchmark]
        public void DNX_Comparison()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<TKey>(_work, i, Length).IntroSort(new Span<TValue>(_workValues, i, Length), ComparableComparison<TKey>.Instance);
            }
        }

        public static class DNX
        {
            internal static int PickPivotAndPartition(
                ref TKey keys, ref TValue values, int length)
            {

                Debug.Assert(length > 2);
                //
                // Compute median-of-three.  But also partition them, since we've done the comparison.
                //
                // Sort left, middle and right appropriately, then pick middle as the pivot.
                int middle = (length - 1) >> 1;
                ref TKey keysAtMiddle = ref Sort3(ref keys, ref values, 0, middle, length - 1);

                TKey pivot = keysAtMiddle;

                int left = 0;
                int nextToLast = length - 2;
                int right = nextToLast;
                ref TKey keysLeft = ref Unsafe.Add(ref keys, left);
                ref TKey keysRight = ref Unsafe.Add(ref keys, right);
                // We already partitioned lo and hi and put the pivot in hi - 1.  
                // And we pre-increment & decrement below.
                Swap(ref keysAtMiddle, ref keysRight);
                Swap(ref values, middle, right);

                while (left < right)
                {
                    if (pivot == null)
                    {
                        do { ++left; keysLeft = ref Unsafe.Add(ref keysLeft, 1); }
                        while (left < right && keysLeft == null);

                        do { --right; keysRight = ref Unsafe.Add(ref keysRight, -1); }
                        while (right > 0 && keysRight != null);
                    }
                    else
                    {
                        do { ++left; keysLeft = ref Unsafe.Add(ref keysLeft, 1); }
                        while (left < right && pivot.CompareTo(keysLeft) > 0);
                        // Check if bad comparable/comparer
                        if (left == right && pivot.CompareTo(keysLeft) > 0)
                            ThrowHelper.ThrowArgumentException_BadComparable(typeof(TKey));

                        do { --right; keysRight = ref Unsafe.Add(ref keysRight, -1); }
                        while (right > 0 && pivot.CompareTo(keysRight) < 0);
                        // Check if bad comparable/comparer
                        if (right == 0 && pivot.CompareTo(keysRight) < 0)
                            ThrowHelper.ThrowArgumentException_BadComparable(typeof(TKey));
                    }

                    if (left >= right)
                        break;

                    // PERF: Swap manually inlined here for better code-gen
                    var t = keysLeft;
                    keysLeft = keysRight;
                    keysRight = t;
                    // PERF: Swap manually inlined here for better code-gen
                    ref var valuesLeft = ref Unsafe.Add(ref values, left);
                    ref var valuesRight = ref Unsafe.Add(ref values, right);
                    var v = valuesLeft;
                    valuesLeft = valuesRight;
                    valuesRight = v;

                    //    while (left < nextToLast && comparison(Unsafe.Add(ref keys, ++left), pivot) < 0) ;
                    //    // Check if bad comparable/comparison
                    //    if (left == nextToLast && comparison(Unsafe.Add(ref keys, left), pivot) < 0)
                    //        ThrowHelper.ThrowArgumentException_BadComparer(comparison);

                    //    while (right > 0 && comparison(pivot, Unsafe.Add(ref keys, --right)) < 0) ;
                    //    // Check if bad comparable/comparison
                    //    if (right == 0 && comparison(pivot, Unsafe.Add(ref keys, right)) < 0)
                    //        ThrowHelper.ThrowArgumentException_BadComparer(comparison);
                    //}
                    //if (left >= right)
                    //    break;

                    //Swap(ref keys, left, right);
                    //Swap(ref values, left, right);

                }
                // Put pivot in the right location.
                right = nextToLast;
                if (left != right)
                {
                    Swap(ref keys, left, right);
                    Swap(ref values, left, right);
                }
                return left;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ref TKey Sort3(
    ref TKey keys, ref TValue values, int i0, int i1, int i2)
            {
                ref var r0 = ref Unsafe.Add(ref keys, i0);
                ref var r1 = ref Unsafe.Add(ref keys, i1);
                ref var r2 = ref Unsafe.Add(ref keys, i2);
                Sort2(ref r0, ref r1, ref values, i0, i1);
                Sort2(ref r0, ref r2, ref values, i0, i2);
                Sort2(ref r1, ref r2, ref values, i1, i2);

                //ref var r0 = ref Unsafe.Add(ref keys, i0);
                //ref var r1 = ref Unsafe.Add(ref keys, i1);
                //ref var r2 = ref Unsafe.Add(ref keys, i2);

                //if (r0 != null && r0.CompareTo(r1) <= 0) //r0 <= r1)
                //{
                //    if (r1 != null && r1.CompareTo(r2) <= 0) //(r1 <= r2)
                //    {
                //        return ref r1;
                //    }
                //    else if (r0.CompareTo(r2) < 0) //(r0 < r2)
                //    {
                //        Swap(ref r1, ref r2);
                //        ref var v1 = ref Unsafe.Add(ref values, i1);
                //        ref var v2 = ref Unsafe.Add(ref values, i2);
                //        Swap(ref v1, ref v2);
                //    }
                //    else
                //    {
                //        TKey tmp = r0;
                //        r0 = r2;
                //        r2 = r1;
                //        r1 = tmp;
                //        ref var v0 = ref Unsafe.Add(ref values, i0);
                //        ref var v1 = ref Unsafe.Add(ref values, i1);
                //        ref var v2 = ref Unsafe.Add(ref values, i2);
                //        TValue vTemp = v0;
                //        v0 = v2;
                //        v2 = v1;
                //        v1 = vTemp;
                //    }
                //}
                //else
                //{
                //    if (r0 != null && r0.CompareTo(r2) < 0) //(r0 < r2)
                //    {
                //        Swap(ref r0, ref r1);
                //        ref var v0 = ref Unsafe.Add(ref values, i0);
                //        ref var v1 = ref Unsafe.Add(ref values, i1);
                //        Swap(ref v0, ref v1);
                //    }
                //    else if (r2 != null && r2.CompareTo(r1) < 0) //(r2 < r1)
                //    {
                //        Swap(ref r0, ref r2);
                //        ref var v0 = ref Unsafe.Add(ref values, i0);
                //        ref var v2 = ref Unsafe.Add(ref values, i2);
                //        Swap(ref v0, ref v2);
                //    }
                //    else
                //    {
                //        TKey tmp = r0;
                //        r0 = r1;
                //        r1 = r2;
                //        r2 = tmp;
                //        ref var v0 = ref Unsafe.Add(ref values, i0);
                //        ref var v1 = ref Unsafe.Add(ref values, i1);
                //        ref var v2 = ref Unsafe.Add(ref values, i2);
                //        TValue vTemp = v0;
                //        v0 = v1;
                //        v1 = v2;
                //        v2 = vTemp;
                //    }
                //}
                return ref r1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Sort2(
                        ref TKey keys, ref TValue values, int i, int j)
            {
                Debug.Assert(i != j);

                ref TKey a = ref Unsafe.Add(ref keys, i);
                ref TKey b = ref Unsafe.Add(ref keys, j);
                Sort2(ref a, ref b, ref values, i, j);
            }

            internal static void Sort2(
                ref TKey a, ref TKey b, ref TValue values, int i, int j)
            {
                if (a != null && a.CompareTo(b) > 0)
                {
                    Swap(ref a, ref b);
                    Swap(ref values, i, j);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Swap<T>(ref T items, int i, int j)
            {
                Debug.Assert(i != j);
                Swap(ref Unsafe.Add(ref items, i), ref Unsafe.Add(ref items, j));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Swap<T>(ref T a, ref T b)
            {
                var t = a;
                a = b;
                b = t;
            }
        }

        public static class CLR
        {
            //public static void Sort(Span<TKey> keys, Span<TValue> values, IComparer<TKey>? comparer)
            //{
            //    // Add a try block here to detect IComparers (or their
            //    // underlying IComparables, etc) that are bogus.
            //    try
            //    {
            //        if (comparer == null || comparer == Comparer<TKey>.Default)
            //        {
            //            IntrospectiveSort(keys, values);
            //        }
            //        else
            //        {
            //            throw new NotImplementedException();
            //            //ArraySortHelper<TKey, TValue>.IntrospectiveSort(keys, values, comparer);
            //        }
            //    }
            //    catch (IndexOutOfRangeException)
            //    {
            //        ThrowHelper.ThrowArgumentException_BadComparer(comparer);
            //    }
            //    //catch (Exception e)
            //    {
            //        //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
            //    }
            //}

            private static void SwapIfGreaterWithValues(Span<TKey> keys, Span<TValue> values, int i, int j)
            {
                Debug.Assert(i != j);

                if (keys[i] != null && keys[i].CompareTo(keys[j]) > 0)
                {
                    TKey key = keys[i];
                    keys[i] = keys[j];
                    keys[j] = key;

                    TValue value = values[i];
                    values[i] = values[j];
                    values[j] = value;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Swap(Span<TKey> keys, Span<TValue> values, int i, int j)
            {
                Debug.Assert(i != j);

                TKey k = keys[i];
                keys[i] = keys[j];
                keys[j] = k;

                TValue v = values[i];
                values[i] = values[j];
                values[j] = v;
            }

            internal static void IntrospectiveSort(Span<TKey> keys, Span<TValue> values)
            {
                Debug.Assert(keys.Length == values.Length);

                if (keys.Length > 1)
                {

                    IntroSort(keys, values, 2 * Common.FloorLog2PlusOne(keys.Length));//(BitOperations.Log2((uint)keys.Length) + 1));
                }
            }

            private static void IntroSort(Span<TKey> keys, Span<TValue> values, int depthLimit)
            {
                Debug.Assert(!keys.IsEmpty);
                Debug.Assert(values.Length == keys.Length);
                Debug.Assert(depthLimit >= 0);

                int partitionSize = keys.Length;
                while (partitionSize > 1)
                {
                    if (partitionSize <= Common.IntrosortSizeThreshold)
                    {

                        if (partitionSize == 2)
                        {
                            SwapIfGreaterWithValues(keys, values, 0, 1);
                            return;
                        }

                        if (partitionSize == 3)
                        {
                            SwapIfGreaterWithValues(keys, values, 0, 1);
                            SwapIfGreaterWithValues(keys, values, 0, 2);
                            SwapIfGreaterWithValues(keys, values, 1, 2);
                            return;
                        }

                        InsertionSort(keys.Slice(0, partitionSize), values.Slice(0, partitionSize));
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        HeapSort(keys.Slice(0, partitionSize), values.Slice(0, partitionSize));
                        return;
                    }
                    depthLimit--;

                    int p = PickPivotAndPartition(keys.Slice(0, partitionSize), values.Slice(0, partitionSize));

                    // Note we've already partitioned around the pivot and do not have to move the pivot again.
                    IntroSort(keys[(p + 1)..partitionSize], values[(p + 1)..partitionSize], depthLimit);
                    partitionSize = p;
                }
            }

            internal static int PickPivotAndPartition(Span<TKey> keys, Span<TValue> values)
            {
                //Debug.Assert(keys.Length >= Array.IntrosortSizeThreshold);

                int hi = keys.Length - 1;

                // Compute median-of-three.  But also partition them, since we've done the comparison.
                int middle = hi >> 1;

                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                SwapIfGreaterWithValues(keys, values, 0, middle);  // swap the low with the mid point
                SwapIfGreaterWithValues(keys, values, 0, hi);   // swap the low with the high
                SwapIfGreaterWithValues(keys, values, middle, hi); // swap the middle with the high

                TKey pivot = keys[middle];
                Swap(keys, values, middle, hi - 1);
                int left = 0, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

                while (left < right)
                {
                    if (pivot == null)
                    {
                        while (left < (hi - 1) && keys[++left] == null) ;
                        while (right > 0 && keys[--right] != null) ;
                    }
                    else
                    {
                        while (pivot.CompareTo(keys[++left]) > 0) ;
                        while (pivot.CompareTo(keys[--right]) < 0) ;
                    }

                    if (left >= right)
                        break;

                    Swap(keys, values, left, right);
                }

                // Put pivot in the right location.
                if (left != hi - 1)
                {
                    Swap(keys, values, left, hi - 1);
                }
                return left;
            }

            private static void HeapSort(Span<TKey> keys, Span<TValue> values)
            {
                Debug.Assert(!keys.IsEmpty);

                int n = keys.Length;
                for (int i = n >> 1; i >= 1; i--)
                {
                    DownHeap(keys, values, i, n, 0);
                }

                for (int i = n; i > 1; i--)
                {
                    Swap(keys, values, 0, i - 1);
                    DownHeap(keys, values, 1, i - 1, 0);
                }
            }

            private static void DownHeap(Span<TKey> keys, Span<TValue> values, int i, int n, int lo)
            {
                Debug.Assert(lo >= 0);
                Debug.Assert(lo < keys.Length);

                TKey d = keys[lo + i - 1];
                TValue dValue = values[lo + i - 1];

                while (i <= n >> 1)
                {
                    int child = 2 * i;
                    if (child < n && (keys[lo + child - 1] == null || keys[lo + child - 1].CompareTo(keys[lo + child]) < 0))
                    {
                        child++;
                    }

                    if (keys[lo + child - 1] == null || keys[lo + child - 1].CompareTo(d) < 0)
                        break;

                    keys[lo + i - 1] = keys[lo + child - 1];
                    values[lo + i - 1] = values[lo + child - 1];
                    i = child;
                }

                keys[lo + i - 1] = d;
                values[lo + i - 1] = dValue;
            }

            private static void InsertionSort(Span<TKey> keys, Span<TValue> values)
            {
                for (int i = 0; i < keys.Length - 1; i++)
                {
                    TKey t = keys[i + 1];
                    TValue tValue = values[i + 1];

                    int j = i;
                    while (j >= 0 && (t == null || t.CompareTo(keys[j]) < 0))
                    {
                        keys[j + 1] = keys[j];
                        values[j + 1] = values[j];
                        j--;
                    }

                    keys[j + 1] = t;
                    values[j + 1] = tValue;
                }
            }
        }
    }

}
