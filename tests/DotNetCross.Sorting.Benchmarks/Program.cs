using BenchmarkDotNet.Running;

namespace DotNetCross.Sorting.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //var s1 = BenchmarkRunner.Run<MedianOfThreeKillerSort>();
            //var s2 = BenchmarkRunner.Run<RandomSort>();
            var s2 = BenchmarkRunner.Run<CompareAsm>();

            //var sut = new RandomSort();
            //for (int i = 0; i < 150; i++)
            //{
            //    sut.IterationSetup();
            //    sut.SpanSort();
            //}
        }
    }
}
