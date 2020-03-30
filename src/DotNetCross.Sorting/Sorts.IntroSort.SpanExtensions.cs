using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// Consolidated code
//using SHK = System.SpanSortHelpersKeysAndOrValues;
//using SHKV = System.SpanSortHelpersKeysAndOrValues;
// Specialized for either only keys or keys and values and for comparable or not
using SHK = DotNetCross.Sorting.Sorts.Keys;
using SHKV = System.SpanSortHelpersKeysValues;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        /// <summary>
        /// Sorts the elements in the entire <see cref="Span{T}" /> 
        /// using the <see cref="IComparableImpl" /> implementation of each 
        /// element of the <see cref= "Span{T}" />
        /// </summary>
        /// <param name="span">The <see cref="Span{T}"/> to sort.</param>
        /// <exception cref = "InvalidOperationException"> 
        /// One or more elements do not implement the <see cref="IComparableImpl" /> interface.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntroSort<T>(this Span<T> span)
        {
            SHK.IntroSort(span);
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="Span{T}" /> 
        /// using the <typeparamref name="TComparer" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntroSort<T, TComparer>(this Span<T> span, TComparer comparer)
           where TComparer : IComparer<T>
        {
            SHK.IntroSort(span, comparer);
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="Span{T}" /> 
        /// using the <see cref="Comparison{T}" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntroSort<T>(this Span<T> span, Comparison<T> comparison)
        {
            if (comparison == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);

            SHK.IntroSort(span, comparison);
        }

        /// <summary>
        /// Sorts a pair of spans 
        /// (one contains the keys <see cref="Span{TKey}"/> 
        /// and the other contains the corresponding items <see cref="Span{TValue}"/>) 
        /// based on the keys in the first <see cref= "Span{TKey}" /> 
        /// using the <see cref="IComparableImpl" /> implementation of each 
        /// element of the <see cref= "Span{TKey}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntroSort<TKey, TValue>(this Span<TKey> keys, Span<TValue> items)
        {
            SHKV.IntroSort(keys, items);
        }

        /// <summary>
        /// Sorts a pair of spans 
        /// (one contains the keys <see cref="Span{TKey}"/> 
        /// and the other contains the corresponding items <see cref="Span{TValue}"/>) 
        /// based on the keys in the first <see cref= "Span{TKey}" /> 
        /// using the <typeparamref name="TComparer" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntroSort<TKey, TValue, TComparer>(this Span<TKey> keys,
           Span<TValue> items, TComparer comparer)
           where TComparer : IComparer<TKey>
        {
            SHKV.IntroSort(keys, items, comparer);
        }

        /// <summary>
        /// Sorts a pair of spans 
        /// (one contains the keys <see cref="Span{TKey}"/> 
        /// and the other contains the corresponding items <see cref="Span{TValue}"/>) 
        /// based on the keys in the first <see cref= "Span{TKey}" /> 
        /// using the <see cref="Comparison{TKey}" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntroSort<TKey, TValue>(this Span<TKey> keys,
           Span<TValue> items, Comparison<TKey> comparison)
        {
            if (comparison == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);

            SHKV.IntroSort(keys, items, comparison);
        }
    }
}
