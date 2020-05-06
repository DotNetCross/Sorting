# API

```csharp
public static class Sorts
{
    public static void IntroSort<T>(this Span<T> span);
    
    public static void IntroSort<T, TComparer>(this Span<T> span, TComparer comparer)
       where TComparer : IComparer<T>;
    
    public static void IntroSort<T>(this Span<T> span, Comparison<T> comparison);
    
    public static void IntroSort<TKey, TValue>(this Span<TKey> keys, Span<TValue> items);
    
    public static void IntroSort<TKey, TValue, TComparer>(this Span<TKey> keys,
       Span<TValue> items, TComparer comparer)
       where TComparer : IComparer<TKey>;
    
    public static void IntroSort<TKey, TValue>(this Span<TKey> keys,
       Span<TValue> items, System.Comparison<TKey> comparison);
}
```



# PR TEXT

Implements https://github.com/dotnet/corefx/issues/15329

**WIP**: This is still very much a work-in-progress. Warts and all! :)

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
`Array.Sort` is implemented as both managed code and native code (for some primitives) in:

- https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Array.cs#L1593 (and following lines)
- https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Array.cs#L1860 (non-generic helper types start here)
- https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs (generic helper types - many variants)
- https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp#L268 (native `TrySZSort`)
- https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.h#L128 (actual algorithm is in header)

#### Base and Variant Differences
This PR is based mainly on the native implementation and the generic implementation.
In retrospective I believed the different variants of Array.Sort would sort identically, but
they do not. The fact that I started out with the native implementation and focused on 
primitives and the specialization of these, appears to have been a mistake.

The fact is `Array.Sort` can yield different sorting results depending on the variant used
when also sorting items (also called values, since the items used for sorting are then keys). 
Note that the sort is still correct, it is just that equal keys can have different results
for where the items are. I.e. here is an example that comes from a special test:

[INSERT IMAGE FROM WINMERGE]

#### Minor Bug Fix
Minor bug fix for introspective depth limit, see https://github.com/dotnet/coreclr/pull/16002

#### Code Structure 
Code is currently, probably temporary, structured into multiple files 
to allow for easier comparison of the different variants. 

