using System.Runtime.CompilerServices;
using BenchmarkDotNet.Running;

namespace DotNetCross.Sorting.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<IntPtrHelperBenchmark>();
            //BenchmarkRunner.Run<NewRandomSort>();
            //BenchmarkRunner.Run<RandomSort>();
            //BenchmarkRunner.Run<MedianOfThreeKillerSort>();
            //BenchmarkRunner.Run<CompareAsm>();

            SomeMethod();
            //var sut = new RandomSort();
            //for (int i = 0; i < 150; i++)
            //{
            //    sut.IterationSetup();
            //    sut.SpanSort();
            //}
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void SomeMethod()
        {
            var s = new RandomSort();
            s.SpanSort();
        }
    }
}
