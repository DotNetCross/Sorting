using System.Diagnostics;
using System.Runtime.CompilerServices;

//#if !netstandard
//using Internal.Runtime.CompilerServices;
//#endif

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        // This is the threshold where Introspective sort switches to Insertion sort.
        // Empirically, 16 seems to speed up most cases without slowing down others, at least for integers.
        // Large value types may benefit from a smaller number.
        internal const int IntrosortSizeThreshold = 16;

        internal static int FloorLog2PlusOne(int n)
        {
            Debug.Assert(n >= 2);
            int result = 2;
            n >>= 2;
            while (n > 0)
            {
                ++result;
                n >>= 1;
            }
            return result;
        }
    }
}
