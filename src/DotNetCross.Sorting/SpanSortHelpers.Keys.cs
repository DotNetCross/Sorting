﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

using static System.SpanSortHelpersCommon;
using S = System.SpanSortHelpersKeys;

namespace System
{
    internal static partial class SpanSortHelpersKeys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<TKey>(this Span<TKey> keys)
        {
            int length = keys.Length;
            if (length < 2)
                return;

            // PERF: Try specialized here for optimal performance
            // Code-gen is weird unless used in loop outside
            if (!TrySortSpecialized(
                ref keys.DangerousGetPinnableReference(), length))
            {
                DefaultSpanSortHelper<TKey>.s_default.Sort(
                    ref keys.DangerousGetPinnableReference(),
                    length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<TKey, TComparer>(
            this Span<TKey> keys, TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            int length = keys.Length;
            if (length < 2)
                return;

            DefaultSpanSortHelper<TKey, TComparer>.s_default.Sort(
                ref keys.DangerousGetPinnableReference(),
                keys.Length, comparer);
        }

        internal static class DefaultSpanSortHelper<TKey>
        {
            internal static readonly ISpanSortHelper<TKey> s_default = CreateSortHelper();

            private static ISpanSortHelper<TKey> CreateSortHelper()
            {
                if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
                {
                    if (typeof(TKey).IsValueType)
                    {
                        // TODO: Is there a faster way? A way without heap alloc? 
                        // Albeit, this only happens once for each type combination
                        var ctor = typeof(ComparableSpanSortHelper<>)
                        .MakeGenericType(new Type[] { typeof(TKey) })
                        .GetConstructor(Array.Empty<Type>());

                        return (ISpanSortHelper<TKey>)ctor.Invoke(Array.Empty<object>());
                        // coreclr does the following:
                        //return (IArraySortHelper<T, TComparer>)
                        //    RuntimeTypeHandle.Allocate(
                        //        .TypeHandle.Instantiate());
                    }
                    else
                    {
                        // TODO: Is there a faster way? A way without heap alloc? 
                        // Albeit, this only happens once for each type combination
                        var ctor = typeof(IComparableSpanSortHelper<>)
                        .MakeGenericType(new Type[] { typeof(TKey) })
                        .GetConstructor(Array.Empty<Type>());

                        return (ISpanSortHelper<TKey>)ctor.Invoke(Array.Empty<object>());
                    }
                }
                else
                {
                    return new SpanSortHelper<TKey>();
                }
            }
        }

        internal interface ISpanSortHelper<TKey>
        {
            void Sort(ref TKey keys, int length);
        }

        internal class SpanSortHelper<TKey> : ISpanSortHelper<TKey>
        {
            public void Sort(ref TKey keys, int length)
            {
                S.Sort(ref keys, length,
                    new ComparerLessThanComparer<TKey, IComparer<TKey>>(Comparer<TKey>.Default));
            }
        }

        internal class ComparableSpanSortHelper<TKey>
            : ISpanSortHelper<TKey>
            where TKey : struct, IComparable<TKey>
        {
            public void Sort(ref TKey keys, int length)
            {
                S.Sort(ref keys, length, new ComparableLessThanComparer<TKey>());
            }
        }

        internal class IComparableSpanSortHelper<TKey>
            : ISpanSortHelper<TKey>
            where TKey : class, IComparable<TKey>
        {
            public void Sort(ref TKey keys, int length)
            {
                S.Sort<IComparable<TKey>, IComparableLessThanComparer<TKey>>(
                    ref Unsafe.As<TKey, IComparable<TKey>>(ref keys), length, 
                    new IComparableLessThanComparer<TKey>());
            }
        }


        internal static class DefaultSpanSortHelper<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            //private static volatile ISpanSortHelper<T, TComparer> defaultArraySortHelper;
            //public static ISpanSortHelper<T, TComparer> Default
            //{
            //    get
            //    {
            //        ISpanSortHelper<T, TComparer> sorter = defaultArraySortHelper;
            //        if (sorter == null)
            //            sorter = CreateArraySortHelper();
            //        return sorter;
            //    }
            //}
            internal static readonly ISpanSortHelper<TKey, TComparer> s_default = CreateSortHelper();

            private static ISpanSortHelper<TKey, TComparer> CreateSortHelper()
            {
                if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
                {
                    if (typeof(TKey).IsValueType)
                    {
                        // TODO: Is there a faster way? A way without heap alloc? 
                        // Albeit, this only happens once for each type combination
                        var ctor = typeof(ComparableSpanSortHelper<,>)
                            .MakeGenericType(new Type[] { typeof(TKey), typeof(TComparer) })
                            .GetConstructor(Array.Empty<Type>());

                        return (ISpanSortHelper<TKey, TComparer>)ctor.Invoke(Array.Empty<object>());
                        // coreclr does the following:
                        //return (IArraySortHelper<T, TComparer>)
                        //    RuntimeTypeHandle.Allocate(
                        //        .TypeHandle.Instantiate());
                    }
                    else
                    {
                        // TODO: Is there a faster way? A way without heap alloc? 
                        // Albeit, this only happens once for each type combination
                        var ctor = typeof(IComparableSpanSortHelper<,>)
                            .MakeGenericType(new Type[] { typeof(TKey), typeof(TComparer) })
                            .GetConstructor(Array.Empty<Type>());

                        return (ISpanSortHelper<TKey, TComparer>)ctor.Invoke(Array.Empty<object>());
                    }
                }
                else
                {
                    return new SpanSortHelper<TKey, TComparer>();
                }
            }
        }

        // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
        internal interface ISpanSortHelper<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            void Sort(ref TKey keys, int length, TComparer comparer);
        }

