using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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


	public class SpanTests
	{
		private Memory<char> SomeMemory { get; set; }

		private Span<char> SomeSpan { get; set; }

		[Fact]
		public void TestIdea()
		{
			SomeMemory = new Memory<char>();
			SomeMemory = "my big test".ToCharArray();

			
			DoHackery();

			Assert.Equal("my big test::some more text", SomeMemory.ToString());

			DoRemoveHackery();
		}

		public void DoHackery()
		{
			var extraText = "::some more text";

			// First allocate the buffer with the target size
			char[] buffer = new char[SomeMemory.Length + extraText.Length];
			// "Convert" it to writable Span<char>
			var spanBuffer = new Span<char>(buffer);

			// Then copy each span at the right position in the buffer
			int index = 0;
			SomeMemory.Span.CopyTo(spanBuffer.Slice(index, SomeMemory.Length));
			index += SomeMemory.Length;

			extraText.AsSpan().CopyTo(spanBuffer.Slice(index, extraText.Length));
			index += extraText.Length;

			SomeMemory = spanBuffer.ToArray();
		}

		private void DoRemoveHackery()
		{
			//var removeText
		}
	}
}