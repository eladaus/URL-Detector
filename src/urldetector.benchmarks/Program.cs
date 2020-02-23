using System;
using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

//using BenchmarkDotNet.Columns;
//using BenchmarkDotNet.Configs;
//using BenchmarkDotNet.Horology;
//using BenchmarkDotNet.Reports;
//using BenchmarkDotNet.Running;

namespace urldetector.benchmarks
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello Benchmarker!");

			RunBenchmarks(args);
		}


		static void RunBenchmarks(string[] args)
		{
			var configuration = new ManualConfig()
			{
				SummaryStyle = new SummaryStyle(
					printUnitsInHeader: true,
					sizeUnit: SizeUnit.KB,
					timeUnit: TimeUnit.Millisecond,
					printUnitsInContent: true,
					printZeroValuesInContent: true)
			};

			configuration.Add(DefaultConfig.Instance.GetExporters().ToArray());
			configuration.Add(DefaultConfig.Instance.GetLoggers().ToArray());
			configuration.Add(DefaultConfig.Instance.GetColumnProviders().ToArray());

			// Run the benchmarks
			var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, configuration);
		}
	}
}
