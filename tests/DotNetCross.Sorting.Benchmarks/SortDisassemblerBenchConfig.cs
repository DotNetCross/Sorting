using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace DotNetCross.Sorting.Benchmarks
{
    public class SortDisassemblerBenchConfig : ManualConfig
    {
        public SortDisassemblerBenchConfig()
        {
            var runMode = new BenchmarkDotNet.Jobs.RunMode() { LaunchCount = 1, WarmupCount = 1, /*TargetCount = 3,*/ RunStrategy = RunStrategy.Monitoring };
            var envModes = new[] {
                //new EnvMode { Runtime = Runtime.Core, Platform = Platform.X86 },
                new EnvironmentMode { Runtime = Runtime.Core, Platform = Platform.X64 },
                //new EnvMode { Runtime = Runtime.Clr, Platform = Platform.X86 },
                //new EnvMode { Runtime = Runtime.Clr, Platform = Platform.X64 },
            };
            foreach (var envMode in envModes)
            {
                Add(new Job(envMode, Job.Dry, runMode));
            }
            Add(DisassemblyDiagnoser.Create(
                new DisassemblyDiagnoserConfig(printAsm: true, printSource: true, recursiveDepth: 5)));
           
            //Add(DisassemblyDiagnoser.Create(
            //    new DisassemblyDiagnoserConfig(printAsm: true, printPrologAndEpilog: true, printSource: true, recursiveDepth: 3)));

        }
    }
}
