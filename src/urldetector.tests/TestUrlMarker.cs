using System.Collections.Generic;
using Xunit;

namespace urldetector.tests
{
	public class TestUrlMarker
	{

		public static IEnumerable<object[]> GetUrlMarkers =>
			new List<object[]>
			{
				new object[]
				{
					"hello@hello.com", "http", "hello", "", "hello.com", 80, "/", "", "",
					new[] {-1, 0, 6, -1, -1, -1, -1}
				},
				new object[]
				{
					"http://hello@hello.com", "http", "hello", "", "hello.com", 80, "/", "", "",
					new[] {0, 7, 13, -1, -1, -1, -1}
				},
				new object[]
				{
					"hello@hello.com", "http", "hello", "", "hello.com", 80, "/", "", "",
					new[] {-1, 0, 6, -1, -1, -1, -1}
				},
				new object[]
				{
					"https://user@google.com/h?hello=w#abc", "https", "user", "", "google.com", 443, "/h", "?hello=w", "#abc",
					new[] {0, 8, 13, -1, 23, 25, 33}
				},
				new object[]
				{
					"www.booopp.com:20#fa", "http", "", "", "www.booopp.com", 20, "/", "", "#fa",
					new[] {-1, -1, 0, 15, -1, -1, 17}
				},
				new object[]
				{
					"www.yahooo.com:20?fff#aa", "http", "", "", "www.yahooo.com", 20, "/", "?fff", "#aa",
					new[] {-1, -1, 0, 15, -1, 17, 21}
				},
				new object[]
				{
					"www.google.com#fa", "http", "", "", "www.google.com", 80, "/", "", "#fa",
					new[] {-1, -1, 0, -1, -1, -1, 14}
				},
				new object[]
				{
					"www.google.com?3fd#fa", "http", "", "", "www.google.com", 80, "/", "?3fd", "#fa",
					new[] {-1, -1, 0, -1, -1, 14, 18}
				},
				new object[]
				{
					"//www.google.com/", "", "", "", "www.google.com", -1, "/", "", "",
					new[] {-1, -1, 2, -1, 16, -1, -1}
				},
				new object[]
				{
					"http://www.google.com/", "http", "", "", "www.google.com", 80, "/", "", "",
					new[] {0, -1, 7, -1, 21, -1, -1}
				},
				new object[]
				{
					"ftp://whosdere:me@google.com/", "ftp", "whosdere", "me", "google.com", 21, "/", "", "",
					new[] {0, 6, 18, -1, 28, -1, -1}
				},
				new object[]
				{
					"ono:doope@fb.net:9090/dhdh", "http", "ono", "doope", "fb.net", 9090, "/dhdh", "", "",
					new[] {-1, 0, 10, 17, 21, -1, -1}
				},
				new object[]
				{
					"ono:a@fboo.com:90/dhdh/@1234", "http", "ono", "a", "fboo.com", 90, "/dhdh/@1234", "", "",
					new[] {-1, 0, 6, 15, 17, -1, -1}
				},
				new object[]
				{
					"fbeoo.net:990/dhdeh/@1234", "http", "", "", "fbeoo.net", 990, "/dhdeh/@1234", "", "",
					new[] {-1, -1, 0, 10, 13, -1, -1}
				},
				new object[]
				{
					"fbeoo:@boop.com/dhdeh/@1234?aj=r", "http", "fbeoo", "", "boop.com", 80, "/dhdeh/@1234", "?aj=r", "",
					new[] {-1, 0, 7, -1, 15, 27, -1}
				},
				new object[]
				{
					"bloop:@noooo.com/doop/@1234", "http", "bloop", "", "noooo.com", 80, "/doop/@1234", "", "",
					new[] {-1, 0, 7, -1, 16, -1, -1}
				},
				new object[]
				{
					"bah.com/lala/@1234/@dfd@df?@dsf#ono", "http", "", "", "bah.com", 80, "/lala/@1234/@dfd@df", "?@dsf",
					"#ono", new[] {-1, -1, 0, -1, 7, 26, 31}
				},
				new object[]
				{
					"https://dewd:dood@www.google.com:20/?why=is&this=test#?@Sdsf", "https", "dewd", "dood",
					"www.google.com", 20, "/", "?why=is&this=test", "#?@Sdsf", new[] {0, 8, 18, 33, 35, 36, 53}
				}
			};

		[Theory] 
		[MemberData(nameof(GetUrlMarkers))]
		public void TestUrlMarkers(string testString, string scheme, string username, string password, string host, int port,
			string path, string query, string fragment, int[] indices)
		{
			var urlMarker = new UrlMarker();
			urlMarker.SetOriginalUrl(testString);
			urlMarker.SetIndices(indices);
			var url = urlMarker.CreateUrl();
			Assert.Equal(url.GetHost(), host /*, "host, " + testString*/);
			Assert.Equal(url.GetPath(), path /*, "path, " + testString*/);
			Assert.Equal(url.GetScheme(), scheme /*, "scheme, " + testString*/);
			Assert.Equal(url.GetUsername(), username /*, "username, " + testString*/);
			Assert.Equal(url.GetPassword(), password /*, "password, " + testString*/);
			Assert.Equal(url.GetPort(), port /*, "port, " + testString*/);
			Assert.Equal(url.GetQuery(), query /*, "query, " + testString*/);
			Assert.Equal(url.GetFragment(), fragment /*, "fragment, " + testString*/);
		}
	}
}