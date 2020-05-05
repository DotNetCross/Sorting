using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SDC = System.SpanSortHelpersKeys_DirectComparer;

namespace DotNetCross.Sorting
{
    internal static partial class IntroKeysSorters
    {
        static readonly object[] EmptyObjects = new object[0];

        internal static class Default<TKey>
        {
            internal static readonly IKeysSorter<TKey> Instance = CreateSorter();

            private static IKeysSorter<TKey> CreateSorter()
            {
                if (IComparableTraits<TKey>.IsComparable)
                {
                    // coreclr uses RuntimeTypeHandle.Allocate
                    var ctor = typeof(KeysSorter_Comparable<>)
                        .MakeGenericType(new Type[] { typeof(TKey) })
                        .GetTypeInfo().DeclaredConstructors.Where(ci => !ci.IsStatic).Single();

                    return (IKeysSorter<TKey>)ctor.Invoke(EmptyObjects);
                }
                else
                {
                    return new NonComparable<TKey>();
                }
            }
        }

        internal sealed class NonComparable<TKey> : IKeysSorter<TKey>
        {
            internal static readonly KeysSorter_Comparison<TKey> ComparisonInstance = new KeysSorter_Comparison<TKey>();
            internal static readonly KeysSorter_TComparer<TKey, Comparer<TKey>> ComparerInstance = new KeysSorter_TComparer<TKey, Comparer<TKey>>();

            public void IntroSort(ref TKey keys, int length)
            {
                // TODO: Cache Comparer<TKey>.Default.Compare as Comparison<TKey> since faster
                ComparerInstance.IntroSort(ref keys, length, Comparer<TKey>.Default);
            }

            public void IntroSort(ref TKey keys, int length, Comparison<TKey> comparison)
            {
                ComparisonInstance.IntroSort(ref keys, length, comparison);
            }
        }


        internal static class Default<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            internal static readonly IComparerKeysSorter<TKey, TComparer> Instance = CreateSorter();

            private static IComparerKeysSorter<TKey, TComparer> CreateSorter()
            {
                if (IComparableTraits<TKey>.IsComparable)
                {
                    // coreclr uses RuntimeTypeHandle.Allocate
                    var ctor = typeof(Comparable<,>)
                        .MakeGenericType(new Type[] { typeof(TKey), typeof(TComparer) })
                        .GetTypeInfo().DeclaredConstructors.Where(ci => !ci.IsStatic).Single();

                    return (IComparerKeysSorter<TKey, TComparer>)ctor.Invoke(EmptyObjects);
                }
                else
                {
                    return new NonComparable<TKey, TComparer>();
                }
            }
        }

        internal sealed class NonComparable<TKey, TComparer> : IComparerKeysSorter<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            internal static readonly KeysSorter_TComparer<TKey, TComparer> ComparerInstance = new KeysSorter_TComparer<TKey, TComparer>();
            internal static readonly KeysSorter_TComparer<TKey, Comparer<TKey>> DefaultComparerInstance = new KeysSorter_TComparer<TKey, Comparer<TKey>>();

            public void IntroSort(ref TKey keys, int length, TComparer comparer)
            {
                // Add a try block here to detect IComparers (or their
                // underlying IComparables, etc) that are bogus.
                //
                // TODO: Do we need the try/catch?
                //try
                //{
                if (typeof(TComparer) == typeof(IComparer<TKey>) && comparer == null)
                {
                    DefaultComparerInstance.IntroSort(ref keys, length, Comparer<TKey>.Default);
                }
                else
                {
                    ComparerInstance.IntroSort(ref keys, length, comparer);
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

        internal sealed class Comparable<TKey, TComparer>
            : IComparerKeysSorter<TKey, TComparer>
            where TKey : IComparable<TKey>
            where TComparer : IComparer<TKey>
        {
            internal static readonly KeysSorter_Comparable<TKey> NonComparerInstance = new KeysSorter_Comparable<TKey>();
            internal static readonly NonComparable<TKey, TComparer> NonComparableInstance = new NonComparable<TKey, TComparer>();

            public void IntroSort(ref TKey keys, int length,
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
                        //IComparableImpl.IntroSort(ref keys, length);
                        NonComparerInstance.IntroSort(ref keys, length);
                    }
                }
                else
                {
                    NonComparableInstance.IntroSort(ref keys, length, comparer);
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