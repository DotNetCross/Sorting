// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

namespace System
{
    internal static partial class SpanHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<T>(this Span<T> span)
        {
            if (typeof(T) == typeof(int))
            {
                //ref var intRef = ref Unsafe.As<T, int>(ref MemoryManager.GetReference(span));
                ref var intRef = ref Unsafe.As<T, int>(ref span.DangerousGetPinnableReference());
                SpanSortHelper<int, IntLessThanComparer>.Sort(ref intRef, span.Length, new IntLessThanComparer());
            }
            else
            {
                Sort(span, Comparer<T>.Default);
            }
        }

        internal struct IntLessThanComparer : ILessThanComparer<int>
        {
            public int Compare(int x, int y) => x.CompareTo(y);
            public bool LessThan(int x, int y) => x < y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<T, TComparer>(
            this Span<T> span, TComparer comparer)
            where TComparer : IComparer<T>
        {
            SpanSortHelper<T, LessThanComparer<T, TComparer>>.s_default.Sort(span, 
                new LessThanComparer<T, TComparer>(comparer));
        }

        public interface ILessThanComparer<T> : IComparer<T>
        {
            bool LessThan(T x, T y);
        }

        // Helper to allow sharing all code via IComparer<T> inlineable
        internal struct LessThanComparer<T, TComparer> : ILessThanComparer<T>
            where TComparer : IComparer<T>
        {
            readonly TComparer _comparer;

            public LessThanComparer(TComparer comparer)
            {
                _comparer = comparer;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(T x, T y) => _comparer.Compare(x, y);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(T x, T y) => _comparer.Compare(x, y) < 0;
        }
        // Helper to allow sharing all code via IComparer<T> inlineable
        internal struct ComparableComparer<T> : ILessThanComparer<T>
            where T : IComparable<T>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(T x, T y) => x.CompareTo(y);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(T x, T y) => x.CompareTo(y) < 0;
        }
        // Helper to allow sharing all code via IComparer<T> inlineable
        internal struct ComparisonComparer<T> : IComparer<T>
        {
            readonly Comparison<T> m_comparison;

            public ComparisonComparer(Comparison<T> comparison)
            {
                m_comparison = comparison;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(T x, T y) => m_comparison(x, y);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(T x, T y) => m_comparison(x, y) < 0;
        }

        // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
        internal interface ISpanSortHelper<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            void Sort(Span<TKey> keys, in TComparer comparer);
            //int BinarySearch(Span<TKey> keys, TKey value, IComparer<TKey> comparer);
        }

        internal static class IntrospectiveSortUtilities
        {
            // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
            // https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp

            // This is the threshold where Introspective sort switches to Insertion sort.
            // Empirically, 16 seems to speed up most cases without slowing down others, at least for integers.
            // Large value types may benefit from a smaller number.
            internal const int IntrosortSizeThreshold = 16;

            internal static int FloorLog2PlusOne(int n)
            {
                int result = 0;
                while (n >= 1)
                {
                    result++;
                    n = n / 2;
                }
                return result;
            }
            // https://stackoverflow.com/questions/11376288/fast-computing-of-log2-for-64-bit-integers
            //const int tab32[32] = {
            //     0,  9,  1, 10, 13, 21,  2, 29,
            //    11, 14, 16, 18, 22, 25,  3, 30,
            //     8, 12, 20, 28, 15, 17, 24,  7,
            //    19, 27, 23,  6, 26,  5,  4, 31};

            //int log2_32(uint32_t value)
            //{
            //    value |= value >> 1;
            //    value |= value >> 2;
            //    value |= value >> 4;
            //    value |= value >> 8;
            //    value |= value >> 16;
            //    return tab32[(uint32_t)(value * 0x07C4ACDD) >> 27];
            //}
        }

        internal class SpanSortHelper
        {
        }

