using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetCross.Sorting
{
    internal static partial class IntroKeysValuesSorters
    {
        static readonly object[] EmptyObjects = new object[0];

        internal static class ForStraight<TKey, TValue>
        {
            internal delegate void Sort(ref TKey keys, ref TValue values, int length);
            internal static readonly Sort Instance = Create();

            static Sort Create()
            {
                if (TypeTraits<TKey>.IsComparable)
                {
                    // coreclr uses RuntimeTypeHandle.Allocate
                    var ctor = typeof(KeysValuesSorter_Comparable<,>)
                        .MakeGenericType(new Type[] { typeof(TKey), typeof(TValue) })
                        .GetTypeInfo().DeclaredConstructors.Where(ci => !ci.IsStatic).Single();

                    var sorter = (IKeysValuesSorter<TKey, TValue>)ctor.Invoke(EmptyObjects);
                    return sorter.IntroSort;
                }
                else
                {
                    Comparison<TKey> comparison = Comparer<TKey>.Default.Compare;
                    // PERF: Using Comparison<TKey> since faster than interface call
                    // PERF: There is a double indirection cost here for small sorts
                    return (ref TKey keys, ref TValue values, int length) =>
                        ForComparison<TKey, TValue>.Instance(ref keys, ref values, length, comparison);
                }
            }
        }

        internal static class ForComparer<TKey, TValue, TComparer>
            where TComparer : IComparer<TKey>
        {
            internal delegate void Sort(ref TKey keys, ref TValue values, int length, TComparer comparer);
            internal static readonly Sort Instance = Create();

            static Sort Create()
            {
                var sorter = new KeysValuesSorter_TComparer<TKey, TValue, TComparer>();
                return sorter.IntroSort;
            }
        }

        internal static class ForComparison<TKey, TValue>
        {
            internal delegate void Sort(ref TKey keys, ref TValue values, int length, Comparison<TKey> comparison);
            internal static readonly Sort Instance = Create();

            static Sort Create()
            {
                // TODO: Check if Comparison is default perhaps
                var sorter = new KeysValuesSorter_Comparison<TKey, TValue>();
                return sorter.IntroSort;
            }
        }
    }
}