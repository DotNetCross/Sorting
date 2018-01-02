using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

namespace DotNetCross.Sorting.Benchmarks
{
    [DisassemblyDiagnoser(printAsm: true, printSource: true, recursiveDepth: 3)]
    [SimpleJob(launchCount: 1, warmupCount: 2, targetCount: 11)]
    //[RyuJitX64Job()]
    public class CompareAsm
    {
        ComparableComparer<int> comparer = new ComparableComparer<int>();

        [Params(42)]
        public int Value { get; set; }
        [Params(1000)]
        public int Index { get; set; }
        [Params(256 * 1024)]
        public int Length { get; set; }
        
        [Benchmark]
        public int ComparerCompare()
        {
            //return Value * Index * Length;
            unsafe
            {
                var a = stackalloc int[Length];
                a[0] = Value;
                a[1] = Value + 2;
                ref var refLoMinus1 = ref a[0];
                ref var refLo = ref a[1];
                var i = Index;
                var n = Length;
                while (i <= n)
                {
                    int child = i << 1;
                    //if (child < n && comparer(keys[lo + child - 1], keys[lo + child]) < 0)
                    if (child < n &&
                        comparer.Compare(Unsafe.Add(ref refLoMinus1, child), Unsafe.Add(ref refLo, child)) < 0)
                    {
                        ++child;
                    }

                    i = child;
                }
                return i;
            }
        }
    }

}
