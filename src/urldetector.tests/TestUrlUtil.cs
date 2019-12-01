using System.Collections.Generic;
using Xunit;

namespace urldetector.tests
{
	public class TestUrlUtil
	{

		public static IEnumerable<object[]> GetDecodeStrings =>
			new List<object[]>
			{
				new object[] {"%%32%35", "%"},
				new object[] {"%2%35", "%"},
				new object[] {"%%325", "%"},
				new object[] {"%%32%3525", "%"},
				new object[] {"%%%32%35", "%%"},
				new object[] {"%%32%35%", "%%"},
				new object[] {"%%32%3532", "2"},
				new object[] {"%%%32%3532%%32%3535", "%"},
				new object[] {"/%25%32%35", "/%"},
				new object[] {"/%2%2%2", "/%2%2%2"},
				new object[] {"/%2%%335", "/%"},
				new object[] {"/%25%32%35%25%32%35", "/%%"},
				new object[] {"/%2525252525252525", "/%"},
				new object[] {"/asdf%25%32%35asd", "/asdf%asd"},
				new object[] {"/%%%25%32%35asd%%", "/%%%asd%%"},
				new object[] {"/%2E%73%65%63%75%72%65/%77%77%77%2E%65%62%61%79%2E%63%6F%6D/", "/.secure/www.ebay.com/"},
				new object[] {"/uploads/%20%20%20%20/", "/uploads/    /"},
				new object[]
				{
					"/%257Ea%2521b%2540c%2523d%2524e%25f%255E00%252611%252A22%252833%252944_55%252B",
					"/~a!b@c#d$e%f^00&11*22(33)44_55+"
				}
			};


		public static IEnumerable<object[]> GetEncodeStrings =>
			new List<object[]>
			{
				new object[] {"/lnjbk%", "/lnjbk%25"},
				new object[] {"/%2%2%2", "/%252%252%252"}
			};


		public static IEnumerable<object[]> GetExtraDotsStrings =>
			new List<object[]>
			{
				new object[] {".s..ales.....com", "s.ales.com"},
				new object[] {"33r.nEt...", "33r.nEt"},
				new object[] {"[::-34:50]...", "[::-34:50]"},
				new object[] {"asdf.[-34::192.168.34.-3]...", "asdf.[-34::192.168.34.-3]"},
				new object[] {".", ""}
			};


		[Theory] 
		[MemberData(nameof(GetDecodeStrings))]
		public void TestDecode(string input, string expectedDecodedString)
		{
			Assert.Equal(UrlUtil.Decode(input), expectedDecodedString);
		}

		[Theory] 
		[MemberData(nameof(GetEncodeStrings))]
		public void TestEncode(string input, string expectedEncodedString)
		{
			Assert.Equal(UrlUtil.Encode(input), expectedEncodedString);
		}

		[Theory] 
		[MemberData(nameof(GetExtraDotsStrings))]
		public void TestExtraDotsHosts(string input, string expected)
		{
			Assert.Equal(UrlUtil.RemoveExtraDots(input), expected);
		}
	}
}