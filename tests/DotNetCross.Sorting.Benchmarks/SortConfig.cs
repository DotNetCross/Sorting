﻿using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;

namespace DotNetCross.Sorting.Benchmarks
{
    public class SortBenchmarkConfig : ManualConfig
    {
        public SortBenchmarkConfig()
        {
            var runMode = new BenchmarkDotNet.Jobs.RunMode() { LaunchCount = 1, WarmupCount = 3, TargetCount = 11, RunStrategy = RunStrategy.Monitoring };
            var envModes = new[] {
                // NOTE: None of the other platforms work...
                //new EnvMode { Runtime = Runtime.Core, Platform = Platform.X86 },
                new EnvMode { Runtime = Runtime.Core, Platform = Platform.X64 },
                //new EnvMode { Runtime = Runtime.Clr, Platform = Platform.X86 },
                //new EnvMode { Runtime = Runtime.Clr, Platform = Platform.X64 },
            };
            foreach (var envMode in envModes)
            {
                Add(new Job(envMode, Job.Dry, runMode)
                    .With(InProcessToolchain.Instance))
                    ;
            }
            //Add(DisassemblyDiagnoser.Create(
            //    new DisassemblyDiagnoserConfig(printAsm: true, printSource: true, recursiveDepth: 3)));
           
            //Add(DisassemblyDiagnoser.Create(
            //    new DisassemblyDiagnoserConfig(printAsm: true, printPrologAndEpilog: true, printSource: true, recursiveDepth: 3)));

        }
    }
}