namespace DotNetCross.Sorting
{
    internal static partial class KeysSorter_TDirectComparer<TKey, TComparer>
        where TComparer : IDirectComparer<TKey>
    { }

    internal static partial class KeysSorter_TDirectComparer
    { 
        internal static void IntroSort<TKey, TComparer>(ref TKey keys, int length, TComparer comparer)
            where TComparer : IDirectComparer<TKey>
        {
            KeysSorter_TDirectComparer<TKey, TComparer>.IntroSort(ref keys, length, comparer);
        }
    }
}