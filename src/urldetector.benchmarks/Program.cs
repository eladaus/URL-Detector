using System;
using System.Globalization;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;
using Perfolizer.Metrology;

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
			Console.WriteLine("Hello BenchMarker!");

			RunBenchmarks(args);
		}


		static void RunBenchmarks(string[] args)
		{
			var configuration = new ManualConfig()
			{
				SummaryStyle = new SummaryStyle(
                    CultureInfo.CurrentCulture, 
					printUnitsInHeader: true,
					sizeUnit: SizeUnit.KB,
					timeUnit: TimeUnit.Millisecond,
					printUnitsInContent: true,
					printZeroValuesInContent: true)
			};

			configuration.AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
			configuration.AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
			configuration.AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());

			// Run the benchmarks
			var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, configuration);
		}
	}
}
