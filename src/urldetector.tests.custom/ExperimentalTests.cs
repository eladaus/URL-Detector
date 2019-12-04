using System.Linq;
using urldetector.detection;
using Xunit;

namespace urldetector.tests.custom
{
	public class ExperimentalTests
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
	}
}
