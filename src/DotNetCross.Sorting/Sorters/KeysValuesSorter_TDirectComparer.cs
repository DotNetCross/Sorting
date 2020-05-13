namespace DotNetCross.Sorting
{
    internal static partial class KeysValuesSorter_TDirectComparer<TKey, TValue, TComparer>
        where TComparer : IDirectComparer<TKey>
    { }

    internal static partial class KeysValuesSorter_TDirectComparer
    { 
        internal static void IntroSort<TKey, TValue, TComparer>(
            ref TKey keys, ref TValue values, int length, TComparer comparer)
            where TComparer : IDirectComparer<TKey>
        {
            KeysValuesSorter_TDirectComparer<TKey, TValue, TComparer>
                .IntroSort(ref keys, ref values, length, comparer);
        }
    }
}