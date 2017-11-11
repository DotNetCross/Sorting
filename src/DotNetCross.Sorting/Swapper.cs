using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    public static class Swapper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T a, ref T b)
        {
            var t = a;
            a = b;
            b = t;
        }
    }
}
