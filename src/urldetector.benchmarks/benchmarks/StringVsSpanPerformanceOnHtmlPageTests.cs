using System.Linq;
using BenchmarkDotNet.Attributes;
using urldetector.detection;
using urldetector.legacy.detection;

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

		private static LegacyUrlDetectorOptions legacyFlags =>
			LegacyUrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN
			|
			LegacyUrlDetectorOptions.BRACKET_MATCH
			|
			LegacyUrlDetectorOptions.Default
			|
			LegacyUrlDetectorOptions.HTML
			|
			LegacyUrlDetectorOptions.JAVASCRIPT
			|
			LegacyUrlDetectorOptions.JSON
			|
			LegacyUrlDetectorOptions.QUOTE_MATCH
			|
			LegacyUrlDetectorOptions.SINGLE_QUOTE_MATCH
			|
			LegacyUrlDetectorOptions.XML
		;

		protected string InputText;


		[Benchmark]
		public void LegacyUrlParser_StringAndStringBuilder_LazyParse()
		{
			var links = new LegacyUrlDetector(InputText, legacyFlags).Detect();
		}


		[Benchmark]
		public void UrlParser_SpanAndValueBuilder_LazyParse()
		{
			var links = new UrlDetector(InputText, flags).Detect();
		}
/*

		[Benchmark]
		public void LegacyUrlParser_StringAndStringBuilder_FullProcessing()
		{
			var links = new LegacyUrlDetector(InputText, legacyFlags).Detect().Select(x => new
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
		}*/
	}
}