using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SC = DotNetCross.Sorting.ComparisonImpl;
using SIC = DotNetCross.Sorting.IComparableImpl;
using STC = DotNetCross.Sorting.TComparerImpl;
using SDC = System.SpanSortHelpersKeysValues_DirectComparer;

namespace DotNetCross.Sorting
{
    internal static partial class IntroKeysValuesSorters
    {
        static readonly object[] EmptyObjects = new object[0];

        internal static class Default<TKey, TValue>
        {
            internal static readonly IKeysValuesSorter<TKey, TValue> s_default = CreateSorter();

            private static IKeysValuesSorter<TKey, TValue> CreateSorter()
            {
                if (IComparableTraits<TKey>.IsComparable)
                {
                    // coreclr uses RuntimeTypeHandle.Allocate
                    var ctor = typeof(Comparable<,>)
                        .MakeGenericType(new Type[] { typeof(TKey), typeof(TValue) })
                        .GetTypeInfo().DeclaredConstructors.Single();

                    return (IKeysValuesSorter<TKey, TValue>)ctor.Invoke(EmptyObjects);
                }
                else
                {
                    return new NonComparable<TKey, TValue>();
                }
            }
        }

        internal class NonComparable<TKey, TValue>
            : IKeysValuesSorter<TKey, TValue>
        {
            public void Sort(ref TKey keys, ref TValue values, int length)
            {
                STC.IntroSort(ref keys, ref values, length, Comparer<TKey>.Default);
            }

            public void Sort(ref TKey keys, ref TValue values, int length, Comparison<TKey> comparison)
            {
                SC.IntroSort(ref keys, ref values, length, comparison);
            }
        }

        internal class Comparable<TKey, TValue>
            : IKeysValuesSorter<TKey, TValue>
            where TKey : IComparable<TKey>
        {
            public void Sort(ref TKey keys, ref TValue values, int length)
            {
                SIC.IntroSort(ref keys, ref values, length);
            }

            public void Sort(ref TKey keys, ref TValue values, int length, Comparison<TKey> comparison)
            {
                // TODO: Check if comparison is Comparer<TKey>.Default.Compare

                SC.IntroSort(ref keys, ref values, length, comparison);
            }
        }

        internal static class Default<TKey, TValue, TComparer>
            where TComparer : IComparer<TKey>
        {
            internal static readonly IKeysValuesSorter<TKey, TValue, TComparer> s_default = CreateSortHelper();

            private static IKeysValuesSorter<TKey, TValue, TComparer> CreateSortHelper()
            {
                if (IComparableTraits<TKey>.IsComparable)
                {
                    // coreclr uses RuntimeTypeHandle.Allocate
                    var ctor = typeof(Comparable<,,>)
                        .MakeGenericType(new Type[] { typeof(TKey), typeof(TValue), typeof(TComparer) })
                        .GetTypeInfo().DeclaredConstructors.Single();

                    return (IKeysValuesSorter<TKey, TValue, TComparer>)ctor.Invoke(EmptyObjects);
                }
                else
                {
                    return new NonComparable<TKey, TValue, TComparer>();
                }
            }
        }

        internal class NonComparable<TKey, TValue, TComparer>
            : IKeysValuesSorter<TKey, TValue, TComparer>
            where TComparer : IComparer<TKey>
        {
            public void Sort(ref TKey keys, ref TValue values, int length, TComparer comparer)
            {
                // Add a try block here to detect IComparers (or their
                // underlying IComparables, etc) that are bogus.
                //
                // TODO: Do we need the try/catch?
                //try
                //{
                if (typeof(TComparer) == typeof(IComparer<TKey>) && comparer == null)
                {
                    STC.IntroSort(ref keys, ref values, length, Comparer<TKey>.Default);
                }
                else
                {
                    STC.IntroSort(ref keys, ref values, length, comparer);
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

        internal class Comparable<TKey, TValue, TComparer>
            : IKeysValuesSorter<TKey, TValue, TComparer>
            where TKey : IComparable<TKey>
            where TComparer : IComparer<TKey>
        {
            public void Sort(ref TKey keys, ref TValue values, int length,
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
                    (!ValueTraits<TComparer>.IsValueType &&
                     ReferenceEquals(comparer, Comparer<TKey>.Default))) // Or "=="?
                {
                    if (!SDC.TrySortSpecialized(ref keys, ref values, length))
                    {
                        // NOTE: For Bogus Comparable the exception message will be different, when using Comparer<TKey>.Default
                        //       Since the exception message is thrown internally without knowledge of the comparer
                        SIC.IntroSort(ref keys, ref values, length);
                    }
                }
                else
                {
                    STC.IntroSort(ref keys, ref values, length, comparer);
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