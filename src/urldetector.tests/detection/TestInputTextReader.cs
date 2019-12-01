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
			Assert.Equal(reader.Read(), CONTENT[0]);
			reader.GoBack();
			Assert.Equal(reader.Read(), CONTENT[0]);
			Assert.Equal(reader.Read(), CONTENT[1]);
			Assert.Equal(reader.Read(), CONTENT[2]);
			reader.GoBack();
			reader.GoBack();
			Assert.Equal(reader.Read(), CONTENT[1]);
			Assert.Equal(reader.Read(), CONTENT[2]);
		}

		[Fact]
		public void TestSeek()
		{
			var reader = new InputTextReader(CONTENT);
			reader.Seek(4);
			Assert.Equal(reader.Read(), CONTENT[4]);

			reader.Seek(1);
			Assert.Equal(reader.Read(), CONTENT[1]);
		}

		[Fact]
		public void TestSimpleRead()
		{
			var reader = new InputTextReader(CONTENT);
			for (var i = 0; i < CONTENT.Length; i++)
			{
				Assert.Equal(reader.Read(), CONTENT[i]);
			}
		}
	}
}