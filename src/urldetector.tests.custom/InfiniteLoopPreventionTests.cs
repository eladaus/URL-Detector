using System.Collections.Generic;
using System.Linq;
using urldetector.detection;
using Xunit;

namespace urldetector.tests.custom
{
	public class InfiniteLoopPreventionTests
	{
		[Fact]
		public void TestStuff()
		{
			var testInput = @" some new text attachment://hereclickonthis.exe now see? ";
			var urlDetector = new UrlDetector(testInput, UrlDetectorOptions.Default);

			var urls = urlDetector.Detect().Select(u => u.GetFullUrl()).ToList();

		}

		[Fact]
		public void TestStuff2()
		{
			var testInput = @" some new text attachment://hereclickonthis.exe now see? ";
			var urlDetector = new UrlDetector(testInput, UrlDetectorOptions.Default);

			var urls = urlDetector.Detect().Select(u => u.GetFullUrl()).ToList();

		}


		public static IEnumerable<object[]> DataForLinkedInUrlParserFlags =>
			new List<object[]>
			{
				new object[] { UrlDetectorOptions.HTML },
				new object[] { UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN},
				new object[] { UrlDetectorOptions.BRACKET_MATCH },
				new object[] { UrlDetectorOptions.Default },
				new object[] { UrlDetectorOptions.JAVASCRIPT },
				new object[] { UrlDetectorOptions.JSON },
				new object[] { UrlDetectorOptions.QUOTE_MATCH },
				new object[] { UrlDetectorOptions.SINGLE_QUOTE_MATCH },
				new object[] { UrlDetectorOptions.XML },
				new object[]
				{
					UrlDetectorOptions.HTML | UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN | UrlDetectorOptions.BRACKET_MATCH | 
					UrlDetectorOptions.Default | UrlDetectorOptions.JAVASCRIPT | UrlDetectorOptions.QUOTE_MATCH | 
					UrlDetectorOptions.SINGLE_QUOTE_MATCH | UrlDetectorOptions.XML
				},
			};


		/// <summary>
		/// Confirm that know 'infinite loop' string of ' :u ' is now prevented by forced-exit-on-max-character-read check
		/// </summary>
		/// <param name="flags"></param>
		[Theory]
		[MemberData(nameof(DataForLinkedInUrlParserFlags))]
		public void TestForcedExitOfInfiniteLoopReadsDuringUrlDetection(UrlDetectorOptions flags) {
			var sourceText = " :u "; // previously, without character read count logic, this string kills loop in UrlDetector.ReadDefault()
			var links = new UrlDetector(sourceText, flags)
				.Detect();
			Assert.Empty(links);
		}


		[Theory]
		[MemberData(nameof(DataForLinkedInUrlParserFlags))]
		public void TestSingleLevelDomainLocalhostWithPortUrlDetectionAfterMultipleColons(UrlDetectorOptions flags) {
			var sourceText = " l:url(https://localhost:8080/test "; // previously, without character read count logic, this string kills loop in UrlDetector.ReadDefault()
			var links = new UrlDetector(sourceText, flags)
				// BEWARE THE INFINITE LOOP THAT HAPPENS HERE:
				.Detect();
			Assert.True(true);		// Just want to confirm that we made it past the infinite loop
		}

		[Theory]
		[MemberData(nameof(DataForLinkedInUrlParserFlags))]
		public void TestSingleLevelDomainUrlDetectionAfterMultipleColons(UrlDetectorOptions flags) {
			var sourceText = " l:url(https://test/test2 "; // previously, without character read count logic, this string kills loop in UrlDetector.ReadDefault()
			var links = new UrlDetector(sourceText, flags)
				// BEWARE THE INFINITE LOOP THAT HAPPENS HERE:
				.Detect();
			Assert.True(true);		// Just want to confirm that we made it past the infinite loop
		}

		[Theory]
		[MemberData(nameof(DataForLinkedInUrlParserFlags))]
		public void TestSingleLevelDomainUrlWithPortDetectionAfterMultipleColons(UrlDetectorOptions flags) {
			var sourceText = " l:url(https://test:8080/test "; // previously, without character read count logic, this string kills loop in UrlDetector.ReadDefault()
			var links = new UrlDetector(sourceText, flags)
				// BEWARE THE INFINITE LOOP THAT HAPPENS HERE:
				.Detect();
			Assert.True(true);		// Just want to confirm that we made it past the infinite loop
		}


		[Theory]
		[MemberData(nameof(DataForLinkedInUrlParserFlags))]
		public void TestTextDomaniUrlDetectionAfterMultipleColons(UrlDetectorOptions flags) {
			var sourceText = " l:url(https://mytest.com/test ";
			var links = new UrlDetector(sourceText, flags).Detect();
			Assert.Single(links);
			Assert.Equal("https", links[0].GetScheme());
			Assert.Equal("mytest.com", links[0].GetHost());
		}

	}
}
