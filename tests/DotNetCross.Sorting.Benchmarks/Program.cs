using BenchmarkDotNet.Running;

namespace DotNetCross.Sorting.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<NewRandomSort>();
            BenchmarkRunner.Run<RandomSort>();
            //BenchmarkRunner.Run<MedianOfThreeKillerSort>();
            //BenchmarkRunner.Run<CompareAsm>();

            //var sut = new RandomSort();
            //for (int i = 0; i < 150; i++)
            //{
            //    sut.IterationSetup();
            //    sut.SpanSort();
            //}
        }
    }
}
