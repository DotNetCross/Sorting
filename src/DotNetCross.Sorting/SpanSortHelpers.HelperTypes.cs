﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System
{
    // TODO: Rename to SpanSortHelpers before move to corefx
    internal static partial class SpanSortHelpersHelperTypes
    {
        internal interface ILessThanComparer<T>
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

        // Helper to allow sharing all code via inlineable functor for IComparer<T>
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
        internal struct ComparableLessThanComparer<T> : ILessThanComparer<T>//, IComparer<T>
            where T : IComparable<T>
        {
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
