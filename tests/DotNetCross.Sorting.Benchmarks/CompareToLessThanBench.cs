using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace DotNetCross.Sorting.Benchmarks
{
    // Trying to benchmark the canonical generic issue and finding a work around
    // delegate simply not fast enough, as expected, a direct function pointer might have been...
    [DisassemblyDiagnoser(printSource: true)]
    [RyuJitX64Job]
    public class CompareToLessThanBench
    {
        internal interface ILessThanComparer<in T>
        {
            bool LessThan(T x, T y);
        }
        internal struct ComparableLessThanComparer<T> : ILessThanComparer<T>
            where T : IComparable<T>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(T x, T y) => x.CompareTo(y) < 0;
        }
        ComparableLessThanComparer<string> m_comparer = new ComparableLessThanComparer<string>();
        // Create two comparers 
        ComparableLessThanComparer<ComparableClassInt32> m_comparerInt32 = new ComparableLessThanComparer<ComparableClassInt32>();

        Func<object, object, int> m_stringComparerOpen = ComparableOpenDelegate<string>();
        Func<object, object, int> m_comparableInt32ComparerOpen = ComparableOpenDelegate<ComparableClassInt32>();

        public CompareToLessThanBench()
        {
            ComparerCall(new ComparableClassInt32(1), new ComparableClassInt32(2), m_comparerInt32);
            ComparerCall("1287192", "127912", m_comparer);
        }

        public ComparableClassInt32 X = new ComparableClassInt32(12812912);
        public ComparableClassInt32 Y = new ComparableClassInt32(12812913);

        [Benchmark]
        public bool DirectCall()
        {
            //return X.CompareTo(Y) < 0;
            return ComparableDirectCall(X, Y);
        }
        [Benchmark]
        public bool ValueType()
        {
            //return m_comparer.LessThan(X, Y);
            return ComparerCall(X, Y, m_comparerInt32);
        }
        [Benchmark]
        public bool OpenDelegate() // Too slow
        {
            return m_comparableInt32ComparerOpen.Invoke(X, Y) < 0;
        }

        internal bool ComparableDirectCall<T>(T x, T y) where T : IComparable<T>
        {
            return x.CompareTo(y) < 0;
        }
        internal bool ComparerCall<T, TComparer>(T x, T y, TComparer comparer) 
            where TComparer : ILessThanComparer<T>
        {
            return comparer.LessThan(x, y);
        }

        public class SomeComparable : IComparable<SomeComparable>
        {
            public int CompareTo(SomeComparable other)
            {
                throw new NotImplementedException();
            }
        }

        static Func<object, object, int> ComparableOpenDelegate<T>()
            where T : class, IComparable<T>
        {
            var paramType = typeof(T);
            var comparableType = typeof(T);
            const string methodName = nameof(IComparable<T>.CompareTo);
            var methodInfo = comparableType.GetMethod(methodName, new Type[] { paramType });

            Func<T, T, int> openTypedDelegate = (Func<T, T, int>)Delegate.CreateDelegate(typeof(Func<T, T, int>), methodInfo);

            return (x, y) => openTypedDelegate((T)x, (T)y);
        }

        //static Func<T, object, object> MagicMethod<T>(MethodInfo method) where T : class
        //{
        //    // First fetch the generic form
        //    MethodInfo genericHelper = typeof(StringCompareToLessThanBench).GetMethod("MagicMethodHelper",
        //        BindingFlags.Static | BindingFlags.NonPublic);

        //    // Now supply the type arguments
        //    MethodInfo constructedHelper = genericHelper.MakeGenericMethod
        //        (typeof(T), method.GetParameters()[0].ParameterType, method.ReturnType);

        //    // Now call it. The null argument is because it’s a static method.
        //    object ret = constructedHelper.Invoke(null, new object[] { method });

        //    // Cast the result to the right kind of delegate and return it
        //    return (Func<T, object, object>)ret;
        //}

        //static Func<TTarget, object, object> MagicMethodHelper<TTarget, TParam>(MethodInfo method)
        //    where TTarget : class
        //{
        //    // Convert the slow MethodInfo into a fast, strongly typed, open delegate
        //    Func<TTarget> func = (Func<TTarget>)Delegate.CreateDelegate
        //        (typeof(Func<TTarget, TParam>), method);

        //    // Now create a more weakly typed delegate which will call the strongly typed one
        //    Func<TTarget, object, object> ret = (TTarget target, object param) => func(target, (TParam)param);
        //    return ret;
        //}
    }
}
