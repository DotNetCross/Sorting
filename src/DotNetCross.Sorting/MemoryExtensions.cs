// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

namespace System
{
    /// <summary>
    /// Extension methods for Span{T}, Memory{T}, and friends.
    /// </summary>
    public static partial class MemoryExtensions
    {
        //      /// <summary>
        //      /// Searches an entire sorted <see cref="Span{T}"/> for a value
        //      /// using the specified <see cref="IComparable{T}"/> generic interface.
        //      /// </summary>
        //      /// <typeparam name="T">The element type of the span.</typeparam>
        //      /// <param name="span">The sorted <see cref="Span{T}"/> to search.</param>
        //      /// <param name="comparable">The <see cref="IComparable{T}"/> to use when comparing.</param>
        //      /// <returns>
        //      /// The zero-based index of <paramref name="comparable"/> in the sorted <paramref name="span"/>,
        //      /// if <paramref name="comparable"/> is found; otherwise, a negative number that is the bitwise complement
        //      /// of the index of the next element that is larger than <paramref name="comparable"/> or, if there is
        //      /// no larger element, the bitwise complement of <see cref="Span{T}.Length"/>.
        //      /// </returns>
        //      /// <exception cref="T:System.ArgumentNullException">
        /////     <paramref name = "comparable" /> is <see langword="null"/> .
        //      /// </exception>
        //      [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //      public static int BinarySearch<T>(
        //          this Span<T> span, IComparable<T> comparable)
        //      {
        //          return BinarySearch<T, IComparable<T>>(span, comparable);
        //      }

        //      /// <summary>
        //      /// Searches an entire sorted <see cref="Span{T}"/> for a value
        //      /// using the specified <typeparamref name="TComparable"/> generic type.
        //      /// </summary>
        //      /// <typeparam name="T">The element type of the span.</typeparam>
        //      /// <typeparam name="TComparable">The specific type of <see cref="IComparable{T}"/>.</typeparam>
        //      /// <param name="span">The sorted <see cref="Span{T}"/> to search.</param>
        //      /// <param name="comparable">The <typeparamref name="TComparable"/> to use when comparing.</param>
        //      /// <returns>
        //      /// The zero-based index of <paramref name="comparable"/> in the sorted <paramref name="span"/>,
        //      /// if <paramref name="comparable"/> is found; otherwise, a negative number that is the bitwise complement
        //      /// of the index of the next element that is larger than <paramref name="comparable"/> or, if there is
        //      /// no larger element, the bitwise complement of <see cref="Span{T}.Length"/>.
        //      /// </returns>
        //      /// <exception cref="T:System.ArgumentNullException">
        /////     <paramref name = "comparable" /> is <see langword="null"/> .
        //      /// </exception>
        //      [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //      public static int BinarySearch<T, TComparable>(
        //          this Span<T> span, TComparable comparable)
        //          where TComparable : IComparable<T>
        //      {
        //          return BinarySearch((ReadOnlySpan<T>)span, comparable);
        //      }

        //      /// <summary>
        //      /// Searches an entire sorted <see cref="Span{T}"/> for the specified <paramref name="value"/>
        //      /// using the specified <typeparamref name="TComparer"/> generic type.
        //      /// </summary>
        //      /// <typeparam name="T">The element type of the span.</typeparam>
        //      /// <typeparam name="TComparer">The specific type of <see cref="IComparer{T}"/>.</typeparam>
        //      /// <param name="span">The sorted <see cref="Span{T}"/> to search.</param>
        //      /// <param name="value">The object to locate. The value can be null for reference types.</param>
        //      /// <param name="comparer">The <typeparamref name="TComparer"/> to use when comparing.</param>
        //      /// /// <returns>
        //      /// The zero-based index of <paramref name="value"/> in the sorted <paramref name="span"/>,
        //      /// if <paramref name="value"/> is found; otherwise, a negative number that is the bitwise complement
        //      /// of the index of the next element that is larger than <paramref name="value"/> or, if there is
        //      /// no larger element, the bitwise complement of <see cref="Span{T}.Length"/>.
        //      /// </returns>
        //      /// <exception cref="T:System.ArgumentNullException">
        //      ///     <paramref name = "value" /> is <see langword="null"/> .
        //      /// </exception>
        //      // TODO: Do we accept a null comparer and then revert to T as IComparable if possible??
        //      //   T:System.ArgumentException:
        //      //     comparer is null, and value is of a type that is not compatible with the elements
        //      //     of array.
        //      //   T:System.InvalidOperationException:
        //      //     comparer is null, and T does not implement the System.IComparable`1 generic interface
        //      [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //      public static int BinarySearch<T, TComparer>(
        //          this Span<T> span, T value, TComparer comparer)
        //          where TComparer : IComparer<T>
        //      {
        //          return BinarySearch((ReadOnlySpan<T>)span, value, comparer);
        //      }

