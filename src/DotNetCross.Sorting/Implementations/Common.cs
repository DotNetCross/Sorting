using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    public static partial class Common
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

        // For sorting, move all NaN instances to front of the input array
        internal static int NaNPrepass<TKey, TIsNaN>(
            ref TKey keys, int length,
            TIsNaN isNaN)
            where TIsNaN : struct, IIsNaN<TKey>
        {
            int left = 0;
            for (int i = 0; i < length; i++)
            {
                ref TKey current = ref Unsafe.Add(ref keys, i);
                if (isNaN.IsNaN(current))
                {
                    // TODO: If first index is not NaN or we find just one not NaNs 
                    //       we could skip to version that no longer checks this
                    if (left != i)
                    {
                        ref TKey previous = ref Unsafe.Add(ref keys, left);
                        Swap(ref previous, ref current);
                    }
                    ++left;
                }
            }
            return left;
        }

        // For sorting, move all NaN instances to front of the input array
        internal static int NaNPrepass<TKey, TValue, TIsNaN>(
            ref TKey keys, ref TValue values, int length,
            TIsNaN isNaN)
            where TIsNaN : struct, IIsNaN<TKey>
        {
            int left = 0;
            for (int i = 0; i < length; i++)
            {
                ref TKey current = ref Unsafe.Add(ref keys, i);
                if (isNaN.IsNaN(current))
                {
                    // TODO: If first index is not NaN or we find just one not NaNs 
                    //       we could skip to version that no longer checks this
                    if (left != i)
                    {
                        ref TKey previous = ref Unsafe.Add(ref keys, left);
                        Swap(ref previous, ref current);
                        Swap(ref values, left, i);
                    }
                    ++left;
                }
            }
            return left;
        }
    }
}