Consolidated (most likely will be removed):
- `SpanSortHelpers.KeysAndOrValues.cs`
  - This is my futile attempt to consolidate the different variants into a single code base.
  - I originally had a plan involving `ref struct` and value type generic arguments to ensure
    this would could be done with neglible performance impact but a few things were in the way:
    - https://github.com/dotnet/roslyn/issues/20226
    - `ref struct`s can't contain `ref`s
  - Proposals to improve related to this:
    - https://github.com/dotnet/csharplang/issues/1147
    - https://github.com/dotnet/csharplang/issues/1148
  - And another proposal that would help:
    - https://github.com/dotnet/csharplang/issues/905
  - I was hoping for this to allow for "injecting" values or not with minimal overhead...
    but probably a bit too C++ template like.
  - I even tried referencing [`System.Ben`](https://github.com/benaadams/System.Ben) to see if it helped ;) 

Current:
- `SpanSortHelpers.Common.cs`
  - A few commonly used constants, types and methods e.g. `Swap`.
- `SpanSortHelpers.Keys.cs`
  - Entry points and dispatch types for single span sort.
- `SpanSortHelpers.Keys.IComparable.cs`
  - IComparable variant
- `SpanSortHelpers.Keys.Specialized.cs`
  - Specialized switch for fast primitive versions and a little code sharing.
- `SpanSortHelpers.Keys.TComparer.cs`
  - TComparer variant, used for specialized too.
- `SpanSortHelpers.KeysValues.cs`
  - Entry points and dispatch types for two span sort (e.g. with items/values).
- `SpanSortHelpers.KeysValues.IComparable.cs`
  - IComparable variant
- `SpanSortHelpers.KeysValues.Specialized.cs`
  - Specialized switch for fast primitive versions and a little code sharing.
- `SpanSortHelpers.KeysValues.TComparer.cs`
  - TComparer variant, used for specialized too.

Primary development was done in my DotNetCross.Sorting repo:
https://github.com/DotNetCross/Sorting/
This was to get a better feedback loop and to use BenchmarkDotNet for benchmark testing
and disassembly.

NOTE: I understand that in `corefx` we might want to consolidate this into a single file,
but for now it is easier when comparing variants. Note also that the `coreclr` has
more variants than currently in this PR, there is for example a variant for `Comparison<T>`
in `coreclr`.

#### Changes
Many small changes have been made due to usings refs and Unsafe, but a couple notable changes are: 
- `Sort3` add a specific implementation for sorting three, used both for finding pivot and when sorting exactly 3 elements. 
  - This has currently been changed to use `Sort2` three times like `coreclr` as otherwise,
    there would be differences for some same key cases. That I haven't had time to debug.
  - Why use a special `Sort3`? For expensive comparisons this can be a big improvement
    if keys are almost already sorted, since only 2 compares are needed instead of 3.
- Remove unnecessary ifs on swaps, except for a few places where it is now explicit. 
- A few renames such as `Sort2` instead of `SwapIfGreater`. 
- Comparer based variant uses a specific `IDirectComparer` allowing for the kind of 
  specialization for basic types that `coreclr` does in native code.
  This started out as just a `LessThan` method but the problem is then
  whether bogus comparers should yield the same result as `Array.Sort` so I
  changed this to have more methods.

#### Benchmarks
Since `Sort` is an in-place operation it is "destructive" and benchmarking is done a bit different than normal.
The basic code for benchmarks is shown below, this uses https://github.com/dotnet/BenchmarkDotNet and was
done in the https://github.com/DotNetCross/Sorting/ git repo. Porting these to
the normal `corefx` performance tests is on the TODO list.

The benchmarks use a `Filler` to pre-fill a `_filled` array
with a pattern in slices of `Length`. Depending on the filler the full array will then be filled
with either repeating patterns of `Length` or fill the entire array with a given pattern.

This allows using this for testing sorts of different lengths, but measures the total time to do a number of sorts, 
so it includes the overhead of slicing and the loop. And allows testing different patterns/sequnces.
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
Results for this for specific commits will come in comments later.

##### Fillers
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

For each different type, the `int` value is converted to the given type 
e.g. using `ToString("D9")` for `string`.

These fillers are also used for test case generation. But are combined with other 
sort case generators.


#### Difference to BinarySearch
`BinarySearch` only has an overload without comparer for when `T : IComparable<T>`.
That is, there is no overload where the value searched for is not not
generically constrained.

 - Proposal https://github.com/dotnet/corefx/issues/15818
 - Implementation https://github.com/dotnet/corefx/pull/25777

This is unlike `Sort` where there is no generic constraint on the key type.
If we had https://github.com/dotnet/csharplang/issues/905 this might not be
that big an issue, but we might expect issues for some uses of `BinarySearch`.

#### Tests
The fundamental principle of the tests are they use `Array.Sort` for generating
the expected output. The idea being span `Sort` should give exactly the same result.

#### Notes and TODOs
Biggest problem currently is that there are differences for some specific test cases:
- NaN for float and double will not give the same result when sorting keys **and** values.
  - I have not had time to debug this. In span Sort I wanted to use
  the specialized sort path for these, but `coreclr` only does that when items/values
  are of the same type. This means `coreclr` does not do a `NaNPrepass` for
  floating point types when items are not of same type.
  - I removed this then, but results are still different.
- BogusComparable tests fail since `Array.Sort` will throw on these for **some** lengths.
  - Tests need to be modified to handle this, and span Sort must detect this issue and throw.

There are a lot of other remaining TODOs e.g.:

- Better separation between fast and slow (OuterLoop) tests, currently tests are slow, even the "fast" ones. 
- Port performance tests as per `corefx` standard.
  - Need to determine scope of these... a lot can be added.

Performance is currently on par or significantly better than `coreclr` for value types. As soon
as reference types are used performance is... well miserably. This probably reflects the fact that
my main focus was on optimizing for `int`s. I believe the issue here must be around how I have factored
the generic code.

- Are the changes I have made acceptable? 
- Are there any issues with how I have factored the code into the different types and generic methods?
- Any way to make reference type code faster?
  - Is there any way one can circumvent the canonical representation of generic types and methods
when a type is a reference type? So we can avoid `JIT_GenericHandleMethod` or `JIT_GenericHandleClass`, which shows
up during profiling? This is the reason for the `IComparable` variant of the code... but it is still slow.

