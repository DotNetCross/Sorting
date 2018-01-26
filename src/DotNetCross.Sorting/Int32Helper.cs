using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    internal static class Int32Helper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool GreaterThan(this int a, int b)
        {
            return a > b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool GreaterThanEqual(this int a, int b)
        {
            return a >= b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool LessThan(this int a, int b)
        {
            return a < b;
        }
    }
}