        //      /// <summary>
        //      /// Searches an entire sorted <see cref="ReadOnlySpan{T}"/> for a value
        //      /// using the specified <see cref="IComparable{T}"/> generic interface.
        //      /// </summary>
        //      /// <typeparam name="T">The element type of the span.</typeparam>
        //      /// <param name="span">The sorted <see cref="ReadOnlySpan{T}"/> to search.</param>
        //      /// <param name="comparable">The <see cref="IComparable{T}"/> to use when comparing.</param>
        //      /// <returns>
        //      /// The zero-based index of <paramref name="comparable"/> in the sorted <paramref name="span"/>,
        //      /// if <paramref name="comparable"/> is found; otherwise, a negative number that is the bitwise complement
        //      /// of the index of the next element that is larger than <paramref name="comparable"/> or, if there is
        //      /// no larger element, the bitwise complement of <see cref="ReadOnlySpan{T}.Length"/>.
        //      /// </returns>
        //      /// <exception cref="T:System.ArgumentNullException">
        /////     <paramref name = "comparable" /> is <see langword="null"/> .
        //      /// </exception>
        //      [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //      public static int BinarySearch<T>(
        //          this ReadOnlySpan<T> span, IComparable<T> comparable)
        //      {
        //          return BinarySearch<T, IComparable<T>>(span, comparable);
        //      }

        //      /// <summary>
        //      /// Searches an entire sorted <see cref="ReadOnlySpan{T}"/> for a value
        //      /// using the specified <typeparamref name="TComparable"/> generic type.
        //      /// </summary>
        //      /// <typeparam name="T">The element type of the span.</typeparam>
        //      /// <typeparam name="TComparable">The specific type of <see cref="IComparable{T}"/>.</typeparam>
        //      /// <param name="span">The sorted <see cref="ReadOnlySpan{T}"/> to search.</param>
        //      /// <param name="comparable">The <typeparamref name="TComparable"/> to use when comparing.</param>
        //      /// <returns>
        //      /// The zero-based index of <paramref name="comparable"/> in the sorted <paramref name="span"/>,
        //      /// if <paramref name="comparable"/> is found; otherwise, a negative number that is the bitwise complement
        //      /// of the index of the next element that is larger than <paramref name="comparable"/> or, if there is
        //      /// no larger element, the bitwise complement of <see cref="ReadOnlySpan{T}.Length"/>.
        //      /// </returns>
        //      /// <exception cref="T:System.ArgumentNullException">
        /////     <paramref name = "comparable" /> is <see langword="null"/> .
        //      /// </exception>
        //      [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //      public static int BinarySearch<T, TComparable>(
        //          this ReadOnlySpan<T> span, TComparable comparable)
        //          where TComparable : IComparable<T>
        //      {
        //          return SpanHelpers.BinarySearch(span, comparable);
        //      }

