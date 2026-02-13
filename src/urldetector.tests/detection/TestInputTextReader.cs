using urldetector.detection;
using Xunit;

namespace urldetector.tests.detection
{
	public class TestInputTextReader
	{
		private static readonly string CONTENT = "HELLO WORLD";

		[Fact]
		public void TestEof()
		{
			var reader = new InputTextReader(CONTENT);
			for (var i = 0; i < CONTENT.Length - 1; i++)
			{
				reader.Read();
			}

			Assert.False(reader.Eof());
			reader.Read();
			Assert.True(reader.Eof());
		}

		[Fact]
		public void TestGoBack()
		{
			var reader = new InputTextReader(CONTENT);
			Assert.Equal(CONTENT[0], reader.Read());
			reader.GoBack();
			Assert.Equal(CONTENT[0], reader.Read());
			Assert.Equal(CONTENT[1], reader.Read());
			Assert.Equal(CONTENT[2], reader.Read());
			reader.GoBack();
			reader.GoBack();
			Assert.Equal(CONTENT[1], reader.Read());
			Assert.Equal(CONTENT[2], reader.Read());
		}

		[Fact]
		public void TestSeek()
		{
			var reader = new InputTextReader(CONTENT);
			reader.Seek(4);
			Assert.Equal(CONTENT[4], reader.Read());

			reader.Seek(1);
			Assert.Equal(CONTENT[1], reader.Read());
		}

		[Fact]
		public void TestSimpleRead()
		{
			var reader = new InputTextReader(CONTENT);
			for (var i = 0; i < CONTENT.Length; i++)
			{
				Assert.Equal(CONTENT[i], reader.Read());
			}
		}
	}
}
