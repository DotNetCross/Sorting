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



# PR TEXT

Implements https://github.com/dotnet/corefx/issues/15329

#### Goals
- Port `coreclr` `Array.Sort` code without major changes to the algorithm. 
  That is, it is still Introspective Sort using median-of-three quick sort and heap sort.
- Performance on par with `Array.Sort`.
- Define performance tests that can be used for future new algorithms exploration 

#### Non-Goals
- No new algorithms such as say using histograms for byte or similar.
- Generally no changes to the overall algorithm, the idea being
  for this PR to hopefully be accepted without too much controversy :)

#### Array.Sort
Array.Sort is implemented as both managed code and native code (for some primitives) in:

- https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Array.cs#L1593 (and following lines)
- https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Array.cs#L1860 (non-generic helper types start here)
- https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs (generic helper types - many variants)
- https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp#L268 (native `TrySZSort`)
- https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.h#L128 (actual algorithm is in header)

This PR is based mainly on the generic implementation and the native implementation.

# Minor Bug Fix
Minor bug fix for introspective depth limit, see https://github.com/dotnet/coreclr/pull/16002

# Code Structure 
Code is currently, probably temporary, structured into multiple files to allow for easier comparison of the different variants. 

MENTION other repo

# Changes
Many small changes have been made due to usings refs and Unsafe, but a couple notable changes are: 
- Sort3 add a specific implementation for sorting three, used both for finding pivot and when sorting exactly 3 elements. 
- Remove unnecessary ifs on swaps, except for one place where it is now explicit. 
- A few renames such as Sort2 instead of SwapIfGreater. 
- Comparer based variant uses a specific ILessThanComparer allowing for better specialization for basic types. 

# Benchmarks
Since `Sort` is an in-place operation it is "destructive" and benchmarking is done a bit different than normal.
The basic code for benchmarks is shown below, this uses https://github.com/dotnet/BenchmarkDotNet and was
done in the https://github.com/DotNetCross/Sorting/tree/span-sort git repo. Porting these to
the normal `corefx` performance tests is on the TODO list.

The benchmarks use a `Filler` to pre-fill a `_filled` array
with a pattern in slices of `Length`. Depending on the filler the full array will then be filled
with either repeating patterns of `Length` or fill the entire array with a given pattern.

This allows using this for testing sorts of different lengths, but measures the total time to do a number of sorts, 
so it include the overhead of slicing and the loop. And allows testing different patterns/sequnces.
The main point here is that this is used to compare **relatively** to the `Array.Sort`.

```csharp
[GlobalSetup]
public void GlobalSetup()
{
    Filler.Fill(_filled, Length, _toValue);
}

[IterationSetup]
public void IterationSetup()
{
    Array.Copy(_filled, _work, _maxLength);
}

[Benchmark(Baseline = true)]
public void ArraySort()
{
    for (int i = 0; i <= _maxLength - Length; i += Length)
    {
        Array.Sort(_work, i, Length);
    }
}

[Benchmark]
public void SpanSort()
{
    for (int i = 0; i <= _maxLength - Length; i += Length)
    {
        new Span<TKey>(_work, i, Length).Sort();
    }
}
```

## Fillers
As noted there are different fillers. Som fill the entire array not caring about the slice length.
Others fill based on slice length. This is particularly important for **MedianOfThreeKiller**.

- **Constant** one constant value, e. g. 42, note that this means a single reference type. 
- **Decrementing** full array filled with a decreasing sequence from array `length - 1` to `0`.
- **Incrementing** full array filled with a increasing sequence from `0` to `length - 1`.
- **MedianOfThreeKiller** each slice filled with a sequence as detailed in [Introspective Sorting and Selection Algorithms](http://citeseerx.ist.psu.edu/viewdoc/summary?doi=10.1.1.14.5196). For longer slice lengths this will use heap sort. 
- **Random** full array filled with random values. Seeded so each run is the same. 
- **PartialRandomShuffle** full array filled with incrementing sequence. 
Random pairs are then swapped, as a ratio of length. In this case 10%, 
i.e. 10% pairs have been swapped randomly. Seeded so each run is the same.

# TODOs
Overall, biggest to-do are tests. Feedback on whether it is OK to use Array.Sort as ground truth is needed. 

How can we succinctly define the many test cases that are needed?

- Tests, tests tests. 
  - Each specialized path needs tests. 
  - More error condition tests.
  - Better separation between fast and slow (OuterLoop) tests, currently tests are slow. 
    - Need to know what lengths need testing for fast tests? 50 like in coreclr?
- Add performance tests as per `corefx` standard.