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

        internal static class ForStraight<TKey>
        {
            internal static readonly IKeysSorter<TKey> Instance = CreateSorter();

            static IKeysSorter<TKey> CreateSorter()
            {
                if (TypeTraits<TKey>.IsComparable)
                {
                    // coreclr uses RuntimeTypeHandle.Allocate
                    var ctor = typeof(KeysSorter_Comparable<>)
                        .MakeGenericType(new Type[] { typeof(TKey) })
                        .GetTypeInfo().DeclaredConstructors.Where(ci => !ci.IsStatic).Single();

                    return (IKeysSorter<TKey>)ctor.Invoke(EmptyObjects);
                }
                else
                {
                    // TODO: Replace with delegate as saved instance, then we can lose this type
                    //       There will still be an indirection cost
                    return new KeysSorter_NonComparable<TKey>();
                }
            }
        }

        internal static class ForComparer<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            internal static readonly IComparerKeysSorter<TKey, TComparer> Instance = CreateSorter();

            static IComparerKeysSorter<TKey, TComparer> CreateSorter()
            {
                return new KeysSorter_TComparer<TKey, TComparer>();
            }
        }

        internal static class ForComparison<TKey>
        {
            internal static readonly IComparisonKeysSorter<TKey> Instance = CreateSorter();

            static IComparisonKeysSorter<TKey> CreateSorter()
            {
                // TODO: Check if Comparison is default perhaps
                return new KeysSorter_Comparison<TKey>();
            }
        }

        internal sealed class KeysSorter_NonComparable<TKey> : IKeysSorter<TKey>
        {
            // TODO: Perhaps cache on TypeTraits
            internal static readonly Comparison<TKey> Comparison = Comparer<TKey>.Default.Compare;

            public void IntroSort(ref TKey keys, int length)
            {
                // PERF: Using Comparison<TKey> since faster than interface call
                ForComparison<TKey>.Instance.IntroSort(ref keys, length, Comparison);
            }
        }
    }
}