// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

using S = System.SpanSortHelpersKeysValues;
using SC = DotNetCross.Sorting.KeysValuesSorter_Comparable;
//using SIC = DotNetCross.Sorting.IComparableImpl;
using STC = DotNetCross.Sorting.KeysValuesSorter_TComparer;
using SDC = System.SpanSortHelpersKeysValues_DirectComparer;
using DotNetCross.Sorting;

namespace System
{
    internal static partial class SpanSortHelpersKeysValues
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void IntroSort<TKey, TValue>(Span<TKey> keys, Span<TValue> values)
        {
            int length = keys.Length;
            if (length != values.Length)
                ThrowHelper.ThrowArgumentException_ItemsMustHaveSameLength();
            if (length < 2)
                return;

            // PERF: Try specialized here for optimal performance
            // Code-gen is weird unless used in loop outside
            if (!SDC.TrySortSpecialized(
                ref MemoryMarshal.GetReference(keys),
                ref MemoryMarshal.GetReference(values),
                length))
            {
                IntroKeysValuesSorters.Default<TKey, TValue>.Instance.IntroSort(
                    ref MemoryMarshal.GetReference(keys),
                    ref MemoryMarshal.GetReference(values),
                    length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void IntroSort<TKey, TValue, TComparer>(
            Span<TKey> keys, Span<TValue> values, TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            int length = keys.Length;
            if (length != values.Length)
                ThrowHelper.ThrowArgumentException_ItemsMustHaveSameLength();
            if (length < 2)
                return;

            IntroKeysValuesSorters.Default<TKey, TValue, TComparer>.Instance.Sort(
                ref MemoryMarshal.GetReference(keys),
                ref MemoryMarshal.GetReference(values),
                length, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void IntroSort<TKey, TValue>(
            Span<TKey> keys, Span<TValue> values, Comparison<TKey> comparison)
        {
            int length = keys.Length;
            if (length != values.Length)
                ThrowHelper.ThrowArgumentException_ItemsMustHaveSameLength();
            if (length < 2)
                return;

            IntroKeysValuesSorters.Default<TKey, TValue>.Instance.IntroSort(
                ref MemoryMarshal.GetReference(keys),
                ref MemoryMarshal.GetReference(values),
                length, comparison);
        }

    }
}
