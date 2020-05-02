using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal static class DelegateDoctor
    {
        internal static Comparison<T> GetComparableCompareToAsOpenDelegate<T>()
            where T : class, IComparable<T>
        {
            var paramType = typeof(T);
            var comparableType = typeof(T);
            const string methodName = nameof(IComparable<T>.CompareTo);
            var methodInfo = comparableType.GetRuntimeMethod(methodName, new Type[] { paramType });

            var comparison = (Comparison<T>)methodInfo.CreateDelegate(typeof(Comparison<T>));

            return comparison;
        }

        internal static Comparison<object> GetComparableCompareToAsOpenObjectDelegate<T>()
            where T : class, IComparable<T>
        {
            var comparison = GetComparableCompareToAsOpenDelegate<T>();
            return Unsafe.As<Comparison<object>>(comparison);
        }
    }
}