        //      /// <summary>
        //      /// Searches an entire sorted <see cref="ReadOnlySpan{T}"/> for the specified <paramref name="value"/>
        //      /// using the specified <typeparamref name="TComparer"/> generic type.
        //      /// </summary>
        //      /// <typeparam name="T">The element type of the span.</typeparam>
        //      /// <typeparam name="TComparer">The specific type of <see cref="IComparer{T}"/>.</typeparam>
        //      /// <param name="span">The sorted <see cref="ReadOnlySpan{T}"/> to search.</param>
        //      /// <param name="value">The object to locate. The value can be null for reference types.</param>
        //      /// <param name="comparer">The <typeparamref name="TComparer"/> to use when comparing.</param>
        //      /// /// <returns>
        //      /// The zero-based index of <paramref name="value"/> in the sorted <paramref name="span"/>,
        //      /// if <paramref name="value"/> is found; otherwise, a negative number that is the bitwise complement
        //      /// of the index of the next element that is larger than <paramref name="value"/> or, if there is
        //      /// no larger element, the bitwise complement of <see cref="ReadOnlySpan{T}.Length"/>.
        //      /// </returns>
        //      /// <exception cref="T:System.ArgumentNullException">
        //      ///     <paramref name = "value" /> is <see langword="null"/> .
        //      /// </exception>
        //      [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //      public static int BinarySearch<T, TComparer>(
        //          this ReadOnlySpan<T> span, T value, TComparer comparer)
        //          where TComparer : IComparer<T>
        //      {
        //          if (comparer == null)
        //              ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparer);
        //          // TODO: Do we accept a null comparer and then revert to T as IComparable if possible??
        //          //   T:System.ArgumentException:
        //          //     comparer is null, and value is of a type that is not compatible with the elements
        //          //     of array.
        //          //   T:System.InvalidOperationException:
        //          //     comparer is null, and T does not implement the System.IComparable`1 generic interface
        //          var comparable = new SpanHelpers.ComparerComparable<T, TComparer>(
        //              value, comparer);
        //          return BinarySearch(span, comparable);
        //      }


        /// <summary>
        /// Sorts the elements in the entire <see cref="Span{T}" /> 
        /// using the <see cref="IComparable" /> implementation of each 
        /// element of the <see cref= "Span{T}" />
        /// </summary>
        /// <param name="span">The <see cref="Span{T}"/> to sort.</param>
        /// <exception cref = "InvalidOperationException"> 
        /// One or more elements do not implement the <see cref="IComparable" /> interface.
        /// </exception>
        // TODO: Revise exception list, if we do not try/catch
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this Span<T> span)
        {
            SpanSortHelpers.Sort(span);
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="Span{T}" /> 
        /// using the <typeparamref name="TComparer" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T, TComparer>(this Span<T> span, TComparer comparer)
           where TComparer : IComparer<T>
        {
            SpanSortHelpers.Sort(span, comparer);
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="Span{T}" /> 
        /// using the <see cref="Comparison{T}" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this Span<T> span, Comparison<T> comparison)
        {
            if (comparison == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);

            SpanSortHelpers.Sort(span, new SpanSortHelpers.ComparisonComparer<T>(comparison));
        }

        /// <summary>
        /// Sorts a pair of spans 
        /// (one contains the keys <see cref="Span{TKey}"/> 
        /// and the other contains the corresponding items <see cref="Span{TValue}"/>) 
        /// based on the keys in the first <see cref= "Span{TKey}" /> 
        /// using the <see cref="IComparable" /> implementation of each 
        /// element of the <see cref= "Span{TKey}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<TKey, TValue>(this Span<TKey> keys, Span<TValue> items)
        {
            SpanSortHelpers.Sort(keys, items);
        }

        /// <summary>
        /// Sorts a pair of spans 
        /// (one contains the keys <see cref="Span{TKey}"/> 
        /// and the other contains the corresponding items <see cref="Span{TValue}"/>) 
        /// based on the keys in the first <see cref= "Span{TKey}" /> 
        /// using the <typeparamref name="TComparer" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<TKey, TValue, TComparer>(this Span<TKey> keys,
           Span<TValue> items, TComparer comparer)
           where TComparer : IComparer<TKey>
        {
            SpanSortHelpers.Sort(keys, items, comparer);
        }

        /// <summary>
        /// Sorts a pair of spans 
        /// (one contains the keys <see cref="Span{TKey}"/> 
        /// and the other contains the corresponding items <see cref="Span{TValue}"/>) 
        /// based on the keys in the first <see cref= "Span{TKey}" /> 
        /// using the <see cref="Comparison{TKey}" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<TKey, TValue>(this Span<TKey> keys,
           Span<TValue> items, Comparison<TKey> comparison)
        {
            SpanSortHelpers.Sort(keys, items, new SpanSortHelpers.ComparisonComparer<TKey>(comparison));
        }
    }
}
