# API

```csharp
public static void Sort<T>(this Span<T> span);

public static void Sort<T, TComparer>(this Span<T> span, TComparer comparer)
   where TComparer : IComparer<T>;

public static void Sort<T>(this Span<T> span, System.Comparison<T> comparison);

public static void Sort<TKey, TValue>(this Span<TKey> keys, Span<TValue> items);

public static void Sort<TKey, TValue, TComparer>(this Span<TKey> keys,
   Span<TValue> items, TComparer comparer)
   where TComparer : IComparer<TKey>;

public static void Sort<TKey, TValue>(this Span<TKey> keys,
   Span<TValue> items, System.Comparison<TKey> comparison);
```