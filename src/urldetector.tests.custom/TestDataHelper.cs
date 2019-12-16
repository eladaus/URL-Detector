using System.IO;

namespace urldetector.tests.custom
{
	public static class TestDataHelper
	{
		public static string LoadSmallHtmlFile()
		{
			return File.ReadAllText(@"samples\small.html");
		}

		public static string LoadLargeHtmlFile()
		{
			return File.ReadAllText(@"samples\large.html");
		}

		public static string LoadMediumHtmlFile()
		{
			return File.ReadAllText(@"samples\medium.html");
		}

		public static string LoadTinyHtmlFile()
		{
			return File.ReadAllText(@"samples\tiny.html");
		}
	}
}