using System.Linq;
using BenchmarkDotNet.Attributes;
using urldetector.detection;
using urldetector.tests.custom;

namespace urldetector.benchmarks.benchmarks
{
	public abstract class StringVsSpanPerformanceOnHtmlPageTests 
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

		protected string InputText;


		[Benchmark]
		public void UrlParser_StringAndStringBuilder_LazyParse()
		{
			var links = new UrlDetector(InputText, flags).Detect();
		}


		[Benchmark]
		public void UrlParser_SpanAndValueBuilder_LazyParse()
		{
			var links = new UrlDetector(InputText, flags).Detect();
		}

		[Benchmark]
		public void UrlParser_StringAndStringBuilder_FullProcessing()
		{
			var links = new UrlDetector(InputText, flags).Detect().Select(x => new
			{
				Scheme = x.GetScheme(),
				Fragment = x.GetFragment(),
				FullUrl = x.GetFullUrl(),
				FullurlWithoutFragment = x.GetFullUrlWithoutFragment(),
				Host = x.GetHost(),
				Password = x.GetPassword(),
				Username = x.GetUsername(),
				Path = x.GetPath(),
				Port = x.GetPort(),
				Query = x.GetQuery(),
				NormalizedUrl = x.Normalize()
			}).ToList();
		}


		[Benchmark]
		public void UrlParser_SpanAndValueBuilder_FullProcessing()
		{
			var links = new UrlDetector(InputText, flags).Detect().Select(x => new
			{
				Scheme = x.GetScheme(),
				Fragment = x.GetFragment(),
				FullUrl = x.GetFullUrl(),
				FullurlWithoutFragment = x.GetFullUrlWithoutFragment(),
				Host = x.GetHost(),
				Password = x.GetPassword(),
				Username = x.GetUsername(),
				Path = x.GetPath(),
				Port = x.GetPort(),
				Query = x.GetQuery(),
				NormalizedUrl = x.Normalize()
			}).ToList();
		}
	}

	public class StringVsSpanPerformanceOnSmallHtmlPageTests : StringVsSpanPerformanceOnHtmlPageTests
	{

		/// <summary>
		/// Called only once per benchmark method
		/// </summary>
		[GlobalSetup]
		public void SetupTestData()
		{
			InputText = TestDataHelper.LoadSmallHtmlFile();
		}

	}
}
