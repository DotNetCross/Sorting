// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

using DotNetCross.Sorting;
using static DotNetCross.Sorting.Swapper;
using static DotNetCross.Sorting.TDirectComparerImpl;

namespace System
{
    internal static partial class SpanSortHelpersKeys_DirectComparer
    {
        // https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySortSpecialized<TKey>(
            ref TKey keys, int length)
        {
            // Type unfolding adopted from https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp#L268
            if (typeof(TKey) == typeof(sbyte))
            {
                ref var specificKeys = ref Unsafe.As<TKey, sbyte>(ref keys);
                IntroSort(ref specificKeys, length, new SByteDirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(byte) ||
                     typeof(TKey) == typeof(bool)) // Use byte for bools to reduce code size
            {
                ref var specificKeys = ref Unsafe.As<TKey, byte>(ref keys);
                IntroSort(ref specificKeys, length, new ByteDirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(short))
            {
                ref var specificKeys = ref Unsafe.As<TKey, short>(ref keys);
                IntroSort(ref specificKeys, length, new Int16DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(ushort) ||
                     typeof(TKey) == typeof(char)) // Use ushort for chars to reduce code size)
            {
                ref var specificKeys = ref Unsafe.As<TKey, ushort>(ref keys);
                IntroSort(ref specificKeys, length, new UInt16DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(int))
            {
                ref var specificKeys = ref Unsafe.As<TKey, int>(ref keys);
                IntroSort(ref specificKeys, length, new Int32DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(uint))
            {
                ref var specificKeys = ref Unsafe.As<TKey, uint>(ref keys);
                IntroSort(ref specificKeys, length, new UInt32DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(long))
            {
                ref var specificKeys = ref Unsafe.As<TKey, long>(ref keys);
                IntroSort(ref specificKeys, length, new Int64DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(ulong))
            {
                ref var specificKeys = ref Unsafe.As<TKey, ulong>(ref keys);
                IntroSort(ref specificKeys, length, new UInt64DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(float))
            {
                ref var specificKeys = ref Unsafe.As<TKey, float>(ref keys);

                // Comparison to NaN is always false, so do a linear pass 
                // and swap all NaNs to the front of the array
                var left = Common.NaNPrepass(ref specificKeys, length, new SingleIsNaN());

                var remaining = length - left;
                if (remaining > 1)
                {
                    ref var afterNaNsKeys = ref Unsafe.Add(ref specificKeys, left);
                    IntroSort(ref afterNaNsKeys, remaining, new SingleDirectComparer());
                }
                return true;
            }
            else if (typeof(TKey) == typeof(double))
            {
                ref var specificKeys = ref Unsafe.As<TKey, double>(ref keys);

                // Comparison to NaN is always false, so do a linear pass 
                // and swap all NaNs to the front of the array
                var left = Common.NaNPrepass(ref specificKeys, length, new DoubleIsNaN());
                var remaining = length - left;
                if (remaining > 1)
                {
                    ref var afterNaNsKeys = ref Unsafe.Add(ref specificKeys, left);
                    IntroSort(ref afterNaNsKeys, remaining, new DoubleDirectComparer());
                }
                return true;
            }
            // TODO: Specialize for string if necessary. What about the == null checks?
            else if (typeof(TKey) == typeof(string))
            {
                ref var specificKeys = ref Unsafe.As<TKey, string>(ref keys);
                var comparer = StringDirectComparer.CreateForCurrentCulture();
                IntroSort(ref specificKeys, length, comparer);
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
