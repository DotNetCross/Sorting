using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    // TODO: Rename to SpanSortHelpers before move to corefx
    internal static partial class SpanSortHelpersCommon
    {

        // This is the threshold where Introspective sort switches to Insertion sort.
        // Empirically, 16 seems to speed up most cases without slowing down others, at least for integers.
        // Large value types may benefit from a smaller number.
        internal const int IntrosortSizeThreshold = 16;

        internal static int FloorLog2PlusOne(int n)
        {
            Debug.Assert(n >= 2);
            int result = 0;
            do
            {
                ++result;
                n >>= 1;
            }
            while (n > 0);

            return result;
        }
        
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


        // canonical instantiation of a generic type (is an issue for perf for reference types)
        // since the value type generic comparer does not work for that...
        // https://blogs.msdn.microsoft.com/carlos/2009/11/09/net-generics-and-code-bloat-or-its-lack-thereof/
        // which I am not sure why because the comparer itself is a value type...

        // To work around canonical instantiation of a generic type
        // We use "Reference" as a placeholder...
        internal struct Reference
        {
            internal object o;
        }

        internal interface ILessThanComparer
        {
            bool LessThan<T>(T x, T y);
        }
        internal struct ComparerLessThanComparerNew<TKey, TComparer> : ILessThanComparer
            where TComparer : IComparer<TKey>
        {
            readonly TComparer _comparer;

            public ComparerLessThanComparerNew(in TComparer comparer)
            {
                _comparer = comparer;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan<T>(T x, T y) => _comparer.Compare(Unsafe.As<T, TKey>(ref x), Unsafe.As<T, TKey>(ref y)) < 0;
        }
        internal struct ComparableLessThanComparerNew<TKey> : ILessThanComparer
            where TKey : struct, IComparable<TKey>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan<T>(T x, T y) => Unsafe.As<T, TKey>(ref x).CompareTo(Unsafe.As<T, TKey>(ref y)) < 0;
        }
        internal struct IComparableLessThanComparer : ILessThanComparer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan<T>(T x, T y) => Unsafe.As<T, IComparable<T>>(ref x).CompareTo(y) < 0;
        }
        internal struct IComparableLessThanComparerNew<TKey> : ILessThanComparer
            where TKey : class, IComparable<TKey>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan<T>(T x, T y) => Unsafe.As<IComparable<TKey>>(Unsafe.As<T, Reference>(ref x).o).CompareTo(
                Unsafe.As<TKey>(Unsafe.As<T, Reference>(ref y).o)) < 0;
        }

        internal interface ILessThanComparer<in T>
        {
            bool LessThan(T x, T y);
        }
        //
        // Type specific LessThanComparer(s) to ensure optimal code-gen
        //
        internal struct SByteLessThanComparer : ILessThanComparer<sbyte>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(sbyte x, sbyte y) => x < y;
        }
        internal struct ByteLessThanComparer : ILessThanComparer<byte>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(byte x, byte y) => x < y;
        }
        internal struct Int16LessThanComparer : ILessThanComparer<short>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(short x, short y) => x < y;
        }
        internal struct UInt16LessThanComparer : ILessThanComparer<ushort>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(ushort x, ushort y) => x < y;
        }
        internal struct Int32LessThanComparer : ILessThanComparer<int>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(int x, int y) => x < y;
        }
        internal struct UInt32LessThanComparer : ILessThanComparer<uint>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(uint x, uint y) => x < y;
        }
        internal struct Int64LessThanComparer : ILessThanComparer<long>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(long x, long y) => x < y;
        }
        internal struct UInt64LessThanComparer : ILessThanComparer<ulong>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(ulong x, ulong y) => x < y;
        }
        internal struct SingleLessThanComparer : ILessThanComparer<float>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(float x, float y) => x < y;
        }
        internal struct DoubleLessThanComparer : ILessThanComparer<double>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(double x, double y) => x < y;
        }
        internal struct StringLessThanComparer : ILessThanComparer<string>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(string x, string y) => x.CompareTo(y) < 0;
        }

        // Helper to allow sharing all code via inlineable functor for IComparer<T>
        // Does not work well for reference types
        internal struct ComparerLessThanComparer<T, TComparer> : ILessThanComparer<T>
            where TComparer : IComparer<T>
        {
            readonly TComparer _comparer;

            public ComparerLessThanComparer(in TComparer comparer)
            {
                _comparer = comparer;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(T x, T y) => _comparer.Compare(x, y) < 0;
        }
        // Helper to allow sharing all code via inlineable functor for IComparable<T>
        // Does not work well for reference types
        internal struct ComparableLessThanComparer<T> : ILessThanComparer<T>
            where T : IComparable<T>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(T x, T y) => x.CompareTo(y) < 0;
        }

        //internal struct IComparableLessThanComparer<T> : ILessThanComparer<IComparable<T>>
        //    where T : class, IComparable<T> // Do we need to constrain? Not really
        //{
        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    public bool LessThan(IComparable<T> x, IComparable<T> y) => x.CompareTo(Unsafe.As<T>(y)) < 0; // Cast??
        //}


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
        }


        internal interface IIsNaN<T>
        {
            bool IsNaN(T value);
        }
        internal struct SingleIsNaN : IIsNaN<float>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsNaN(float value) => float.IsNaN(value);
        }
        internal struct DoubleIsNaN : IIsNaN<double>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsNaN(double value) => double.IsNaN(value);
        }
    }
}
