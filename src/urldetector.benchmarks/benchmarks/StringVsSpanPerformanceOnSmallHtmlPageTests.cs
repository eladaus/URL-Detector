using BenchmarkDotNet.Attributes;
using urldetector.tests.custom;

namespace urldetector.benchmarks.benchmarks
{
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
