// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

using DotNetCross.Sorting;
using static DotNetCross.Sorting.TDirectComparerImpl;
using static DotNetCross.Sorting.Swapper;

namespace System
{
    internal static partial class SpanSortHelpersKeysValues_DirectComparer
    {
        // https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySortSpecialized<TKey, TValue>(
            ref TKey keys, ref TValue values, int length)
        {
            //if (typeof(TKey) == typeof(string))
            //{
            //    ref var specificKeys = ref Unsafe.As<TKey, string>(ref keys);
            //    IntroSort(ref specificKeys, ref values, length, StringDirectComparer.CreateForCurrentCulture());
            //    return true;
            //}
            // TODO: For now we do not optimize if value not same type as key 
            //       since this changes output a tiny bit.
            if (typeof(TKey) != typeof(TValue))
            {
                return false;
            }
            else
            // Types unfolding adopted from https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp#L268
            if (typeof(TKey) == typeof(sbyte))
            {
                ref var specificKeys = ref Unsafe.As<TKey, sbyte>(ref keys);
                IntroSort(ref specificKeys, ref values, length, new SByteDirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(byte) ||
                     typeof(TKey) == typeof(bool)) // Use byte for bools to reduce code size
            {
                ref var specificKeys = ref Unsafe.As<TKey, byte>(ref keys);
                IntroSort(ref specificKeys, ref values, length, new ByteDirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(short))
            {
                ref var specificKeys = ref Unsafe.As<TKey, short>(ref keys);
                IntroSort(ref specificKeys, ref values, length, new Int16DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(ushort) ||
                     typeof(TKey) == typeof(char)) // Use ushort for chars to reduce code size)
            {
                ref var specificKeys = ref Unsafe.As<TKey, ushort>(ref keys);
                IntroSort(ref specificKeys, ref values, length, new UInt16DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(int))
            {
                ref var specificKeys = ref Unsafe.As<TKey, int>(ref keys);
                IntroSort(ref specificKeys, ref values, length, new Int32DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(uint))
            {
                ref var specificKeys = ref Unsafe.As<TKey, uint>(ref keys);
                IntroSort(ref specificKeys, ref values, length, new UInt32DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(long))
            {
                ref var specificKeys = ref Unsafe.As<TKey, long>(ref keys);
                IntroSort(ref specificKeys, ref values, length, new Int64DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(ulong))
            {
                ref var specificKeys = ref Unsafe.As<TKey, ulong>(ref keys);
                IntroSort(ref specificKeys, ref values, length, new UInt64DirectComparer());
                return true;
            }
            // Array.Sort only uses NaNPrepass when both key and value are the same,
            // to give exactly the same result we have to do the same.
            // Not only that, the comparisons will then be different since when using normal sort
            // code the IComparable<> path is different than the specialized.
            else if (typeof(TKey) == typeof(float) && typeof(TValue) == typeof(float))
            {
                ref var specificKeys = ref Unsafe.As<TKey, float>(ref keys);
                // Array.Sort only uses NaNPrepass when both key and value are the same
                //if (typeof(TValue) == typeof(float))
                {
                    // Comparison to NaN is always false, so do a linear pass 
                    // and swap all NaNs to the front of the array
                    var left = Common.NaNPrepass(ref specificKeys, ref values, length, new SingleIsNaN());

                    var remaining = length - left;
                    if (remaining > 1)
                    {
                        ref var afterNaNsKeys = ref Unsafe.Add(ref specificKeys, left);
                        ref var afterNaNsValues = ref Unsafe.Add(ref values, left);
                        IntroSort(ref afterNaNsKeys, ref afterNaNsValues, remaining, new SingleDirectComparer());
                    }
                }
                //else
                //{
                //    Sort(ref specificKeys, ref values, length, new SingleDirectComparer());
                //}
                return true;
            }
            // Array.Sort only uses NaNPrepass when both key and value are the same,
            // to give exactly the same result we have to do the same.
            // Not only that, the comparisons will then be different since when using normal sort
            // code the IComparable<> path is different than the specialized.
            else if (typeof(TKey) == typeof(double) && typeof(TValue) == typeof(double))
            {
                ref var specificKeys = ref Unsafe.As<TKey, double>(ref keys);
                // Array.Sort only uses NaNPrepass when both key and value are the same
                //if (typeof(TValue) == typeof(double))
                {
                    // Comparison to NaN is always false, so do a linear pass 
                    // and swap all NaNs to the front of the array
                    var left = Common.NaNPrepass(ref specificKeys, ref values, length, new DoubleIsNaN());

                    var remaining = length - left;
                    if (remaining > 1)
                    {
                        ref var afterNaNsKeys = ref Unsafe.Add(ref specificKeys, left);
                        ref var afterNaNsValues = ref Unsafe.Add(ref values, left);
                        IntroSort(ref afterNaNsKeys, ref afterNaNsValues, remaining, new DoubleDirectComparer());
                    }
                }
                //else
                //{
                //    Sort(ref specificKeys, ref values, length, new DoubleDirectComparer());
                //}
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
