using System.Reflection;

namespace DotNetCross.Sorting
{
    internal static class ValueTraits<T>
    {
        public static readonly bool IsValueType = CheckIsValueType();

        static bool CheckIsValueType() => typeof(T).GetTypeInfo().IsValueType;
    }
}