        internal class SpanSortHelper<T, TComparer> : ISpanSortHelper<T, TComparer>
            where TComparer : ILessThanComparer<T>
        {
            //private static volatile ISpanSortHelper<T, TComparer> defaultArraySortHelper;

            //public static ISpanSortHelper<T, TComparer> Default
            //{
            //    get
            //    {
            //        ISpanSortHelper<T, TComparer> sorter = defaultArraySortHelper;
            //        if (sorter == null)
            //            sorter = CreateArraySortHelper();

            //        return sorter;
            //    }
            //}
            internal static readonly ISpanSortHelper<T, TComparer> s_default = CreateSortHelper();

            private static ISpanSortHelper<T, TComparer> CreateSortHelper()
            {
                if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
                {
                    // TODO: Is there a faster way?
                    var ctor = typeof(ComparableSpanSortHelper<,>)
                        .MakeGenericType(new Type[] { typeof(T), typeof(TComparer) })
                        .GetConstructor(Array.Empty<Type>());

                    return (ISpanSortHelper<T, TComparer>)ctor.Invoke(Array.Empty<object>());
                    // coreclr does the following:
                    //return (IArraySortHelper<T, TComparer>)
                    //    RuntimeTypeHandle.Allocate(
                    //        .TypeHandle.Instantiate());
                }
                else
                {
                    return new SpanSortHelper<T, TComparer>();
                }
            }

            public void Sort(Span<T> keys, in TComparer comparer)
            {
                // Add a try block here to detect IComparers (or their
                // underlying IComparables, etc) that are bogus.
                // TODO: Do we need the try/catch?? Only when using default comparer?
                try
                {
                    if (typeof(TComparer) == typeof(IComparer<T>) && comparer == null)
                    {
                        SpanSortHelper<T, LessThanComparer<T, IComparer<T>>>.Sort(
                            ref keys.DangerousGetPinnableReference(), keys.Length, 
                             new LessThanComparer<T, IComparer<T>>(Comparer<T>.Default));
                    }
                    else
                    {
                        Sort(ref keys.DangerousGetPinnableReference(), keys.Length, comparer);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    //IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
                }
                catch (Exception e)
                {
                    throw e;
                    //throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
                }
            }

            internal static void Sort(ref T spanStart, int length, in TComparer comparer)
            {
                IntrospectiveSort(ref spanStart, length, comparer);
            }

            private static void IntrospectiveSort(ref T spanStart, int length, in TComparer comparer)
            {
                if (length < 2)
                    return;

                // Note how old used the full length of keys array to limit,
                //IntroSort(keys, left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2PlusOne(keys.Length), comparer);
                var depthLimit = 2 * IntrospectiveSortUtilities.FloorLog2PlusOne(length);
                IntroSort(ref spanStart, 0, length - 1, depthLimit, comparer);
                //IntroSort(ref spanStart, length - 1, depthLimit, comparer);
            }
            
            private static void IntroSort(ref T keys, int lo, int hi, int depthLimit, in TComparer comparer)
            {
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);

                while (hi > lo)
                {
                    int partitionSize = hi - lo + 1;
                    if (partitionSize <= IntrospectiveSortUtilities.IntrosortSizeThreshold)
                    {
                        if (partitionSize == 1)
                        {
                            return;
                        }
                        if (partitionSize == 2)
                        {
                            // No indeces equal here!
                            SwapIfGreater(ref keys, lo, hi, comparer);
                            return;
                        }
                        if (partitionSize == 3)
                        {
                            ref T loRef = ref Unsafe.Add(ref keys, lo);
                            ref T hiMinusOneRef = ref Unsafe.Add(ref keys, hi - 1);
                            ref T hiRef = ref Unsafe.Add(ref keys, hi);
                            //ref T hiMinusOneRef = ref Unsafe.SubtractByteOffset(ref hiRef, new IntPtr(Unsafe.SizeOf<T>()));
                            Sort3(ref loRef, ref hiMinusOneRef, ref hiRef, comparer);
                            // No indeces equal here! Many indeces can be reused here...
                            //SwapIfGreater(ref keys, comparer, lo, hi - 1);
                            //SwapIfGreater(ref keys, comparer, lo, hi);
                            //SwapIfGreater(ref keys, comparer, hi - 1, hi);
                            return;
                        }

                        InsertionSort(ref keys, lo, hi, comparer);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        HeapSort(ref keys, lo, hi, comparer);
                        return;
                    }
                    depthLimit--;

                    // We should never reach here, unless > 3 elements due to partition size
                    //int p = PickPivotAndPartitionIntIndeces(ref keys, lo, hi, comparer);
                    //int p = PickPivotAndPartitionIntPtrIndeces(ref keys, lo, hi, comparer);
                    int p = PickPivotAndPartitionIntPtrByteOffsets(ref keys, lo, hi, comparer);
                    // Note we've already partitioned around the pivot and do not have to move the pivot again.
                    IntroSort(ref keys, p + 1, hi, depthLimit, comparer);
                    hi = p - 1;
                }
            }

            private static void IntroSort(ref T keys, int hi, int depthLimit, in TComparer comparer)
            {
                Debug.Assert(comparer != null);
                //Debug.Assert(lo >= 0);
                const int lo = 0;
                while (hi > lo)
                {
                    int partitionSize = hi - lo + 1;
                    if (partitionSize <= IntrospectiveSortUtilities.IntrosortSizeThreshold)
                    {
                        if (partitionSize == 1)
                        {
                            return;
                        }

                        if (partitionSize == 2)
                        {
                            //ref T loRef = ref Unsafe.Add(ref keys, lo);
                            //ref T hiRef = ref Unsafe.Add(ref keys, hi);
                            //SwapIfGreater(ref loRef, ref hiRef, comparer);
                            // No indeces equal here!
                            SwapIfGreater(ref keys, lo, hi, comparer);
                            return;
                        }
                        if (partitionSize == 3)
                        {
                            ref T loRef = ref Unsafe.Add(ref keys, lo);
                            ref T hiMinusOneRef = ref Unsafe.Add(ref keys, hi - 1);
                            ref T hiRef = ref Unsafe.Add(ref keys, hi);
                            //ref T hiMinusOneRef = ref Unsafe.SubtractByteOffset(ref hiRef, new IntPtr(Unsafe.SizeOf<T>()));
                            Sort3(ref loRef, ref hiMinusOneRef, ref hiRef, comparer);
                            //// No indeces equal here! Many indeces can be reused here...
                            //SwapIfGreater(ref loRef, ref hiMinusOneRef, comparer);
                            //SwapIfGreater(ref loRef, ref hiRef, comparer);
                            //SwapIfGreater(ref hiMinusOneRef, ref hiRef, comparer);
                            //SwapIfGreater(ref keys, comparer, lo, hi - 1);
                            //SwapIfGreater(ref keys, comparer, lo, hi);
                            //SwapIfGreater(ref keys, comparer, hi - 1, hi);
                            return;
                        }

                        InsertionSort(ref keys, lo, hi, comparer);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        HeapSort(ref keys, lo, hi, comparer);
                        return;
                    }
                    depthLimit--;

                    // We should never reach here, unless > 3 elements due to partition size
                    //ref var keysAtLo = ref Unsafe.Add(ref keys, lo);
                    //int p = PickPivotAndPartitionIntIndeces(ref keys, lo, hi, comparer);
                    //int p = PickPivotAndPartitionIntPtrIndeces(ref keys, lo, hi, comparer);
                    int p = PickPivotAndPartitionIntPtrByteOffsets(ref keys, lo, hi, comparer);
                    // Note we've already partitioned around the pivot and do not have to move the pivot again.
                    ref var afterPivot = ref Unsafe.Add(ref keys, p + 1);
                    IntroSort(ref afterPivot, hi - (p + 1), depthLimit, comparer);
                    hi = p - 1;
                }
            }

            private static int PickPivotAndPartitionIntIndeces(ref T keys, int lo, int hi, in TComparer comparer)
            {
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                // Compute median-of-three.  But also partition them, since we've done the comparison.

                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know 
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint. 
                //       Saves one subtraction per loop compared to 
                //       `int middle = lo + ((hi - lo) >> 1);`
                int middle = (int)(((uint)hi + (uint)lo) >> 1);

                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                ref T loRef = ref Unsafe.Add(ref keys, lo);
                ref T miRef = ref Unsafe.Add(ref keys, middle);
                ref T hiRef = ref Unsafe.Add(ref keys, hi);
                Sort3(ref loRef, ref miRef, ref hiRef, comparer);
                //SwapIfGreater(ref keys, comparer, lo, middle);  // swap the low with the mid point
                //SwapIfGreater(ref keys, comparer, lo, hi);   // swap the low with the high
                //SwapIfGreater(ref keys, comparer, middle, hi); // swap the middle with the high

                T pivot = miRef;

                int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.
                Swap(ref miRef, ref Unsafe.Add(ref keys, right));

                while (left < right)
                {
                    // TODO: Would be good to update local ref here
                    while (comparer.LessThan(Unsafe.Add(ref keys, ++left), pivot)) ;
                    // TODO: Would be good to update local ref here
                    while (comparer.LessThan(pivot, Unsafe.Add(ref keys, --right))) ;

                    if (left >= right)
                        break;

                    // Indeces cannot be equal here
                    Swap(ref keys, left, right);
                }
                // Put pivot in the right location.
                right = (hi - 1);
                if (left != right)
                {
                    Swap(ref keys, left, right);
                }
                return left;
            }

            private static int PickPivotAndPartitionIntPtrIndeces(ref T keys, int lo, int hi, in TComparer comparer)
            {
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                // Compute median-of-three.  But also partition them, since we've done the comparison.

                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know 
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint. 
                //       Saves one subtraction per loop compared to 
                //       `int middle = lo + ((hi - lo) >> 1);`
                var middle = new IntPtr((int)(((uint)hi + (uint)lo) >> 1));

                var low = new IntPtr(lo);
                var high = new IntPtr(hi);

                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                ref T loRef = ref Unsafe.Add(ref keys, low);
                ref T miRef = ref Unsafe.Add(ref keys, middle);
                ref T hiRef = ref Unsafe.Add(ref keys, high);
                Sort3(ref loRef, ref miRef, ref hiRef, comparer);
                //SwapIfGreater(ref keys, comparer, lo, middle);  // swap the low with the mid point
                //SwapIfGreater(ref keys, comparer, lo, hi);   // swap the low with the high
                //SwapIfGreater(ref keys, comparer, middle, hi); // swap the middle with the high
                T pivot = miRef;

                // Put pivot in the right location.
                IntPtr left = low;
                IntPtr right = high - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.
                Swap(ref miRef, ref Unsafe.Add(ref keys, right));

                while (left.LessThan(right))
                {
                    // TODO: Would be good to update local ref here
                    do
                    {
                        left += 1;
                    }
                    while (comparer.Compare(Unsafe.Add(ref keys, left), pivot) < 0);
                    //while (comparer.Compare(pivot, Unsafe.Add(ref keys, left)) >= 0) ;
                    // TODO: Would be good to update local ref here
                    do
                    {
                        right -= 1;
                    }
                    while (comparer.Compare(pivot, Unsafe.Add(ref keys, right)) < 0);

                    //if (left >= right)
                    //if (left.GreaterThanEqual(right))
                    if (right.LessThan(left))
                        break;

                    // Indeces cannot be equal here
                    Swap(ref keys, left, right);
                }
                // Put pivot in the right location.
                right = high - 1;
                if (left != right)
                {
                    Swap(ref keys, left, right);
                }
                return (int)left;
            }

            private static int PickPivotAndPartitionIntPtrByteOffsets(ref T keys, int lo, int hi, in TComparer comparer)
            {
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                // Compute median-of-three.  But also partition them, since we've done the comparison.

                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know 
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint. 
                //       Saves one subtraction per loop compared to 
                //       `int middle = lo + ((hi - lo) >> 1);`
                var middle = new IntPtr((int)(((uint)hi + (uint)lo) >> 1));
                var low = new IntPtr(lo);
                var high = new IntPtr(hi);

                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                ref T loRef = ref Unsafe.Add(ref keys, low);
                ref T miRef = ref Unsafe.Add(ref keys, middle);
                ref T hiRef = ref Unsafe.Add(ref keys, high);
                Sort3(ref loRef, ref miRef, ref hiRef, comparer);
                //SwapIfGreater(ref keys, comparer, lo, middle);  // swap the low with the mid point
                //SwapIfGreater(ref keys, comparer, lo, hi);   // swap the low with the high
                //SwapIfGreater(ref keys, comparer, middle, hi); // swap the middle with the high

                T pivot = miRef;
                
                // Put pivot in the right location.
                IntPtr left = low;
                IntPtr right = high - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.
                Swap(ref miRef, ref Unsafe.Add(ref keys, right));

                IntPtr leftBytes = left.Multiply(Unsafe.SizeOf<T>());
                IntPtr rightBytes = right.Multiply(Unsafe.SizeOf<T>());

                while (leftBytes.LessThan(rightBytes))
                {
                    // TODO: Would be good to update local ref here
                    do
                    {
                        leftBytes += Unsafe.SizeOf<T>();
                    }
                    while (comparer.LessThan(Unsafe.AddByteOffset(ref keys, leftBytes), pivot)) ;
                    // TODO: Would be good to update local ref here
                    do
                    {
                        rightBytes -= Unsafe.SizeOf<T>();
                    }
                    while (comparer.LessThan(pivot, Unsafe.AddByteOffset(ref keys, rightBytes))) ;

                    if (leftBytes.GreaterThanEqual(rightBytes))
                        break;

                    // Indeces cannot be equal here
                    //Swap(ref keys, left, right);
                    Swap(ref Unsafe.AddByteOffset(ref keys, leftBytes), ref Unsafe.AddByteOffset(ref keys, rightBytes));
                }
                // Put pivot in the right location.
                //right = (hi - 1);
                rightBytes = new IntPtr(hi - 1).Multiply(Unsafe.SizeOf<T>());
                if (leftBytes != rightBytes)
                {
                    //Swap(ref keys, left, right);
                    Swap(ref Unsafe.AddByteOffset(ref keys, leftBytes), ref Unsafe.AddByteOffset(ref keys, rightBytes));
                }
                return (int)leftBytes.Divide(Unsafe.SizeOf<T>());
            }

            private static void HeapSort(ref T keys, int lo, int hi, in TComparer comparer)
            {
                Debug.Assert(keys != null);
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                int n = hi - lo + 1;
                for (int i = n / 2; i >= 1; --i)
                {
                    DownHeap(ref keys, i, n, lo, comparer);
                }
                for (int i = n; i > 1; --i)
                {
                    Swap(ref keys, lo, lo + i - 1);
                    DownHeap(ref keys, 1, i - 1, lo, comparer);
                }
            }

            private static void DownHeap(ref T keys, int i, int n, int lo, in TComparer comparer)
            {
                Debug.Assert(keys != null);
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);

                ////T d = keys[lo + i - 1];
                //T d = Unsafe.Add(ref keys, lo + i - 1);
                //int child;
                //while (i <= n / 2)
                //{
                //    child = 2 * i;
                //    //if (child < n && comparer(keys[lo + child - 1], keys[lo + child]) < 0)
                //    if (child < n && comparer.Compare(Unsafe.Add(ref keys, lo + child - 1),
                //        Unsafe.Add(ref keys, lo + child)) < 0)
                //    {
                //        child++;
                //    }
                //    //if (!(comparer(d, keys[lo + child - 1]) < 0))
                //    if (!(comparer.Compare(d, Unsafe.Add(ref keys, lo + child - 1)) < 0))
                //        break;
                //    // keys[lo + i - 1] = keys[lo + child - 1]
                //    Unsafe.Add(ref keys, lo + i - 1) = Unsafe.Add(ref keys, lo + child - 1);
                //    i = child;
                //}
                ////keys[lo + i - 1] = d;
                //Unsafe.Add(ref keys, lo + i - 1) = d;

                //T d = keys[lo + i - 1];
                ref T refLo = ref Unsafe.Add(ref keys, lo);
                ref T refLoMinus1 = ref Unsafe.Subtract(ref refLo, 1);
                T d = Unsafe.Add(ref refLoMinus1, i);
                var nHalf = n / 2;
                while (i <= nHalf)
                {
                    int child = i << 1;

                    //if (child < n && comparer(keys[lo + child - 1], keys[lo + child]) < 0)
                    if (child < n && 
                        comparer.LessThan(Unsafe.Add(ref refLoMinus1, child), Unsafe.Add(ref refLo, child)))
                    {
                        ++child;
                    }

                    //if (!(comparer(d, keys[lo + child - 1]) < 0))
                    if (!(comparer.LessThan(d, Unsafe.Add(ref refLoMinus1, child))))
                        break;

                    // keys[lo + i - 1] = keys[lo + child - 1]
                    Unsafe.Add(ref refLoMinus1, i) = Unsafe.Add(ref refLoMinus1, child);

                    i = child;
                }
                //keys[lo + i - 1] = d;
                Unsafe.Add(ref keys, lo + i - 1) = d;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InsertionSort(ref T keys, int lo, int hi, in TComparer comparer)
            {
                Debug.Assert(keys != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi >= lo);

                for (int i = lo; i < hi; i++)
                {
                    //t = keys[i + 1];
                    var t = Unsafe.Add(ref keys, i + 1);
                    // Need local ref that can be updated!
                    int j = i;
                    while (j >= lo && comparer.LessThan(t, Unsafe.Add(ref keys, j)))
                    {
                        Unsafe.Add(ref keys, j + 1) = Unsafe.Add(ref keys, j);
                        --j;
                    }
                    Unsafe.Add(ref keys, j + 1) = t;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Sort3(ref T r0, ref T r1, ref T r2, in TComparer comparer)
            {
                if (comparer.LessThan(r0, r1)) //r0 < r1)
                {
                    if (comparer.LessThan(r1, r2)) //(r1 < r2)
                    {
                        return;
                    }
                    else if (comparer.LessThan(r0, r2)) //(r0 < r2)
                    {
                        Swap(ref r1, ref r2); //std::swap(r1, r2);
                    }
                    else
                    {
                        T tmp = r0;
                        r0 = r2;
                        r2 = r1;
                        r1 = tmp;
                    }
                }
                else
                {
                    if (comparer.LessThan(r0, r2)) //(r0 < r2)
                    {
                        Swap(ref r0, ref r1); //std::swap(r0, r1);
                    }
                    else if (comparer.LessThan(r2, r1)) //(r2 < r1)
                    {
                        Swap(ref r0, ref r2); //std::swap(r0, r2);
                    }
                    else
                    {
                        T tmp = r0;
                        r0 = r1;
                        r1 = r2;
                        r2 = tmp;
                    }
                }
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void SwapIfGreater(ref T keys, int i, int j, TComparer comparer)
            {
                Debug.Assert(i != j);
                // Check moved to the one case actually needing it, not all!
                //if (i != j)
                {
                    ref var iElement = ref Unsafe.Add(ref keys, i);
                    ref var jElement = ref Unsafe.Add(ref keys, j);
                    SwapIfGreater(ref iElement, ref jElement, comparer);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void SwapIfGreater(ref T a, ref T b, in TComparer comparer)
            {
                //if (comparer.Compare(a, b) > 0)
                if (comparer.LessThan(b, a))
                {
                    T temp = a;
                    a = b;
                    b = temp;
                }
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Swap(ref T keys, int i, int j)
            {
                // TODO: Is the i!=j check necessary? Most cases not needed?
                // Only in one case it seems, REFACTOR
                Debug.Assert(i != j);
                // No place needs this it seems
                //if (i != j)
                {
                    ref var iElement = ref Unsafe.Add(ref keys, i);
                    ref var jElement = ref Unsafe.Add(ref keys, j);
                    Swap(ref iElement, ref jElement);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Swap(ref T keys, IntPtr i, IntPtr j)
            {
                // TODO: Is the i!=j check necessary? Most cases not needed?
                // Only in one case it seems, REFACTOR
                Debug.Assert(i != j);
                // No place needs this it seems
                //if (i != j)
                {
                    ref var iElement = ref Unsafe.Add(ref keys, i);
                    ref var jElement = ref Unsafe.Add(ref keys, j);
                    Swap(ref iElement, ref jElement);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Swap(ref T a, ref T b)
            {
                T temp = a;
                a = b;
                b = temp;
            }
        }

        internal class ComparableSpanSortHelper<T, TComparer>
        : ISpanSortHelper<T, TComparer>
        where T : IComparable<T>
        where TComparer : IComparer<T>
        {
            // Do not add a constructor to this class because SpanSortHelper<T>.CreateSortHelper will not execute it

            public void Sort(Span<T> keys, in TComparer comparer)
            {
                try
                {
                    if (comparer == null ||
                        // Cache this in generic traits helper class perhaps
                        (!typeof(TComparer).IsValueType &&
                         object.ReferenceEquals(comparer, Comparer<T>.Default)))
                    {
                        SpanSortHelper<T, ComparableComparer<T>>.Sort(
                            ref keys.DangerousGetPinnableReference(), keys.Length,
                            new ComparableComparer<T>());
                    }
                    else
                    {
                        SpanSortHelper<T, LessThanComparer<T, TComparer>>.Sort(
                            ref keys.DangerousGetPinnableReference(), keys.Length,
                            new LessThanComparer<T, TComparer>(comparer));
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    //IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
                }
                catch (Exception e)
                {
                    throw e;
                    //throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
                }
            }

            //internal class ArraySortHelper<T, TComparer>
            //    : ISpanSortHelper<T, TComparer>
            //    where TComparer : IComparer<T>
            //{

            //public int BinarySearch(Span<T> array, T value, TComparer comparer)
            //{
            //    try
            //    {
            //        if (comparer == null)
            //        {
            //            comparer = Comparer<T>.Default;
            //        }

            //        return InternalBinarySearch(array, index, length, value, comparer);
            //    }
            //    catch (Exception e)
            //    {
            //        throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
            //    }
            //}

            //internal static void Sort(Span<T> keys, Comparison<T> comparer)
            //{
            //    Debug.Assert(keys != null, "Check the arguments in the caller!");
            //    Debug.Assert(index >= 0 && length >= 0 && (keys.Length - index >= length), "Check the arguments in the caller!");
            //    Debug.Assert(comparer != null, "Check the arguments in the caller!");

            //    // Add a try block here to detect bogus comparisons
            //    try
            //    {
            //        IntrospectiveSort(keys, index, length, comparer);
            //    }
            //    catch (IndexOutOfRangeException)
            //    {
            //        IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
            //    }
            //    catch (Exception e)
            //    {
            //        throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
            //    }
            //}

            //internal static int InternalBinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer)
            //{
            //    Debug.Assert(array != null, "Check the arguments in the caller!");
            //    Debug.Assert(index >= 0 && length >= 0 && (array.Length - index >= length), "Check the arguments in the caller!");

            //    int lo = index;
            //    int hi = index + length - 1;
            //    while (lo <= hi)
            //    {
            //        int i = lo + ((hi - lo) >> 1);
            //        int order = comparer.Compare(array[i], value);

            //        if (order == 0)
            //            return i;
            //        if (order < 0)
            //        {
            //            lo = i + 1;
            //        }
            //        else
            //        {
            //            hi = i - 1;
            //        }
            //    }

            //    return ~lo;
            //}

            //}

        }
    }
}
