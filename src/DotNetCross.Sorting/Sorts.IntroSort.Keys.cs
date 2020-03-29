// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

using DotNetCross.Sorting;
using static DotNetCross.Sorting.Sorts;
//using S = System.SpanSortHelpersKeys;
//using SC = System.SpanSortHelpersKeys_Comparer;
using SDC = System.SpanSortHelpersKeys_DirectComparer;
using System.Reflection;
using System;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        public static class Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void IntroSort<TKey>(Span<TKey> keys)
            {
                int length = keys.Length;
                if (length < 2)
                    return;

                // PERF: Try specialized here for optimal performance
                // Code-gen is weird unless used in loop outside
                if (!SDC.TrySortSpecialized(
                    ref MemoryMarshal.GetReference(keys),
                    length))
                {
                    IntroSorters.DefaultSpanSortHelper<TKey>.s_default.Sort(
                        ref MemoryMarshal.GetReference(keys),
                        length);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void IntroSort<TKey, TComparer>(
                Span<TKey> keys, TComparer comparer)
                where TComparer : IComparer<TKey>
            {
                int length = keys.Length;
                if (length < 2)
                    return;

                IntroSorters.DefaultSpanSortHelper<TKey, TComparer>.s_default.Sort(
                    ref MemoryMarshal.GetReference(keys),
                    length, comparer);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void IntroSort<TKey>(
                Span<TKey> keys, Comparison<TKey> comparison)
            {
                int length = keys.Length;
                if (length < 2)
                    return;

                IntroSorters.DefaultSpanSortHelper<TKey>.s_default.Sort(
                    ref MemoryMarshal.GetReference(keys),
                    length, comparison);
            }
        }
    }
}
