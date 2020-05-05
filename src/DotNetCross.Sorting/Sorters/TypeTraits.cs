using System;
using System.Collections.Generic;
using System.Reflection;

namespace DotNetCross.Sorting
{
    internal static class TypeTraits<T>
    {
        internal static readonly bool IsValueType = CheckIsValueType();
        internal static readonly bool IsComparable = CheckIsComparable();

        internal static bool IsComparerNullOrDefault<TComparer>(TComparer comparer)
            where TComparer : IComparer<T>
        {
            return comparer == null || (!IsValueType && 
                ReferenceEquals(comparer, Comparer<T>.Default));
        }

        static bool CheckIsComparable() => typeof(IComparable<T>).GetTypeInfo()
            .IsAssignableFrom(typeof(T).GetTypeInfo());

        static bool CheckIsValueType() => typeof(T).GetTypeInfo()
            .IsValueType;
    }
}