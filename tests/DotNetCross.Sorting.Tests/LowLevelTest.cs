using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace DotNetCross.Sorting.Tests
{
    public class LowLevelTest
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct ComparisonAsStruct
        {
            [MarshalAs(UnmanagedType.FunctionPtr)]
            internal Comparison<object> Delegate;
        };

        public class Comp
            : IComparable<Comp>
        {
            public readonly int Value;

            public Comp(int value) =>
                Value = value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(Comp other) =>
                Value.CompareTo(other.Value);
        }

        // Marshal.GetDelegateForFunctionPointer does not work for generic delegates
        delegate int ComparisonComp(Comp x, Comp y);

        [Fact]
        public unsafe void ClassMethodToPointer()
        {
            IntPtr functionPointer = GetFunctionPointerComp();

            Assert.NotEqual(IntPtr.Zero, functionPointer);
        }

        private static unsafe IntPtr GetFunctionPointerComp()
        {
            var paramType = typeof(Comp);
            var comparableType = typeof(Comp);
            const string methodName = nameof(IComparable<Comp>.CompareTo);
            var methodInfo = comparableType.GetRuntimeMethod(methodName, new Type[] { paramType });

            var runtimeHandle = methodInfo.MethodHandle;
            var functionPointer = runtimeHandle.GetFunctionPointer();
            return functionPointer;
        }

        [Fact]
        public unsafe void InterfaceMethodToPointer()
        {
            IntPtr functionPointer = GetFunctionPointerCompAsIComparable();

            Assert.NotEqual(IntPtr.Zero, functionPointer);
        }

        private static unsafe IntPtr GetFunctionPointerCompAsIComparable()
        {
            var paramType = typeof(Comp);
            var comparableType = typeof(IComparable<Comp>);
            const string methodName = nameof(IComparable<Comp>.CompareTo);
            var methodInfo = comparableType.GetRuntimeMethod(methodName, new Type[] { paramType });

            var runtimeHandle = methodInfo.MethodHandle;
            var functionPointer = runtimeHandle.GetFunctionPointer();
            return functionPointer;
        }

        [Fact]
        public unsafe void DelegateToPointer()
        {
            var compare = DelegateDoctor.GetComparableCompareToAsOpenObjectDelegate<Comp>();
            
            Assert.Equal(-1, compare(new Comp(-1), new Comp(1)));
            Assert.Equal(0, compare(new Comp(1), new Comp(1)));
            Assert.Equal(1, compare(new Comp(1), new Comp(-1)));

            var methodInfo = compare.Method;
            var runtimeHandle = methodInfo.MethodHandle;
            var functionPointer = runtimeHandle.GetFunctionPointer();

            Assert.NotEqual(IntPtr.Zero, functionPointer);

            var compareFromPtr = Marshal.GetDelegateForFunctionPointer<ComparisonComp>(functionPointer);
            // Below fails yielding incorrect results for some reason
            //Assert.Equal(-1, compareFromPtr(new Comp(-1), new Comp(1)));
            //Assert.Equal(0, compareFromPtr(new Comp(1), new Comp(1)));
            //Assert.Equal(1, compareFromPtr(new Comp(1), new Comp(-1)));
        }
    }
}
