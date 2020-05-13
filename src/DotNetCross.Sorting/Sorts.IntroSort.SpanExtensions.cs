using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Specialized for either only keys or keys and values and for comparable or not
using KSDC = DotNetCross.Sorting.KeysSorter_TDirectComparer;
using KVSDC = DotNetCross.Sorting.KeysValuesSorter_TDirectComparer;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        /// <summary>
        /// Sorts the elements in the entire <see cref="Span{T}" /> 
        /// using the <see cref="IComparableImpl" /> implementation of each 
        /// element of the <see cref= "Span{T}" />
        /// </summary>
        /// <param name="keys">The <see cref="Span{T}"/> to sort.</param>
        /// <exception cref = "InvalidOperationException"> 
        /// One or more elements do not implement the <see cref="IComparableImpl" /> interface.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntroSort<T>(this Span<T> keys)
        {
            int length = keys.Length;
            if (length < 2)
                return;

            // PERF: Try specialized here for optimal performance
            // Code-gen is weird unless used in loop outside
            if (!KSDC.TrySortSpecialized(
                ref MemoryMarshal.GetReference(keys),
                length))
            {
                IntroKeysSorters.ForStraight<T>.Instance(
                    ref MemoryMarshal.GetReference(keys),
                    length);
            }
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="Span{T}" /> 
        /// using the <typeparamref name="TComparer" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntroSort<T, TComparer>(this Span<T> keys, TComparer comparer)
           where TComparer : IComparer<T>
        {
            if (TypeTraits<T>.IsComparerNullOrDefault(comparer))
            {
                IntroSort(keys);
            }
            else
            {
                int length = keys.Length;
                if (length < 2)
                    return;

                IntroKeysSorters.ForComparer<T, TComparer>.Instance(
                    ref MemoryMarshal.GetReference(keys),
                    length, comparer);
            }
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="Span{T}" /> 
        /// using the <see cref="Comparison{T}" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntroSort<T>(this Span<T> keys, Comparison<T> comparison)
        {
            if (comparison == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);

            int length = keys.Length;
            if (length < 2)
                return;

            IntroKeysSorters.ForComparison<T>.Instance(
                ref MemoryMarshal.GetReference(keys),
                length, comparison);
        }

        /// <summary>
        /// Sorts a pair of spans 
        /// (one contains the keys <see cref="Span{TKey}"/> 
        /// and the other contains the corresponding values <see cref="Span{TValue}"/>) 
        /// based on the keys in the first <see cref= "Span{TKey}" /> 
        /// using the <see cref="IComparableImpl" /> implementation of each 
        /// element of the <see cref= "Span{TKey}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntroSort<TKey, TValue>(this Span<TKey> keys, Span<TValue> values)
        {
            int length = keys.Length;
            if (length != values.Length)
                ThrowHelper.ThrowArgumentException_ItemsMustHaveSameLength();
            if (length < 2)
                return;

            // PERF: Try specialized here for optimal performance
            // Code-gen is weird unless used in loop outside
            if (!KVSDC.TrySortSpecialized(
                ref MemoryMarshal.GetReference(keys),
                ref MemoryMarshal.GetReference(values),
                length))
            {
                IntroKeysValuesSorters.ForStraight<TKey, TValue>.Instance(
                    ref MemoryMarshal.GetReference(keys),
                    ref MemoryMarshal.GetReference(values),
                    length);
            }
        }

        /// <summary>
        /// Sorts a pair of spans 
        /// (one contains the keys <see cref="Span{TKey}"/> 
        /// and the other contains the corresponding values <see cref="Span{TValue}"/>) 
        /// based on the keys in the first <see cref= "Span{TKey}" /> 
        /// using the <typeparamref name="TComparer" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntroSort<TKey, TValue, TComparer>(this Span<TKey> keys,
           Span<TValue> values, TComparer comparer)
           where TComparer : IComparer<TKey>
        {
            if (TypeTraits<TKey>.IsComparerNullOrDefault(comparer))
            {
                IntroSort(keys, values);
            }
            else
            {
                int length = keys.Length;
                if (length != values.Length)
                    ThrowHelper.ThrowArgumentException_ItemsMustHaveSameLength();
                if (length < 2)
                    return;

                IntroKeysValuesSorters.ForComparer<TKey, TValue, TComparer>.Instance(
                    ref MemoryMarshal.GetReference(keys),
                    ref MemoryMarshal.GetReference(values),
                    length, comparer);
            }
        }

        /// <summary>
        /// Sorts a pair of spans 
        /// (one contains the keys <see cref="Span{TKey}"/> 
        /// and the other contains the corresponding values <see cref="Span{TValue}"/>) 
        /// based on the keys in the first <see cref= "Span{TKey}" /> 
        /// using the <see cref="Comparison{TKey}" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntroSort<TKey, TValue>(this Span<TKey> keys,
           Span<TValue> values, Comparison<TKey> comparison)
        {
            if (comparison == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);

            int length = keys.Length;
            if (length != values.Length)
                ThrowHelper.ThrowArgumentException_ItemsMustHaveSameLength();
            if (length < 2)
                return;

            IntroKeysValuesSorters.ForComparison<TKey, TValue>.Instance(
                ref MemoryMarshal.GetReference(keys),
                ref MemoryMarshal.GetReference(values),
                length, comparison);
        }
    }
}
