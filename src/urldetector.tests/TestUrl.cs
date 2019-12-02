using System.Collections.Generic;
using Xunit;

namespace urldetector.tests
{
	public class TestUrl
	{
		public static IEnumerable<object[]> GetUsernamePasswordUrls =>
			new List<object[]>
			{
				new object[] {"http://www.google.com/", "www.google.com", "/", "", ""},
				new object[] {"nooooo:password@teeee.com", "teeee.com", "/", "nooooo", "password"},
				new object[] {"hello:ono@bob.com/lala.html", "bob.com", "/lala.html", "hello", "ono"},
				new object[] {"lala:asdfjdj1k@bob.com", "bob.com", "/", "lala", "asdfjdj1k"},
				new object[] {"sdf@bob.com", "bob.com", "/", "sdf", ""},
				new object[] {"@www.google.com", "www.google.com", "/", "", ""},
				new object[] {"lalal:@www.gogo.com", "www.gogo.com", "/", "lalal", ""},
				new object[] {"nono:boo@[::1]", "[::1]", "/", "nono", "boo"},
				new object[] {"nono:boo@yahoo.com/@1234", "yahoo.com", "/@1234", "nono", "boo"},
				new object[] {"big.big.boss@google.com", "google.com", "/", "big.big.boss", ""},
				new object[] {"big.big.boss@013.xxx", "013.xxx", "/", "big.big.boss", ""},
				new object[] {"big.big.boss@0xb02067cz", "0xb02067cz", "/", "big.big.boss", ""}
			};

		public static IEnumerable<object[]> GetPortUrls =>
			new List<object[]>
			{
				new object[] {"http://www.google.com:820", "www.google.com", "/", 820},
				new object[] {"foooo.coo:80", "foooo.coo", "/", 80},
				new object[] {"[::ffff:192.168.1.1]:800", "[::ffff:192.168.1.1]", "/", 800},
				new object[] {"[::1]:900/dodododo", "[::1]", "/dodododo", 900},
				new object[] {"hdh:@[::1]:9/nono", "[::1]", "/nono", 9},
				new object[] {"http://touch.www.linkedin.com:9000", "touch.www.linkedin.com", "/", 9000}
			};


		public static IEnumerable<object[]> GetQueryUrls =>
			new List<object[]>
			{
				new object[] {"http://www.google.com/", "www.google.com", "/", ""},
				new object[] {"www.google.com/lala?here=2", "www.google.com", "/lala", "?here=2"},
				new object[] {"bewp.bop.com/boop?bip=2&bep=3", "bewp.bop.com", "/boop", "?bip=2&bep=3"},
				new object[] {"[fe80::1:192.168.12.3]/nooo?dop=2&wop=4", "[fe80::1:192.168.12.3]", "/nooo", "?dop=2&wop=4"},
				new object[] {"[::1:192.1.1.1]:80/nooo?dop=[::1]&wop=4", "[::1:192.1.1.1]", "/nooo", "?dop=[::1]&wop=4"}
			};


		public static IEnumerable<object[]> GetSchemeUrls =>
			new List<object[]>
			{
				new object[] {"http://www.google.com/", "http", "www.google.com", "/"},
				new object[] {"//www.google.com/", "", "www.google.com", "/"},
				new object[] {"//123825342/", "", "123825342", "/"},
				new object[] {"//hello/", "", "hello", "/"},
				new object[] {"//hello:/", "", "hello", "/"}
			};


