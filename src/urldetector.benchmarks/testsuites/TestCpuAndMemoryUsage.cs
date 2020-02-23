using BenchmarkDotNet.Attributes;
using urldetector.detection;
using urldetector.tests.custom;

namespace urldetector.benchmarks.testsuites
{
	[MemoryDiagnoser]
	public class TestCpuAndMemoryUsage
	{
		private static UrlDetectorOptions flags =>
			UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN
			|
			UrlDetectorOptions.BRACKET_MATCH
			|
			UrlDetectorOptions.Default
			|
			UrlDetectorOptions.HTML
			|
			UrlDetectorOptions.JAVASCRIPT
			|
			UrlDetectorOptions.JSON
			|
			UrlDetectorOptions.QUOTE_MATCH
			|
			UrlDetectorOptions.SINGLE_QUOTE_MATCH
			|
			UrlDetectorOptions.XML
		;

		[GlobalSetup]
		public void SetupTestData()
		{
			InputTextTiny = TestDataHelper.LoadTinyHtmlFile();
			InputTextSmall = TestDataHelper.LoadSmallHtmlFile();
			InputTextMedium = TestDataHelper.LoadMediumHtmlFile();
			InputTextLarge = TestDataHelper.LoadLargeHtmlFile();
		}

		[Benchmark]
		public void TestOnTinyAmountOfText()
		{
			var links = new UrlDetector(InputTextTiny, flags).Detect();
		}

		public string InputTextTiny { get; set; }

		[Benchmark]
		public void TestOnSmallAmountOfText()
		{
			var links = new UrlDetector(InputTextSmall, flags).Detect();
		}

		public string InputTextSmall { get; set; }


		[Benchmark]
		public void TestOnMediumAmountOfText()
		{
			var links = new UrlDetector(InputTextMedium, flags).Detect();
		}

		public string InputTextMedium { get; set; }

		[Benchmark]
		public void TestOnLargeAmountOfText()
		{
			var links = new UrlDetector(InputTextLarge, flags).Detect();
		}

		public string InputTextLarge { get; set; }
	}
}
