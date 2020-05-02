using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;

namespace DotNetCross.Sorting.Benchmarks
{
    public class ComparableInt32ClassCompareToLessThanBench : CompareToLessThanBench<ComparableClassInt32>
    {
        public ComparableInt32ClassCompareToLessThanBench()
        {
            X = new ComparableClassInt32(12812912);
            Y = new ComparableClassInt32(12812913);
        }
    }

    // Trying to benchmark the canonical generic issue and finding a work around
    // delegate simply not fast enough, as expected, a direct function pointer might have been...
    [MemoryDiagnoser]
    //[DisassemblyDiagnoser(printSource: true, maxDepth: 2)]
    [SimpleJob(RunStrategy.Throughput, warmupCount: 5, targetCount: 15, id: "CompareJob")]
    public abstract class CompareToLessThanBench<TComparable> 
        where TComparable : class, IComparable<TComparable>
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
        internal readonly struct OpenDelegateObjectComparer : ILessThanComparer<object>
        {
            readonly Comparison<object> m_compare;

            public OpenDelegateObjectComparer(Comparison<object> compare)
            {
                m_compare = compare;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(object x, object y) => m_compare(x, y) < 0;
        }

        internal readonly struct OpenDelegateObjectComparerWithChecks : ILessThanComparer<object>
        {
            readonly Comparison<object> m_compare;

            public OpenDelegateObjectComparerWithChecks(Comparison<object> compare)
            {
                m_compare = compare;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(object x, object y)
            {
                // Mimicking GenericComparer<T>
                if (x != null)
                {
                    if (y != null) return m_compare(x, y) < 0;
                    return false;
                }
                if (y != null) return true;
                return false;
            }
        }

        readonly ComparableLessThanComparer<TComparable> m_valueTypeComparer = 
            new ComparableLessThanComparer<TComparable>();

        readonly OpenDelegateObjectComparer m_openDelegateObjectComparer;
        readonly OpenDelegateObjectComparerWithChecks m_openDelegateObjectComparerWithChecks;

        readonly Comparison<object> m_comparableComparerOpen = DelegateDoctor
            .GetComparableCompareToAsOpenObjectDelegate<TComparable>();
        //readonly Func<TComparable, int> m_comparableComparerClosed;
        readonly IComparer<TComparable> m_defaultComparer;
        readonly Comparison<TComparable> m_comparerComparableComparerClosed =
            Comparer<TComparable>.Default.Compare;
        readonly Func<object, object, object, int> m_comparerComparableComparerOpen =
            ComparerOpenDelegate<TComparable>();

        public CompareToLessThanBench()
        {
            //m_comparableComparerClosed = X.CompareTo;

            // Default creation can be seen at (for CoreCLR):
            // https://github.com/dotnet/runtime/blob/master/src/coreclr/src/System.Private.CoreLib/src/System/Collections/Generic/ComparerHelpers.cs
            // Which for IComparable types create GenericComparer<T>
            // public override int Compare([AllowNull] T x, [AllowNull] T y)
            // {
            //     if (x != null)
            //     {
            //         if (y != null) return x.CompareTo(y);
            //         return 1;
            //     }
            //     if (y != null) return -1;
            //     return 0;
            // }
            // https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/Comparer.cs
            m_defaultComparer = Comparer<TComparable>.Default;
            m_openDelegateObjectComparer = new OpenDelegateObjectComparer(m_comparableComparerOpen);
            m_openDelegateObjectComparerWithChecks = new OpenDelegateObjectComparerWithChecks(m_comparableComparerOpen);
        }

        public TComparable X;
        public TComparable Y;

        [Benchmark]
        public bool DirectCall()
        {
            return X.CompareTo(Y) < 0;
        }
        [Benchmark]
        public bool DirectCallWhereT()
        {
            return ComparableDirectCallWhereT(X, Y);
        }
        [Benchmark]
        public bool InterfaceCall()
        {
            return ComparableInterfaceCall(X, Y);
        }
        [Benchmark]
        public bool ValueType()
        {
            //return m_comparer.LessThan(X, Y);
            return ComparerCall(X, Y, m_valueTypeComparer);
        }
        //[Benchmark]
        //public bool InstanceClosedDelegate()
        //{
        //    return m_comparableComparerClosed.Invoke(Y) < 0;
        //}
        [Benchmark]
        public bool InstanceOpenDelegate()
        {
            return m_comparableComparerOpen.Invoke(X, Y) < 0;
        }
        [Benchmark]
        public bool ComparableOpenDelegateObjectComparer()
        {
            return m_openDelegateObjectComparer.LessThan(X, Y);
        }
        [Benchmark]
        public bool ComparableOpenDelegateObjectComparerWithChecks()
        {
            return m_openDelegateObjectComparerWithChecks.LessThan(X, Y);
        }

        [Benchmark(Baseline = true)]
        public bool ComparerDefault()
        {
            return m_defaultComparer.Compare(X, Y) < 0;
        }
        [Benchmark]
        public bool ComparerClosedDelegate()
        {
            return m_comparerComparableComparerClosed(X, Y) < 0;
        }
        [Benchmark]
        public bool ComparerOpenDelegate()
        {
            return m_comparerComparableComparerOpen(m_defaultComparer, X, Y) < 0;
        }

        internal bool ComparableDirectCallWhereT<T>(T x, T y) where T : IComparable<T>
        {
            return x.CompareTo(y) < 0;
        }
        internal bool ComparableInterfaceCall<T>(IComparable<T> x, T y)
        {
            return x.CompareTo(y) < 0;
        }
        internal bool ComparerCall<T, TComparer>(T x, T y, TComparer comparer) 
            where TComparer : ILessThanComparer<T>
        {
            return comparer.LessThan(x, y);
        }


        public static Func<object, object, object, int> ComparerOpenDelegate<T>()
            where T : class, IComparable<T>
        {
            var paramType = typeof(T);
            var comparableType = typeof(Comparer<T>);
            const string methodName = nameof(Comparer<T>.Compare);
            var methodInfo = comparableType.GetMethod(methodName, new Type[] { paramType, paramType });

            Func<Comparer<T>, T, T, int> openTypedDelegate = (Func<Comparer<T>, T, T, int>)
                Delegate.CreateDelegate(typeof(Func<Comparer<T>, T, T, int>), methodInfo);

            return Unsafe.As<Func<object, object, object, int>>(openTypedDelegate);
            //return (x, y) => openTypedDelegate((T)x, (T)y);
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
