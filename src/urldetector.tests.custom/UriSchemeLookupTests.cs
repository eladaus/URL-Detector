using System.Collections.Generic;
using urldetector.detection;
using urldetector.eladaus;
using Xunit;

namespace urldetector.tests.custom
{
	public class UriSchemeLookupTests
	{
		[Fact]
		public void TestUriSchemeLocators()
		{
			foreach (var schemeName in UriSchemeLookup.UriSchemeNames)
			{
				var urlToFind1 = $"{schemeName}://mytestsite.com";
				var urlToFind2 = $"{schemeName}%3a//othersite.org";
				var inputText = $"did we @>> << !!://JK find #4jadsfj the url: {urlToFind1} and this one too {urlToFind2} ?";
				var urlDetector = new UrlDetector(inputText, UrlDetectorOptions.HTML, new HashSet<string> { schemeName });
				var urls = urlDetector.Detect();
				urls.ForEach(u => u.GetScheme());
				urls.ForEach(u => u.GetHost());
				Assert.Equal(2, urls.Count);

			}
		}
	}
}