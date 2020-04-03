using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using DotNetCross.Sorting;

namespace DotNetCross.Sorting.Benchmarks
{
    // https://github.com/KirillOsenkov/Benchmarks/blob/e9b3af701eb4bf98a598b866cc252349b50819e3/Benchmarks/Tests/SortDictionary.cs
    // https://gist.github.com/nietras/eb7bec7803e0e9c4e0e865a2e70ff254
    [MemoryDiagnoser]
    public class SortDictionary
    {
        private readonly Dictionary<string, string> dictionary = new Dictionary<string, string>
        {
            { "DestinationSubPath", "ICSharpCode.Decompiler.dll" },
            { "NuGetPackageId", "ICSharpCode.Decompiler" },
            { "AssetType", "runtime" },
            { "PackageVersion", "5.0.2.5153" },
            { "PackageName", "ICSharpCode.Decompiler" },
            { "NuGetPackageVersion", "5.0.2.5153" },
            { "CopyLocal", "true" },
            { "PathInPackage", "lib/netstandard2.0/ICSharpCode.Decompiler.dll" },
        };

        [Benchmark(Baseline = true)]
        public void OrderBy()
        {
            var result = dictionary.OrderBy(kvp => kvp.Key);
            foreach (var item in result)
            {
            }
        }

        private int Comparer((string, string) left, (string, string) right)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(left.Item1, right.Item1);
        }

        [Benchmark]
        public void SortInPlaceMethodGroup()
        {
            var list = new List<(string key, string value)>(dictionary.Count);

            foreach (var kvp in dictionary)
            {
                list.Add((kvp.Key, kvp.Value));
            }

            list.Sort(Comparer);

            foreach (var kvp in list)
            {
            }
        }

        [Benchmark]
        public void SortInPlaceLambda()
        {
            var list = new List<(string key, string value)>(dictionary.Count);

            foreach (var kvp in dictionary)
            {
                list.Add((kvp.Key, kvp.Value));
            }

            list.Sort((l, r) => StringComparer.OrdinalIgnoreCase.Compare(l.key, r.key));

            foreach (var kvp in list)
            {
            }
        }

        [Benchmark]
        public void IntroSortInPlaceLambda()
        {
            Span<(string key, string value)> array = new (string key, string value)[dictionary.Count];

            int i = 0;
            foreach (var kvp in dictionary)
            {
                array[i] = (kvp.Key, kvp.Value);
                ++i;
            }
            // DotNetCross.Sorting preview :)
            array.IntroSort((l, r) => StringComparer.OrdinalIgnoreCase.Compare(l.key, r.key));

            foreach (var kvp in array)
            {
            }
        }

        [Benchmark]
        public void IntroSortInPlaceStruct()
        {
            Span<(string key, string value)> array = new (string key, string value)[dictionary.Count];

            int i = 0;
            foreach (var kvp in dictionary)
            {
                array[i] = (kvp.Key, kvp.Value);
                ++i;
            }
            // DotNetCross.Sorting preview :)
            array.IntroSort(new OrdinalComparer());

            foreach (var kvp in array)
            {
            }
        }

        public readonly struct OrdinalComparer : IComparer<(string key, string value)>
        {
            public int Compare((string key, string value) x, (string key, string value) y) =>
                string.Compare(x.key, y.key, StringComparison.OrdinalIgnoreCase);
        }
    }
}
