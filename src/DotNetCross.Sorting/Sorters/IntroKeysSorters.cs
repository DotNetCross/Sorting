﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetCross.Sorting
{
    internal static partial class IntroKeysSorters
    {
        static readonly object[] EmptyObjects = new object[0];

        internal static class ForStraight<TKey>
        {
            internal delegate void Sort(ref TKey keys, int length);
            internal static readonly Sort Instance = Create();

            static Sort Create()
            {
                if (TypeTraits<TKey>.IsComparable)
                {
                    // coreclr uses RuntimeTypeHandle.Allocate
                    var ctor = typeof(KeysSorter_Comparable<>)
                        .MakeGenericType(new Type[] { typeof(TKey) })
                        .GetTypeInfo().DeclaredConstructors.Where(ci => !ci.IsStatic).Single();

                    var sorter = (IKeysSorter<TKey>)ctor.Invoke(EmptyObjects);
                    return sorter.IntroSort;
                }
                else
                {
                    Comparison<TKey> comparison = Comparer<TKey>.Default.Compare;
                    // PERF: Using Comparison<TKey> since faster than interface call
                    // PERF: There is a double indirection cost here for small sorts
                    return (ref TKey keys, int length) => 
                        ForComparison<TKey>.Instance(ref keys, length, comparison);
                }
            }
        }

        internal static class ForComparer<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            internal delegate void Sort(ref TKey keys, int length, TComparer comparer);
            internal static readonly Sort Instance = Create();

            static Sort Create()
            {
                var sorter = new KeysSorter_TComparer<TKey, TComparer>();
                return sorter.IntroSort;
            }
        }

        internal static class ForComparison<TKey>
        {
            internal delegate void Sort(ref TKey keys, int length, Comparison<TKey> comparison);
            internal static readonly Sort Instance = Create();

            static Sort Create()
            {
                // TODO: Check if Comparison is default perhaps
                var sorter = new KeysSorter_Comparison<TKey>();
                return sorter.IntroSort;
            }
        }
    }
}