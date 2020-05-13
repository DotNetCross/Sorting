using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal static partial class KeysSorter_TDirectComparer
    {
        // https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySortSpecialized<TKey>(
            ref TKey keys, int length)
        {
            // Type unfolding adopted from https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp#L268
            if (typeof(TKey) == typeof(sbyte))
            {
                ref var specificKeys = ref Unsafe.As<TKey, sbyte>(ref keys);
                KeysSorter_TDirectComparer<sbyte, SByteDirectComparer>
                    .IntroSort(ref specificKeys, length, new SByteDirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(byte) ||
                     typeof(TKey) == typeof(bool)) // Use byte for bools to reduce code size
            {
                ref var specificKeys = ref Unsafe.As<TKey, byte>(ref keys);
                KeysSorter_TDirectComparer<byte, ByteDirectComparer>
                    .IntroSort(ref specificKeys, length, new ByteDirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(short))
            {
                ref var specificKeys = ref Unsafe.As<TKey, short>(ref keys);
                KeysSorter_TDirectComparer<short, Int16DirectComparer>
                    .IntroSort(ref specificKeys, length, new Int16DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(ushort) ||
                     typeof(TKey) == typeof(char)) // Use ushort for chars to reduce code size)
            {
                ref var specificKeys = ref Unsafe.As<TKey, ushort>(ref keys);
                KeysSorter_TDirectComparer<ushort, UInt16DirectComparer>
                    .IntroSort(ref specificKeys, length, new UInt16DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(int))
            {
                ref var specificKeys = ref Unsafe.As<TKey, int>(ref keys);
                KeysSorter_TDirectComparer<int, Int32DirectComparer>
                    .IntroSort(ref specificKeys, length, new Int32DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(uint))
            {
                ref var specificKeys = ref Unsafe.As<TKey, uint>(ref keys);
                KeysSorter_TDirectComparer<uint, UInt32DirectComparer>
                    .IntroSort(ref specificKeys, length, new UInt32DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(long))
            {
                ref var specificKeys = ref Unsafe.As<TKey, long>(ref keys);
                KeysSorter_TDirectComparer<long, Int64DirectComparer>
                    .IntroSort(ref specificKeys, length, new Int64DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(ulong))
            {
                ref var specificKeys = ref Unsafe.As<TKey, ulong>(ref keys);
                KeysSorter_TDirectComparer<ulong, UInt64DirectComparer>
                    .IntroSort(ref specificKeys, length, new UInt64DirectComparer());
                return true;
            }
            else if (typeof(TKey) == typeof(float))
            {
                ref var specificKeys = ref Unsafe.As<TKey, float>(ref keys);

                // Comparison to NaN is always false, so do a linear pass 
                // and swap all NaNs to the front of the array
                var left = Common.NaNPrepass(ref specificKeys, length, new SingleIsNaN());

                var remaining = length - left;
                if (remaining > 1)
                {
                    ref var afterNaNsKeys = ref Unsafe.Add(ref specificKeys, left);
                    KeysSorter_TDirectComparer<float, SingleDirectComparer>
                        .IntroSort(ref afterNaNsKeys, remaining, new SingleDirectComparer());
                }
                return true;
            }
            else if (typeof(TKey) == typeof(double))
            {
                ref var specificKeys = ref Unsafe.As<TKey, double>(ref keys);

                // Comparison to NaN is always false, so do a linear pass 
                // and swap all NaNs to the front of the array
                var left = Common.NaNPrepass(ref specificKeys, length, new DoubleIsNaN());
                var remaining = length - left;
                if (remaining > 1)
                {
                    ref var afterNaNsKeys = ref Unsafe.Add(ref specificKeys, left);
                    KeysSorter_TDirectComparer<double, DoubleDirectComparer>
                        .IntroSort(ref afterNaNsKeys, remaining, new DoubleDirectComparer());
                }
                return true;
            }
            // Specializing for strings to avoid the overhead of calling
            // `CultureInfo.CurrentCulture.CompareInfo` for every compare
            // by "caching" this before sorting. Gives a 20-30% performance
            // improvement for the 9 digit string test case on Windows.
            // NOTE: Sorting based on ordinals is faster than default compare.
            else if (typeof(TKey) == typeof(string))
            {
                ref var specificKeys = ref Unsafe.As<TKey, string>(ref keys);
                var comparer = StringDirectComparer.CreateForCurrentCulture();
                KeysSorter_TDirectComparer<string, StringDirectComparer>
                  .IntroSort(ref specificKeys, length, comparer);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
