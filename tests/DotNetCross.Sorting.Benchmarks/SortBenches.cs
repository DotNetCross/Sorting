using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace DotNetCross.Sorting.Benchmarks
{
    public class Int32SortBench : SortBench<int>
    {
        public Int32SortBench()
            : base(maxLength: 3000000, new[] { 2, 3, 10, 100, 1000, 10000, 1000000 },
                   SpanFillers.RandomOnly, i => i)
        { }

        [Benchmark]
        public void DNX_CustomStructComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<int>(_work, i, Length).IntroSort(new CustomStructComparer());
            }
        }

        struct CustomStructComparer : IComparer<int>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(int x, int y) => x.CompareTo(y);
        }
    }
    public class SingleSortBench : SortBench<float>
    {
        public SingleSortBench()
            : base(maxLength: 3000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i)
        { }
    }
    public class StringSortBench : SortBench<string>
    {
        public StringSortBench()
            : base(maxLength: 100000, new[] { 2, 3, 10, 100, 1000, 10000 },
                   SpanFillers.Default, i => i.ToString("D9"))
        { }
    }
    public class ComparableStructInt32SortBench : SortBench<ComparableStructInt32>
    {
        public ComparableStructInt32SortBench()
            : base(maxLength: 3000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => new ComparableStructInt32(i))
        { }
    }
    //[InliningDiagnoser(true, true)]
    public class ComparableClassInt32SortBench : SortBench<ComparableClassInt32>
    {
        readonly Comparison<object> m_comparableComparisonOpen = DelegateDoctor
            .GetComparableCompareToAsOpenObjectDelegate<ComparableClassInt32>();
        readonly Comparison<object> m_icomparableComparisonOpen = DelegateDoctor
            .GetIComparableCompareToAsOpenObjectDelegate<ComparableClassInt32>();

        public ComparableClassInt32SortBench()
            : base(maxLength: 400000, new[] { 2, 3, 10, 100, 10000, 100000 },
                   SpanFillers.Default, i => new ComparableClassInt32(i))
        { }

        [Benchmark]
        public void DNX_DirectComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                ref var keys = ref _work[i];
                KeysSorter_TDirectComparer<ComparableClassInt32, DirectComparer>
                    .IntroSort(ref keys, Length, new DirectComparer());
            }
        }

        [Benchmark]
        public void DNX_InterfaceDirectComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                ref var keys = ref _work[i];
                KeysSorter_TDirectComparer.IntroSort(ref keys, Length,
                    new InterfaceDirectComparer<ComparableClassInt32>());
            }
        }

        [Benchmark]
        public void DNX_Comparison_TComparable_OpenDelegate()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<ComparableClassInt32>(_work, i, Length).IntroSort(m_comparableComparisonOpen);
            }
        }

        [Benchmark]
        public void DNX_Comparison_IComparable_OpenDelegate()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<ComparableClassInt32>(_work, i, Length).IntroSort(m_icomparableComparisonOpen);
            }
        }

        ClassGenericDirectComparer<ComparableClassInt32> m_classGenericDirectComparer =
            new ClassGenericDirectComparer<ComparableClassInt32>();
        [Benchmark]
        public void DNX_ClassInterfaceDirectComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                ref var keys = ref _work[i];
                KeysSorter_TDirectComparer.IntroSort(ref keys, Length, m_classGenericDirectComparer);
            }
        }

        [Benchmark]
        public void DNX_GenericDirectComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                ref var keys = ref _work[i];
                KeysSorter_TDirectComparer.IntroSort(ref keys, Length,
                    new GenericDirectComparer<ComparableClassInt32>());
            }
        }

        [Benchmark]
        public void DNX_OpenDelegateDirectComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                ref var keys = ref _work[i];
                KeysSorter_TDirectComparer.IntroSort(ref keys, Length,
                    new OpenDelegateDirectComparer(m_comparableComparisonOpen));
            }
        }

        [Benchmark]
        public void DNX_OpenDelegateObjectDirectComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                ref var keys = ref _work[i];
                KeysSorter_TDirectComparer.IntroSort(ref keys, Length,
                    new OpenDelegateObjectDirectComparer(m_comparableComparisonOpen));
            }
        }

        [Benchmark]
        public void DNX_OpenDelegateObjectComparer()
        {
            for (int i = 0; i <= _maxLength - Length; i += Length)
            {
                new Span<ComparableClassInt32>(_work, i, Length)
                    .IntroSort(new OpenDelegateObjectComparer(m_comparableComparisonOpen));
            }
        }

        readonly struct DirectComparer : IDirectComparer<ComparableClassInt32>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool GreaterThan(ComparableClassInt32 x, ComparableClassInt32 y) => x.CompareTo(y) > 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(ComparableClassInt32 x, ComparableClassInt32 y) => x.CompareTo(y) < 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThanEqual(ComparableClassInt32 x, ComparableClassInt32 y) => x.CompareTo(y) <= 0;
        }

        readonly struct InterfaceDirectComparer<T> : IDirectComparer<IComparable<T>>
            where T : class, IComparable<T>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool GreaterThan(IComparable<T> x, IComparable<T> y) => x.CompareTo(Unsafe.As<T>(y)) > 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(IComparable<T> x, IComparable<T> y) => x.CompareTo(Unsafe.As<T>(y)) < 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThanEqual(IComparable<T> x, IComparable<T> y) => x.CompareTo(Unsafe.As<T>(y)) <= 0;
        }

        sealed class ClassGenericDirectComparer<T> : IDirectComparer<T>
            where T : IComparable<T>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool GreaterThan(T x, T y) => x.CompareTo(y) > 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(T x, T y) => x.CompareTo(y) < 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThanEqual(T x, T y) => x.CompareTo(y) <= 0;
        }

        readonly struct GenericDirectComparer<T> : IDirectComparer<T>
            where T : IComparable<T>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool GreaterThan(T x, T y) => x.CompareTo(y) > 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(T x, T y) => x.CompareTo(y) < 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThanEqual(T x, T y) => x.CompareTo(y) <= 0;
        }

        readonly struct OpenDelegateObjectDirectComparer : IDirectComparer<object>
        {
            readonly Comparison<object> m_compare;

            public OpenDelegateObjectDirectComparer(Comparison<object> compare) =>
                m_compare = compare;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool GreaterThan(object x, object y) => m_compare(x, y) > 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(object x, object y) => m_compare(x, y) < 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThanEqual(object x, object y) => m_compare(x, y) <= 0;
        }

        // Methods with delegate calls cannot be inlined unfortunately! So have to give up.
        // https://github.com/dotnet/runtime/issues/35791 (issue filed by nietras)
        // https://github.com/dotnet/runtime/issues/10048 (missed this before filing above)
        readonly struct OpenDelegateObjectComparer : IComparer<object>
        {
            readonly Comparison<object> m_compare;

            public OpenDelegateObjectComparer(Comparison<object> compare) =>
                m_compare = compare;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(object x, object y) => m_compare(x, y);
        }

        readonly struct OpenDelegateDirectComparer : IDirectComparer<ComparableClassInt32>
        {
            readonly Comparison<object> m_compare;

            public OpenDelegateDirectComparer(Comparison<object> compare) =>
                m_compare = compare;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool GreaterThan(ComparableClassInt32 x, ComparableClassInt32 y) => m_compare(x, y) > 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThan(ComparableClassInt32 x, ComparableClassInt32 y) => m_compare(x, y) < 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool LessThanEqual(ComparableClassInt32 x, ComparableClassInt32 y) => m_compare(x, y) <= 0;
        }

    }

    public class Sample<T, TComparer>
        where TComparer : IComparer<T>
    {
        public int Do(Span<T> s, T value, TComparer comparer)
        {
            int count = 0;
            foreach (var v in s)
            {
                if (comparer.Compare(v, value) < 0)
                {
                    ++count;
                }
            }
            return count;
        }
    }

    public class Int32Int32SortBench : SortBench<int, int>
    {
        public Int32Int32SortBench()
            : base(maxLength: 2000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i, i => i)
        { }
    }
    public class Int32SingleSortBench : SortBench<int, float>
    {
        public Int32SingleSortBench()
            : base(maxLength: 2000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i, i => i)
        { }
    }
    public class SingleInt32SortBench : SortBench<float, int>
    {
        public SingleInt32SortBench()
            : base(maxLength: 2000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i, i => i)
        { }
    }
    public class SingleSingleSortBench : SortBench<float, float>
    {
        public SingleSingleSortBench()
            : base(maxLength: 2000000, new[] { 2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i, i => i)
        { }
    }
    public class Int32StringSortBench : SortBench<int, string>
    {
        public Int32StringSortBench()
            : base(maxLength: 2000000, new[] { 1000000 }, //2, 3, 10, 100, 10000, 1000000 },
                   SpanFillers.Default, i => i, i => i.ToString("D9"))
        { }
    }
    public class StringInt32SortBench : SortBench<string, int>
    {
        public StringInt32SortBench()
            : base(maxLength: 100000, new[] { 2, 3, 10, 100, 1000, 10000 },
                   SpanFillers.Default, i => i.ToString("D9"), i => i)
        { }
    }
    public class ComparableClassInt32Int32SortBench : SortBench<ComparableClassInt32, int>
    {
        public ComparableClassInt32Int32SortBench()
            : base(maxLength: 100000, new[] { 2, 3, 10, 100, 1000, 10000 },
                   SpanFillers.Default, i => new ComparableClassInt32(i), i => i)
        { }
    }
    public class ComparableStructInt32Int32SortBench : SortBench<ComparableClassInt32, int>
    {
        public ComparableStructInt32Int32SortBench()
            : base(maxLength: 100000, new[] { 2, 3, 10, 100, 1000, 10000 },
                   SpanFillers.Default, i => new ComparableClassInt32(i), i => i)
        { }
    }


    public class Int32SortDisassemblerBench : SortDisassemblerBench<int>
    {
        public Int32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => i)
        { }
    }
    public class SingleSortDisassemblerBench : SortDisassemblerBench<float>
    {
        public SingleSortDisassemblerBench()
            : base(length: 1024 * 1024, i => i)
        { }
    }
    public class ComparableStructInt32SortDisassemblerBench : SortDisassemblerBench<ComparableStructInt32>
    {
        public ComparableStructInt32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => new ComparableStructInt32(i))
        { }
    }
    public class ComparableClassInt32SortDisassemblerBench : SortDisassemblerBench<ComparableClassInt32>
    {
        public ComparableClassInt32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => new ComparableClassInt32(i))
        { }
    }
    public class StringSortDisassemblerBench : SortDisassemblerBench<string>
    {
        public StringSortDisassemblerBench()
            : base(length: 1024 * 1024, i => i.ToString("D9"))
        { }
    }

    public class Int32Int32SortDisassemblerBench : SortDisassemblerBench<int, int>
    {
        public Int32Int32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => i, i => i)
        { }
    }
    public class Int32SingleSortDisassemblerBench : SortDisassemblerBench<int, float>
    {
        public Int32SingleSortDisassemblerBench()
            : base(length: 1024 * 1024, i => i, i => i)
        { }
    }
    public class SingleInt32SortDisassemblerBench : SortDisassemblerBench<float, int>
    {
        public SingleInt32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => i, i => i)
        { }
    }
    public class StringInt32SortDisassemblerBench : SortDisassemblerBench<string, int>
    {
        public StringInt32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => i.ToString("D9"), i => i)
        { }
    }
    public class Int32StringSortDisassemblerBench : SortDisassemblerBench<int, string>
    {
        public Int32StringSortDisassemblerBench()
            : base(length: 1024 * 1024, i => i, i => i.ToString("D9"))
        { }
    }
    public class ComparableClassInt32Int32SortDisassemblerBench : SortDisassemblerBench<ComparableClassInt32, int>
    {
        public ComparableClassInt32Int32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => new ComparableClassInt32(i), i => i)
        { }
    }
    public class ComparableStructInt32Int32SortDisassemblerBench : SortDisassemblerBench<ComparableClassInt32, int>
    {
        public ComparableStructInt32Int32SortDisassemblerBench()
            : base(length: 1024 * 1024, i => new ComparableClassInt32(i), i => i)
        { }
    }
}
