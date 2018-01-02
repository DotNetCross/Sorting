using System;
using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    public static class IntPtrHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool LessThan(this IntPtr a, IntPtr b)
        {
            return (sizeof(IntPtr) == sizeof(int))
                ? (int)a < (int)b
                : (long)a < (long)b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool LessThan(this IntPtr a, UIntPtr b)
        {
            return (sizeof(UIntPtr) == sizeof(uint))
                ? (int)a < (int)b
                : (long)a < (long)b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool LessThanEqual(this IntPtr a, IntPtr b)
        {
            return (sizeof(IntPtr) == sizeof(int))
                ? (int)a <= (int)b
                : (long)a <= (long)b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool LessThanEqual(this IntPtr a, UIntPtr b)
        {
            return (sizeof(UIntPtr) == sizeof(uint))
                ? (int)a <= (int)b
                : (long)a <= (long)b;
        }
    }
}
