using System;
using System.Runtime.CompilerServices;

// TODO: Rename namespace when done with span sort
namespace System
{
    public static class IntPtrHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static IntPtr Multiply(this IntPtr a, int factor)
        {
            return (sizeof(IntPtr) == sizeof(int))
                ? new IntPtr((int)a * factor)
                : new IntPtr((long)a * factor);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static IntPtr Divide(this IntPtr a, int factor)
        {
            return (sizeof(IntPtr) == sizeof(int))
                ? new IntPtr((int)a / factor)
                : new IntPtr((long)a / factor);
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool GreaterThanEqual(this IntPtr a, IntPtr b)
        {
            return (sizeof(IntPtr) == sizeof(int))
                ? (int)a >= (int)b
                : (long)a >= (long)b;
        }
    }
}
