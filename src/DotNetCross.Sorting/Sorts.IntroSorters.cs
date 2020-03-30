using System;
using System.Collections.Generic;
using System.Reflection;
using SDC = System.SpanSortHelpersKeys_DirectComparer;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        internal static partial class IntroSorters
        {
            internal static class Default<TKey>
            {
                internal static readonly ISorter<TKey> s_default = CreateSortHelper();

                private static ISorter<TKey> CreateSortHelper()
                {
                    if (typeof(IComparable<TKey>).GetTypeInfo().IsAssignableFrom(typeof(TKey)))
                    {
                        // coreclr uses RuntimeTypeHandle.Allocate
                        var ctor = typeof(Comparable<>)
                            .MakeGenericType(new Type[] { typeof(TKey) })
                            .GetTypeInfo()
                            .GetConstructor(Array.Empty<Type>());

                        return (ISorter<TKey>)ctor.Invoke(Array.Empty<object>());
                    }
                    else
                    {
                        return new NonComparable<TKey>();
                    }
                }
            }

            internal class NonComparable<TKey> : ISorter<TKey>
            {
                public void Sort(ref TKey keys, int length)
                {
                    TComparerImpl.IntroSort(ref keys, length, Comparer<TKey>.Default);
                }

                public void Sort(ref TKey keys, int length, Comparison<TKey> comparison)
                {
                    ComparisonImpl.IntroSort(ref keys, length, comparison);
                }
            }

            internal class Comparable<TKey>
                : ISorter<TKey>
                where TKey : IComparable<TKey>
            {
                public void Sort(ref TKey keys, int length)
                {
                    IComparableImpl.IntroSort(ref keys, length);
                }

                public void Sort(ref TKey keys, int length, Comparison<TKey> comparison)
                {
                    // TODO: Check if comparison is Comparer<TKey>.Default.Compare

                    ComparisonImpl.IntroSort(ref keys, length, comparison);
                }
            }

            internal static class Default<TKey, TComparer>
                where TComparer : IComparer<TKey>
            {
                internal static readonly ISorter<TKey, TComparer> s_default = CreateSortHelper();

                private static ISorter<TKey, TComparer> CreateSortHelper()
                {
                    if (typeof(IComparable<TKey>).GetTypeInfo().IsAssignableFrom(typeof(TKey)))
                    {
                        // coreclr uses RuntimeTypeHandle.Allocate
                        var ctor = typeof(Comparable<,>)
                            .MakeGenericType(new Type[] { typeof(TKey), typeof(TComparer) })
                            .GetTypeInfo()
                            .GetConstructor(Array.Empty<Type>());

                        return (ISorter<TKey, TComparer>)ctor.Invoke(Array.Empty<object>());
                    }
                    else
                    {
                        return new NonComparable<TKey, TComparer>();
                    }
                }
            }

            internal class NonComparable<TKey, TComparer> : ISorter<TKey, TComparer>
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
                        TComparerImpl.IntroSort(ref keys, length, Comparer<TKey>.Default);
                    }
                    else
                    {
                        TComparerImpl.IntroSort(ref keys, length, comparer);
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

            internal class Comparable<TKey, TComparer>
                : ISorter<TKey, TComparer>
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
                        (!typeof(TComparer).GetTypeInfo().IsValueType &&
                         object.ReferenceEquals(comparer, Comparer<TKey>.Default)))
                    {
                        if (!SDC.TrySortSpecialized(ref keys, length))
                        {
                            // NOTE: For Bogus Comparable the exception message will be different, when using Comparer<TKey>.Default
                            //       Since the exception message is thrown internally without knowledge of the comparer
                            IComparableImpl.IntroSort(ref keys, length);
                        }
                    }
                    else
                    {
                        TComparerImpl.IntroSort(ref keys, length, comparer);
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
}
