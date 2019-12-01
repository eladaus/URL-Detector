using System.Collections.Generic;
using Xunit;

namespace urldetector.tests
{
	public class TestNormalizedUrl
	{

		public static IEnumerable<object[]> GetHostPathUrls =>
			new List<object[]>
			{
				new object[] {"http://www.google.com/", "www.google.com", "/"},
				new object[] {"teeee.com", "teeee.com", "/"},
				new object[] {"[::1]", "[::1]", "/"},
				new object[] {"yahoo.com/@1234", "yahoo.com", "/@1234"},
				new object[] {"http://[::0xfe.07.23.33]/%25%32%35", "[::fe07:1721]", "/%25"},
				new object[] {"http://host.com/%2525252525252525", "host.com", "/%25"},
				new object[] {"http://[::1]/asdf%25%32%35asd", "[::1]", "/asdf%25asd"},
				new object[] {"http://[::10]/%%%25%32%35asd%%", "[::10]", "/%25%25%25asd%25%25"},
				new object[] {"343324381/", "20.118.182.221", "/"}
			};


		/// <summary>
		/// https://developers.google.com/safe-browsing/developers_guide_v3#Canonicalization
		/// </summary>
		public static IEnumerable<object[]> GetFullUrls =>
			new List<object[]>
			{
				new object[] {"http://host/%25%32%35", "http://host/%25"},
				new object[] {"http://host/%25%32%35%25%32%35", "http://host/%25%25"},
				new object[] {"http://host/%2525252525252525", "http://host/%25"},
				new object[] {"http://host/asdf%25%32%35asd", "http://host/asdf%25asd"},
				new object[] {"http://host/%%%25%32%35asd%%", "http://host/%25%25%25asd%25%25"},
				new object[] {"http://www.google.com/", "http://www.google.com/"},
				new object[]
				{
					"http://%31%36%38%2e%31%38%38%2e%39%39%2e%32%36/%2E%73%65%63%75%72%65/%77%77%77%2E%65%62%61%79%2E%63%6F%6D/",
					"http://168.188.99.26/.secure/www.ebay.com/"
				},
				new object[] {"http://195.127.0.11/uploads/%20%20%20%20/.verify/.eBaysecure=updateuserdataxplimnbqmn-xplmvalidateinfoswqpcmlx=hgplmcx/",
						"http://195.127.0.11/uploads/%20%20%20%20/.verify/.eBaysecure=updateuserdataxplimnbqmn-xplmvalidateinfoswqpcmlx=hgplmcx/"},
				new object[] {"http://host%23.com/%257Ea%2521b%2540c%2523d%2524e%25f%255E00%252611%252A22%252833%252944_55%252B",
						"http://host%23.com/~a!b@c%23d$e%25f^00&11*22(33)44_55+"},
				new object[] {"http://3279880203/blah", "http://195.127.0.11/blah"},
				new object[] {"http://www.google.com/blah/..", "http://www.google.com/"},
				new object[] {"www.google.com/", "http://www.google.com/"},
				new object[] {"www.google.com", "http://www.google.com/"},
				new object[] {"http://www.evil.com/blah#frag", "http://www.evil.com/blah"},
				new object[] {"http://www.GOOgle.com/", "http://www.google.com/"},
				new object[] {"http://www.google.com/foo\tbar\rbaz\n2", "http://www.google.com/foobarbaz2"},
				new object[] {"http://www.google.com/q?", "http://www.google.com/q?"},
				new object[] {"http://www.google.com/q?r?", "http://www.google.com/q?r?"},
				new object[] {"http://www.google.com/q?r?s", "http://www.google.com/q?r?s"},
				new object[] {"http://evil.com/foo#bar#baz", "http://evil.com/foo"},
				new object[] {"http://evil.com/foo;", "http://evil.com/foo;"},
				new object[] {"http://evil.com/foo?bar;", "http://evil.com/foo?bar;"},
				new object[] {"http://\\x01\\x80.com/", "http://%01%80.com/"},
				new object[] {"http://notrailingslash.com", "http://notrailingslash.com/"},
				new object[] {"http://www.gotaport.com:1234/", "http://www.gotaport.com:1234/"},
				new object[] {"  http://www.google.com/  ", "http://www.google.com/"},
				new object[] {"http:// leadingspace.com/", "http://%20leadingspace.com/"},
				new object[] {"http://%20leadingspace.com/", "http://%20leadingspace.com/"},
				new object[] {"%20leadingspace.com/", "http://%20leadingspace.com/"},
				new object[] {"https://www.securesite.com/", "https://www.securesite.com/"},
				new object[] {"http://host.com/ab%23cd", "http://host.com/ab%23cd"},
				new object[] {"http://host.com//twoslashes?more//slashes", "http://host.com/twoslashes?more//slashes"},
				new object[] {"http://go.co/a/b/../c", "http://go.co/a/c"},
				new object[] {"http://big.big.boss@0xb02067cd/", "http://big.big.boss@176.32.103.205/"},
				new object[] {"http://www.0xb02067cd/", "http://www.0xb02067cd/"},
				new object[] {"http://012.0xb02067cd/", "http://012.0xb02067cd/"}
			};


		[Theory]
		[MemberData(nameof(GetHostPathUrls))]
		public void TestUsernamePasswordUrls(string testString, string host, string path)
		{
			Url url = NormalizedUrl.Create(testString);
			Assert.NotNull(url);
			Assert.Equal(url.GetHost(), host);
			Assert.Equal(url.GetPath(), path);
		}


		[Theory]
		[MemberData(nameof(GetFullUrls))]
		public void TestFullUrls(string host, string expectedHost)
		{
			var normalizedUrl = NormalizedUrl.Create(host);
			var fullUrlWithoutFragment = normalizedUrl.GetFullUrlWithoutFragment();
			Assert.Equal(expectedHost, fullUrlWithoutFragment);
		}
	}
}