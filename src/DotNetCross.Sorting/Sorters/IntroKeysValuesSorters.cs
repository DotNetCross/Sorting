using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
//using SC = DotNetCross.Sorting.KeysValuesSorter_Comparable;
//using SIC = DotNetCross.Sorting.IComparableImpl;
//using STC = DotNetCross.Sorting.KeysValuesSorter_TComparer;
using SDC = System.SpanSortHelpersKeysValues_DirectComparer;

namespace DotNetCross.Sorting
{
    internal static partial class IntroKeysValuesSorters
    {
        static readonly object[] EmptyObjects = new object[0];

        internal static class Default<TKey, TValue>
        {
            internal static readonly IKeysValuesSorter<TKey, TValue> Instance = CreateSorter();

            private static IKeysValuesSorter<TKey, TValue> CreateSorter()
            {
                if (TypeTraits<TKey>.IsComparable)
                {
                    // coreclr uses RuntimeTypeHandle.Allocate
                    var ctor = typeof(KeysValuesSorter_Comparable<,>)
                        .MakeGenericType(new Type[] { typeof(TKey), typeof(TValue) })
                        .GetTypeInfo().DeclaredConstructors.Where(ci => !ci.IsStatic).Single();

                    return (IKeysValuesSorter<TKey, TValue>)ctor.Invoke(EmptyObjects);
                }
                else
                {
                    return new NonComparable<TKey, TValue>();
                }
            }
        }

        internal sealed class NonComparable<TKey, TValue>
            : IKeysValuesSorter<TKey, TValue>
        {
            public void IntroSort(ref TKey keys, ref TValue values, int length)
            {
                STC.IntroSort(ref keys, ref values, length, Comparer<TKey>.Default);
            }

            //public void IntroSort(ref TKey keys, ref TValue values, int length, Comparison<TKey> comparison)
            //{
            //    SC.IntroSort(ref keys, ref values, length, comparison);
            //}
        }


        internal static class Default<TKey, TValue, TComparer>
            where TComparer : IComparer<TKey>
        {
            internal static readonly IComparerKeysValuesSorter<TKey, TValue, TComparer> Instance = CreateSorter();

            private static IComparerKeysValuesSorter<TKey, TValue, TComparer> CreateSorter()
            {
                if (TypeTraits<TKey>.IsComparable)
                {
                    // coreclr uses RuntimeTypeHandle.Allocate
                    var ctor = typeof(Comparable<,,>)
                        .MakeGenericType(new Type[] { typeof(TKey), typeof(TValue), typeof(TComparer) })
                        .GetTypeInfo().DeclaredConstructors.Where(ci => !ci.IsStatic).Single();

                    return (IComparerKeysValuesSorter<TKey, TValue, TComparer>)ctor.Invoke(EmptyObjects);
                }
                else
                {
                    return new NonComparable<TKey, TValue, TComparer>();
                }
            }
        }

        internal sealed class NonComparable<TKey, TValue, TComparer>
            : IComparerKeysValuesSorter<TKey, TValue, TComparer>
            where TComparer : IComparer<TKey>
        {
            public void IntroSort(ref TKey keys, ref TValue values, int length, TComparer comparer)
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

        internal sealed class Comparable<TKey, TValue, TComparer>
            : IComparerKeysValuesSorter<TKey, TValue, TComparer>
            where TKey : IComparable<TKey>
            where TComparer : IComparer<TKey>
        {
            internal static readonly KeysValuesSorter_Comparable<TKey, TValue> NonComparerInstance = 
                new KeysValuesSorter_Comparable<TKey, TValue>();

            public void IntroSort(ref TKey keys, ref TValue values, int length,
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
                    (!TypeTraits<TComparer>.IsValueType &&
                     ReferenceEquals(comparer, Comparer<TKey>.Default))) // Or "=="?
                {
                    if (!SDC.TrySortSpecialized(ref keys, ref values, length))
                    {
                        // NOTE: For Bogus Comparable the exception message will be different, when using Comparer<TKey>.Default
                        //       Since the exception message is thrown internally without knowledge of the comparer
                        NonComparerInstance.IntroSort(ref keys, ref values, length);
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