        internal class SpanSortHelper<TKey, TComparer> : ISpanSortHelper<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            public void Sort(ref TKey keys, int length, TComparer comparer)
            {
                // Add a try block here to detect IComparers (or their
                // underlying IComparables, etc) that are bogus.
                //
                // TODO: Do we need the try/catch?
                //try
                //{
                if (typeof(TComparer) == typeof(IComparer<TKey>) && comparer == null)
                {
                    S.Sort(ref keys, length,
                        new ComparerLessThanComparer<TKey, IComparer<TKey>>(Comparer<TKey>.Default));
                }
                else
                {
                    S.Sort(ref keys, length,
                        new ComparerLessThanComparer<TKey, IComparer<TKey>>(comparer));
                }
                //}
                //catch (IndexOutOfRangeException e)
                //{
                //    throw e;
                //    //IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
                //}
                //catch (Exception e)
                //{
                //    throw e;
                //    //throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
                //}
            }
        }

        internal class ComparableSpanSortHelper<TKey, TComparer>
            : ISpanSortHelper<TKey, TComparer>
            where TKey : IComparable<TKey>
            where TComparer : IComparer<TKey>
        {
            public void Sort(ref TKey keys, int length,
                TComparer comparer)
            {
                // Add a try block here to detect IComparers (or their
                // underlying IComparables, etc) that are bogus.
                //
                // TODO: Do we need the try/catch?
                //try
                //{
                if (comparer == null ||
                    // Cache this in generic traits helper class perhaps
                    (!typeof(TComparer).IsValueType &&
                     object.ReferenceEquals(comparer, Comparer<TKey>.Default))) // Or "=="?
                {
                    if (!S.TrySortSpecialized(ref keys, length))
                    {
                        S.Sort(ref keys, length,
                            new ComparableLessThanComparer<TKey>());
                    }
                }
                else
                {
                    S.Sort(ref keys, length,
                        new ComparerLessThanComparer<TKey, TComparer>(comparer));
                }
                //}
                //catch (IndexOutOfRangeException e)
                //{
                //    throw e;
                //    //IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
                //}
                //catch (Exception e)
                //{
                //    throw e;
                //    //throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
                //}
            }
        }
        internal class IComparableSpanSortHelper<TKey, TComparer>
            : ISpanSortHelper<TKey, TComparer>
            where TKey : class, IComparable<TKey>
            where TComparer : IComparer<TKey>
        {
            public void Sort(ref TKey keys, int length,
                TComparer comparer)
            {
                // Add a try block here to detect IComparers (or their
                // underlying IComparables, etc) that are bogus.
                //
                // TODO: Do we need the try/catch?
                //try
                //{
                if (comparer == null ||
                    // Cache this in generic traits helper class perhaps
                    (!typeof(TComparer).IsValueType &&
                     object.ReferenceEquals(comparer, Comparer<TKey>.Default))) // Or "=="?
                {
                    if (!S.TrySortSpecialized(ref keys, length))
                    {
                        if (typeof(TKey).IsValueType)
                        {
                            S.Sort(ref keys, length, new ComparableLessThanComparer<TKey>());
                        }
                        else
                        {
                            S.Sort(ref Unsafe.As<TKey, IComparable<TKey>>(ref keys), length,
                                new IComparableLessThanComparer<TKey>());
                        }
                    }
                }
                else
                {
                    S.Sort(ref keys, length,
                        new ComparerLessThanComparer<TKey, TComparer>(comparer));
                }
                //}
                //catch (IndexOutOfRangeException e)
                //{
                //    throw e;
                //    //IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
                //}
                //catch (Exception e)
                //{
                //    throw e;
                //    //throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
                //}
            }
        }
    }
}
