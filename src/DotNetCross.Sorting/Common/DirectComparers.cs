﻿using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    // This started out with just LessThan.
    // However, due to bogus comparers, comparables etc.
    // we need to preserve semantics completely to get same result.
    internal interface IDirectComparer<in T>
    {
        bool GreaterThan(T x, T y);
        bool LessThan(T x, T y);
        bool LessThanEqual(T x, T y); // TODO: Delete if we are not doing specialize Sort3
    }
    //
    // Type specific DirectComparer(s) to ensure optimal code-gen
    //
    internal struct SByteDirectComparer : IDirectComparer<sbyte>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GreaterThan(sbyte x, sbyte y) => x > y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThan(sbyte x, sbyte y) => x < y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThanEqual(sbyte x, sbyte y) => x <= y;
    }
    internal struct ByteDirectComparer : IDirectComparer<byte>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GreaterThan(byte x, byte y) => x > y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThan(byte x, byte y) => x < y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThanEqual(byte x, byte y) => x <= y;
    }
    internal struct Int16DirectComparer : IDirectComparer<short>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GreaterThan(short x, short y) => x > y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThan(short x, short y) => x < y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThanEqual(short x, short y) => x <= y;
    }
    internal struct UInt16DirectComparer : IDirectComparer<ushort>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GreaterThan(ushort x, ushort y) => x > y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThan(ushort x, ushort y) => x < y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThanEqual(ushort x, ushort y) => x <= y;
    }
    internal struct Int32DirectComparer : IDirectComparer<int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GreaterThan(int x, int y) => x > y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThan(int x, int y) => x < y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThanEqual(int x, int y) => x <= y;
    }
    internal struct UInt32DirectComparer : IDirectComparer<uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GreaterThan(uint x, uint y) => x > y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThan(uint x, uint y) => x < y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThanEqual(uint x, uint y) => x <= y;
    }
    internal struct Int64DirectComparer : IDirectComparer<long>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GreaterThan(long x, long y) => x > y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThan(long x, long y) => x < y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThanEqual(long x, long y) => x <= y;
    }
    internal struct UInt64DirectComparer : IDirectComparer<ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GreaterThan(ulong x, ulong y) => x > y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThan(ulong x, ulong y) => x < y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThanEqual(ulong x, ulong y) => x <= y;
    }
    internal struct SingleDirectComparer : IDirectComparer<float>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GreaterThan(float x, float y) => x > y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThan(float x, float y) => x < y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThanEqual(float x, float y) => x <= y;
    }
    internal struct DoubleDirectComparer : IDirectComparer<double>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GreaterThan(double x, double y) => x > y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThan(double x, double y) => x < y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThanEqual(double x, double y) => x <= y;
    }
    // TODO: Revise whether this is needed
    internal struct StringDirectComparer : IDirectComparer<string>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GreaterThan(string x, string y) => x.CompareTo(y) > 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThan(string x, string y) => x.CompareTo(y) < 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LessThanEqual(string x, string y) => x.CompareTo(y) <= 0;
    }
}
