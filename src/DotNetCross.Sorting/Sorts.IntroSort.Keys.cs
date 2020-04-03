using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDC = System.SpanSortHelpersKeys_DirectComparer;

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
                    IntroKeysSorters.Default<TKey>.Instance.Sort(
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

                IntroKeysSorters.Default<TKey, TComparer>.Instance.Sort(
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

                IntroKeysSorters.Default<TKey>.Instance.Sort(
                    ref MemoryMarshal.GetReference(keys),
                    length, comparison);
            }
        }
    }
}
