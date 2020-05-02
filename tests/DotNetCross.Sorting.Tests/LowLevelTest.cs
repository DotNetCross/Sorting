using System;
using System.Collections.Generic;
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

        [Fact]
        public unsafe void DelegateToPointer()
        {
            var compare = DelegateDoctor.GetComparableCompareToAsOpenObjectDelegate<Comp>();
            var methodInfo = compare.Method;
            var runtimeHandle = methodInfo.MethodHandle;
            var functionPointer = runtimeHandle.GetFunctionPointer();

            Assert.NotEqual(IntPtr.Zero, functionPointer);

            //var asStruct = new ComparisonAsStruct() { Delegate = Unsafe.As<Comparison<object>>(compare) };
            //IntPtr delegatePointer = IntPtr.Zero;
            //void* pointerToDelegatePointer = &delegatePointer;
            //Marshal.StructureToPtr(asStruct, new IntPtr(pointerToDelegatePointer), false);
            //Assert.NotEqual(IntPtr.Zero, delegatePointer);
        }
    }
}
