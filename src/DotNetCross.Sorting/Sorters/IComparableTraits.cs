using System;
using System.Reflection;

namespace DotNetCross.Sorting
{
    internal static class IComparableTraits<T>
    {
        public static readonly bool IsComparable = CheckIsComparable();

        static bool CheckIsComparable() =>
            typeof(IComparable<T>).GetTypeInfo()
                .IsAssignableFrom(typeof(T).GetTypeInfo());
    }
}