		public static IEnumerable<object[]> GetUrlsAndHosts =>
			new List<object[]>
			{
				new object[] {"www.booopp.com:20#fa", "www.booopp.com", "http://www.booopp.com:20/#fa"},
				new object[] {"www.yahooo.com:20?fff#aa", "www.yahooo.com", "http://www.yahooo.com:20/?fff#aa"},
				new object[] {"www.google.com#fa", "www.google.com", "http://www.google.com/#fa"},
				new object[] {"www.google.com?3fd#fa", "www.google.com", "http://www.google.com/?3fd#fa"},
				new object[] {"//www.google.com/", "www.google.com", "//www.google.com/"},
				new object[] {"http://www.google.com/", "www.google.com", "http://www.google.com/"},
				new object[] {"ftp://whosdere:me@google.com/", "google.com", "ftp://whosdere:me@google.com/"},
				new object[] {"ono:doope@fb.net:9090/dhdh", "fb.net", "http://ono:doope@fb.net:9090/dhdh"},
				new object[] {"ono:a@fboo.com:90/dhdh/@1234", "fboo.com", "http://ono:a@fboo.com:90/dhdh/@1234"},
				new object[] {"fbeoo.net:990/dhdeh/@1234", "fbeoo.net", "http://fbeoo.net:990/dhdeh/@1234"},
				new object[] {"fbeoo:@boop.com/dhdeh/@1234?aj=r", "boop.com", "http://fbeoo@boop.com/dhdeh/@1234?aj=r"},
				new object[] {"bloop:@noooo.com/doop/@1234", "noooo.com", "http://bloop@noooo.com/doop/@1234"},
				new object[] {"bah.com/lala/@1234/@dfd@df?@dsf#ono", "bah.com", "http://bah.com/lala/@1234/@dfd@df?@dsf#ono"},
				new object[]
				{
					"https://dewd:dood@www.google.com:20/?why=is&this=test#?@Sdsf", "www.google.com",
					"https://dewd:dood@www.google.com:20/?why=is&this=test#?@Sdsf"
				},
				new object[] {"http://013.xxx/", "013.xxx", "http://013.xxx/"}
			};


		public static IEnumerable<object[]> GetSingleDomainUrls =>
			new List<object[]>
			{
				new object[] {"localhost:9000/", "localhost", 9000, "http://localhost:9000/"},
				new object[] {"go/tj", "go", 80, "http://go/tj"}
			};


		[Theory]
		[MemberData(nameof(GetUsernamePasswordUrls))]
		public void TestUsernamePasswordUrls(string testInput, string host, string path, string username, string password)
		{
			var url = Url.Create(testInput);
			Assert.Equal(url.GetHost(), host);
			Assert.Equal(url.GetPath(), path);
			Assert.Equal(url.GetUsername(), username);
			Assert.Equal(url.GetPassword(), password);
		}


		[Theory]
		[MemberData(nameof(GetPortUrls))]
		public void TestPort(string testInput, string host, string path, int port)
		{
			var url = Url.Create(testInput);
			Assert.Equal(url.GetHost(), host);
			Assert.Equal(url.GetPath(), path);
			Assert.Equal(url.GetPort(), port);
		}


		[Theory]
		[MemberData(nameof(GetQueryUrls))]
		public void TestQuery(string testInput, string host, string path, string query)
		{
			var url = Url.Create(testInput);
			Assert.Equal(url.GetHost(), host);
			Assert.Equal(url.GetPath(), path);
			Assert.Equal(url.GetQuery(), query);
		}


		[Theory]
		[MemberData(nameof(GetSchemeUrls))]
		public void TestScheme(string testInput, string scheme, string host, string path)
		{
			var url = Url.Create(testInput);
			Assert.Equal(url.GetScheme(), scheme);
			Assert.Equal(url.GetHost(), host);
			Assert.Equal(url.GetPath(), path);
		}


		[Theory]
		[MemberData(nameof(GetUrlsAndHosts))]
		public void TestHostAndFullUrl(string testInput, string host, string fullUrl) 
		{
			var url = Url.Create(testInput);
			Assert.Equal(url.GetHost(), host /*, testInput*/);
			Assert.Equal(url.GetFullUrl(), fullUrl);
			var fragmentIndex = fullUrl.IndexOf("#");
			Assert.Equal(url.GetFullUrlWithoutFragment(),
				fragmentIndex == -1 ? fullUrl : fullUrl.Substring(0, fragmentIndex));
		}


		[Theory]
		[MemberData(nameof(GetSingleDomainUrls))]
		public void TestSingleDomainUrls(string testInput, string host, int port, string fullUrl)
		{
			var url = Url.Create(testInput);
			Assert.Equal(url.GetHost(), host);
			Assert.Equal(url.GetPort(), port);
			Assert.Equal(url.GetFullUrl(), fullUrl);
		}
	}
}