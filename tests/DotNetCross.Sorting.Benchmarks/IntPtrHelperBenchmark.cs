using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

namespace DotNetCross.Sorting.Benchmarks
{
    [DisassemblyDiagnoser(maxDepth: 2)]
    [SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 2, targetCount: 11)]
    public class IntPtrHelperBenchmark
    {
        [Benchmark]
        public IntPtr Ctor()
        {
            return new IntPtr(42).Multiply(4);
        }
    }
}
