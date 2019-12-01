using System.Collections.Generic;
using System.Linq;
using urldetector.detection;
using Xunit;

namespace urldetector.tests.detection
{
	public class TestUriDetection
	{
		//IPv6 Tests


		public static IEnumerable<object[]> GetIPv6ColonsTestStrings =>
			new List<object[]>
			{
				new object[] {"[fe80:aaaa:aaaa:aaaa:3dd0:7f8e:57b7:34d5]"},
				new object[] {"[bcad::aaaa:aaaa:3dd0:7f8e:222.168.1.1]"},
				new object[] {"[bcad::aaaa:aaaa:3dd0:7f8e:57b7:34d5]"},
				new object[] {"[dead::85a3:0:0:8a2e:370:7334]"},
				new object[] {"[::BEEF:0:8a2e:370:7334]"},
				new object[] {"[::beEE:EeEF:0:8a2e:370:7334]"},
				new object[] {"[::]"},
				new object[] {"[0::]"},
				new object[] {"[::1]"},
				new object[] {"[0::1]"}
			};


		[Theory] 
		[MemberData(nameof(GetIPv6ColonsTestStrings))]
		public void TestIPv6Colons(string testString)
		{
			RunTest(testString, UrlDetectorOptions.Default, testString);
			RunTest(" " + testString + " ", UrlDetectorOptions.Default, testString);
			RunTest("bobo" + testString + " ", UrlDetectorOptions.Default, testString);
			RunTest("bobo" + testString + "bobo", UrlDetectorOptions.Default, testString);
			RunTest("bobo " + testString, UrlDetectorOptions.Default, testString);
			RunTest("alkfs:afef:" + testString, UrlDetectorOptions.Default, testString);
		}


		public static IEnumerable<object[]> GetIPv6Ipv4AddressTestStrings =>
			new List<object[]>
			{
				new object[] {"[fe80:aaaa:aaaa:aaaa:3dd0:7f8e:192.168.1.1]", "[fe80:aaaa:aaaa:aaaa:3dd0:7f8e:192.168.1.1]"},
				new object[] {"[bcad::aaaa:aaaa:3dd0:7f8e:222.168.1.1]", "[bcad::aaaa:aaaa:3dd0:7f8e:222.168.1.1]"},
				new object[] {"[dead::85a3:0:0:8a2e:192.168.1.1]", "[dead::85a3:0:0:8a2e:192.168.1.1]"},
				new object[] {"[::BEEF:0:8a2e:192.168.1.1]", "[::BEEF:0:8a2e:192.168.1.1]"},
				new object[] {"[:BAD:BEEF:0:8a2e:192.168.1.1]", "192.168.1.1"},
				new object[] {"[::beEE:EeEF:0:8a2e:192.168.1.1]", "[::beEE:EeEF:0:8a2e:192.168.1.1]"},
				new object[] {"[::192.168.1.1]", "[::192.168.1.1]"},
				new object[] {"[0::192.168.1.1]", "[0::192.168.1.1]"},
				new object[] {"[::ffff:192.168.1.1]", "[::ffff:192.168.1.1]"},
				new object[] {"[0::ffff:192.168.1.1]", "[0::ffff:192.168.1.1]"},
				new object[] {"[0:ffff:192.168.1.1::]", "192.168.1.1"}
			};


		[Theory] 
		[MemberData(nameof(GetIPv6Ipv4AddressTestStrings))]
		public void TestIPv6Ipv4Addresses(string testString, string expectedString)
		{
			RunTest(testString, UrlDetectorOptions.Default, expectedString);
		}

		[Theory] 
		[MemberData(nameof(GetIPv6Ipv4AddressTestStrings))]
		public void TestIPv6Ipv4AddressesWithSpaces(string testString, string expectedString)
		{
			testString += " ";
			RunTest(testString, UrlDetectorOptions.Default, expectedString);
			testString = " " + testString;
			RunTest(testString, UrlDetectorOptions.Default, expectedString);
		}


		public static IEnumerable<object[]> GetHexOctalIpAddresses =>
			new List<object[]>
			{
				new object[] {"http://[::ffff:0xC0.0x00.0x02.0xEB]", "%251"},
				new object[] {"http://[::0301.0250.0002.0353]", "%251"},
				new object[] {"http://[0::ffff:0xC0.0x00.0x02.0xEB]", "%223"},
				new object[] {"http://[0::0301.0250.0002.0353]", "%2lalal-a."},
				new object[] {"http://[::bad:ffff:0xC0.0x00.0x02.0xEB]", "%---"},
				new object[] {"http://[::bad:ffff:0301.0250.0002.0353]", "%-.-.-.-....-....--"}
			};

		[Theory] 
		[MemberData(nameof(GetHexOctalIpAddresses))]
		public void TestIpv6HexOctalIpAddress(string validUrl, string zoneIndex)
		{
			//these are supported by chrome and safari but not firefox;
			//chrome supports without scheme and safari does not without scheme

			RunTest(validUrl, UrlDetectorOptions.Default, validUrl);
		}

		[Theory] 
		[MemberData(nameof(GetHexOctalIpAddresses))]
		public void TestIpv6ZoneIndices(string address, string zoneIndex)
		{
			//these are not supported by common browsers, but earlier versions of firefox do and future versions may support this
			var validUrl = address.Substring(0, address.Length - 1) + zoneIndex + ']';
			RunTest(validUrl, UrlDetectorOptions.Default, validUrl);
		}

		[Theory] 
		[MemberData(nameof(GetHexOctalIpAddresses))]
		public void TestIpv6ZoneIndicesWithUrlEncodedDots(string address, string zoneIndex)
		{
			//these are not supported by common browsers, but earlier versions of firefox do and future versions may support this
			var tmp = address.Replace(".", "%2e");
			var validUrl = tmp.Substring(0, tmp.Length - 1) + zoneIndex + ']';
			RunTest(validUrl, UrlDetectorOptions.Default, validUrl);
		}

		private void RunTest(string text, UrlDetectorOptions options, params string[] expected)
		{
			//do the detection
			var parser = new UrlDetector(text, options);
			var found = parser.Detect();
			var foundArray = new string[found.Count];
			for (var i = 0; i < foundArray.Length; i++)
			{
				foundArray[i] = found[i].GetOriginalUrl();
			}

			// All expected items found, ordering irrelevant
			var areSame = !expected.Except(foundArray).Any() && expected.Length == foundArray.Length;
			Assert.True(areSame);
		}

		[Fact]
		public void TestBacktrackingEmptyDomainName()
		{
			RunTest("check out my http:///hello", UrlDetectorOptions.Default);
			RunTest("check out my http://./hello", UrlDetectorOptions.Default);
		}

		[Fact]
		public void TestBacktrackingStrangeFormats()
		{
			RunTest("http:http:http://www.google.com www.www:yahoo.com yahoo.com.br hello.hello..hello.com",
				UrlDetectorOptions.Default, "www.www", "hello.hello.", "http://www.google.com", "yahoo.com", "yahoo.com.br",
				"hello.com");
		}

		[Fact]
		public void TestBacktrackingUsernamePassword()
		{
			RunTest("check out my url:www.google.com", UrlDetectorOptions.Default, "www.google.com");
			RunTest("check out my url:www.google.com ", UrlDetectorOptions.Default, "www.google.com");
		}

		[Fact]
		public void TestBacktrackInvalidUsernamePassword()
		{
			RunTest("http://hello:asdf.com", UrlDetectorOptions.Default, "asdf.com");
		}

		[Fact]
		public void TestBasicDetect()
		{
			RunTest("this is a link: www.google.com", UrlDetectorOptions.Default, "www.google.com");
		}

		[Fact]
		public void TestBasicHtml()
		{
			RunTest(
				"<script type=\"text/javascript\">var a = 'http://www.abc.com', b=\"www.def.com\"</script><a href=\"http://www.google.com\">google.com</a>",
				UrlDetectorOptions.HTML, "http://www.google.com", "http://www.abc.com", "www.def.com", "google.com");
		}

		[Fact]
		public void TestBasicString()
		{
			RunTest("hello world", UrlDetectorOptions.Default);
		}

		[Fact]
		public void TestBracketMatching()
		{
			RunTest(
				"MY url (www.google.com) is very cool. the domain [www.google.com] is popular and when written like this {www.google.com} it looks like code",
				UrlDetectorOptions.BRACKET_MATCH, "www.google.com", "www.google.com", "www.google.com");
		}

		[Fact]
		public void TestDetectUrlEncoded()
		{
			RunTest("%77%77%77%2e%67%75%6d%62%6c%61%72%2e%63%6e", UrlDetectorOptions.Default,
				"%77%77%77%2e%67%75%6d%62%6c%61%72%2e%63%6e");
			RunTest(" asdf  %77%77%77%2e%67%75%6d%62%6c%61%72%2e%63%6e", UrlDetectorOptions.Default,
				"%77%77%77%2e%67%75%6d%62%6c%61%72%2e%63%6e");
			RunTest("%77%77%77%2e%67%75%6d%62%6c%61%72%2e%63%6e%2e", UrlDetectorOptions.Default,
				"%77%77%77%2e%67%75%6d%62%6c%61%72%2e%63%6e%2e");
		}

		[Fact]
		public void TestDomainAndLabelSizeConstraints()
		{
			//Really long addresses testing rules about total length of domain name and number of labels in a domain and size of each label.
			RunTest(
				"This will work: 1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.a.b.c.d.e.ly "
				+ "This will not work:  1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.a.b.c.d.e.f.ly "
				+ "This should as well: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc.dddddddddddddddddddddddddddddddddddddddddddddddddddddd.bit.ly "
				+ "But this wont: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc.dddddddddddddddddddddddddddddddddddddddddddddddddddddd.bit.ly.dbl.spamhaus.org",
				UrlDetectorOptions.Default,
				"1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.1.2.3.4.5.6.7.8.9.0.a.b.c.d.e.ly",
				"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc.dddddddddddddddddddddddddddddddddddddddddddddddddddddd.bit.ly");
		}

		[Fact]
		public void TestDomainWithUsernameAndPassword()
		{
			RunTest("domain with username is http://username:password@www.google.com/site/1/2", UrlDetectorOptions.Default,
				"http://username:password@www.google.com/site/1/2");
		}

		[Fact]
		public void TestDottedHexIpAddress()
		{
			RunTest("http://0xc0.0x00.0xb2.0xEB", UrlDetectorOptions.Default, "http://0xc0.0x00.0xb2.0xEB");
			RunTest("http://0xc0.0x0.0xb2.0xEB", UrlDetectorOptions.Default, "http://0xc0.0x0.0xb2.0xEB");
			RunTest("http://0x000c0.0x00000.0xb2.0xEB", UrlDetectorOptions.Default, "http://0x000c0.0x00000.0xb2.0xEB");
			RunTest("http://0xc0.0x00.0xb2.0xEB/bobo", UrlDetectorOptions.Default, "http://0xc0.0x00.0xb2.0xEB/bobo");
			RunTest("ooh look i can find it in text http://0xc0.0x00.0xb2.0xEB/bobo like this", UrlDetectorOptions.Default,
				"http://0xc0.0x00.0xb2.0xEB/bobo");
			RunTest("noscheme look 0xc0.0x00.0xb2.0xEB/bobo", UrlDetectorOptions.Default, "0xc0.0x00.0xb2.0xEB/bobo");
			RunTest("no scheme 0xc0.0x00.0xb2.0xEB or path", UrlDetectorOptions.Default, "0xc0.0x00.0xb2.0xEB");
		}

		[Fact]
		public void TestDottedOctalIpAddress()
		{
			RunTest("http://0301.0250.0002.0353", UrlDetectorOptions.Default, "http://0301.0250.0002.0353");
			RunTest("http://0301.0250.0002.0353/bobo", UrlDetectorOptions.Default, "http://0301.0250.0002.0353/bobo");
			RunTest("http://192.168.017.015/", UrlDetectorOptions.Default, "http://192.168.017.015/");
			RunTest("ooh look i can find it in text http://0301.0250.0002.0353/bobo like this", UrlDetectorOptions.Default,
				"http://0301.0250.0002.0353/bobo");
			RunTest("noscheme look 0301.0250.0002.0353/bobo", UrlDetectorOptions.Default, "0301.0250.0002.0353/bobo");
			RunTest("no scheme 0301.0250.0002.0353 or path", UrlDetectorOptions.Default, "0301.0250.0002.0353");
		}

		[Fact]
		public void TestDoubleScheme()
		{
			RunTest("http://http://", UrlDetectorOptions.Default);
			RunTest("hello http://http://", UrlDetectorOptions.Default);
		}

		[Fact]
		public void TestEmailAndLinkWithUserPass()
		{
			RunTest("email and username is hello@test.google.com or hello@www.google.com hello:password@www.google.com",
				UrlDetectorOptions.Default, "hello@test.google.com", "hello@www.google.com", "hello:password@www.google.com");
		}

		[Fact]
		public void TestEmailAndNormalUrl()
		{
			RunTest("my email is vshlosbe@linkedin.com and my site is http://www.linkedin.com/vshlos",
				UrlDetectorOptions.Default, "vshlosbe@linkedin.com", "http://www.linkedin.com/vshlos");
		}

		[Fact]
		public void TestFtpWithUsernameAndPassword()
		{
			RunTest("ftp with username is ftp://username:password@www.google.com", UrlDetectorOptions.Default,
				"ftp://username:password@www.google.com");
		}

		[Fact]
		public void TestHexIpAddress()
		{
			RunTest("http://0xC00002EB/hello", UrlDetectorOptions.Default, "http://0xC00002EB/hello");
			RunTest("http://0xC00002EB.com/hello", UrlDetectorOptions.Default, "http://0xC00002EB.com/hello");
			RunTest("still look it up as a normal url http://0xC00002EXsB.com/hello", UrlDetectorOptions.Default,
				"http://0xC00002EXsB.com/hello");
			RunTest("ooh look i can find it in text http://0xC00002EB/bobo like this", UrlDetectorOptions.Default,
				"http://0xC00002EB/bobo");
			RunTest("browsers dont support this without a scheme look 0xC00002EB/bobo", UrlDetectorOptions.Default);
		}

		[Fact]
		public void TestIncompleteBracketSet()
		{
			RunTest("[google.com", UrlDetectorOptions.BRACKET_MATCH, "google.com");
			RunTest("lalla [google.com", UrlDetectorOptions.Default, "google.com");
		}

		[Fact]
		public void TestIncompleteIpAddresses()
		{
			RunTest("hello 10...", UrlDetectorOptions.Default);
			RunTest("hello 10...1", UrlDetectorOptions.Default);
			RunTest("hello 10..1.", UrlDetectorOptions.Default);
			RunTest("hello 10..1.1", UrlDetectorOptions.Default);
			RunTest("hello 10.1..1", UrlDetectorOptions.Default);
			RunTest("hello 10.1.1.", UrlDetectorOptions.Default);
			RunTest("hello .192..", UrlDetectorOptions.Default);
			RunTest("hello .192..1", UrlDetectorOptions.Default);
			RunTest("hello .192.1.", UrlDetectorOptions.Default);
			RunTest("hello .192.1.1", UrlDetectorOptions.Default);
			RunTest("hello ..3.", UrlDetectorOptions.Default);
			RunTest("hello ..3.1", UrlDetectorOptions.Default);
			RunTest("hello ...1", UrlDetectorOptions.Default);
		}

		[Fact]
		public void TestIncorrectParsingHtmlWithBadOptions()
		{
			RunTest("<a href=\"http://www.google.com/\">google.com</a>", UrlDetectorOptions.Default,
				//Doesn't have the http since it was read as "http:// and goes to the end.
				"www.google.com/\">google.com</a>");
		}

		[Fact]
		public void TestInternationalUrls()
		{
			RunTest("this is an international domain: http://\u043F\u0440\u0438\u043c\u0435\u0440.\u0438\u0441\u043f\u044b"
			        + "\u0442\u0430\u043d\u0438\u0435 so is this: \u4e94\u7926\u767c\u5c55.\u4e2d\u570b.",
				UrlDetectorOptions.Default,
				"http://\u043F\u0440\u0438\u043c\u0435\u0440.\u0438\u0441\u043f\u044b\u0442\u0430\u043d\u0438\u0435",
				"\u4e94\u7926\u767c\u5c55.\u4e2d\u570b.");
		}

		[Fact]
		public void TestInternationalUrlsInHtml()
		{
			RunTest(
				"<a rel=\"nofollow\" class=\"external text\" href=\"http://xn--mgbh0fb.xn--kgbechtv/\">http://\u1605\u1579\u1575\u1604.\u1573\u1582\u1578\u1576\u1575\u1585</a>",
				UrlDetectorOptions.HTML, "http://xn--mgbh0fb.xn--kgbechtv/",
				"http://\u1605\u1579\u1575\u1604.\u1573\u1582\u1578\u1576\u1575\u1585");
		}

		[Fact]
		public void TestInvalidPartsUrl()
		{
			RunTest("aksdhf http://asdf#asdf.google.com", UrlDetectorOptions.Default, "asdf.google.com");
			RunTest("00:41.<google.com/>", UrlDetectorOptions.HTML, "google.com/");
		}

		[Fact]
		public void TestIpAddressFormat()
		{
			RunTest(
				"How about IP addresses? fake: 1.1.1 1.1.1.1.1 0.0.0.256 255.255.255.256 real: 1.1.1.1 192.168.10.1 1.1.1.1.com 255.255.255.255",
				UrlDetectorOptions.Default, "1.1.1.1", "192.168.10.1", "1.1.1.1.com", "255.255.255.255");
		}

		[Fact]
		public void TestIPv4EncodedDot()
		{
			RunTest("hello 192%2e168%2e1%2e1", UrlDetectorOptions.Default, "192%2e168%2e1%2e1");
			RunTest("hello 192.168%2e1%2e1/lalala", UrlDetectorOptions.Default, "192.168%2e1%2e1/lalala");
		}

		[Fact]
		public void TestIPv4HexEncodedDot()
		{
			RunTest("hello 0xee%2e0xbb%2e0x1%2e0x1", UrlDetectorOptions.Default, "0xee%2e0xbb%2e0x1%2e0x1");
			RunTest("hello 0xee%2e0xbb.0x1%2e0x1/lalala", UrlDetectorOptions.Default, "0xee%2e0xbb.0x1%2e0x1/lalala");
		}

		[Fact]
		public void TestIpv6BacktrackingEmptyDomainName()
		{
			RunTest("check out my http:///[::2e80:0:0]", UrlDetectorOptions.Default, "[::2e80:0:0]");
			RunTest("check out my http://./[::2e80:0:0]", UrlDetectorOptions.Default, "[::2e80:0:0]");
		}

		[Fact]
		public void TestIpv6BacktrackingUsernamePassword()
		{
			RunTest("check out my url:google.com", UrlDetectorOptions.Default, "google.com");
			RunTest("check out my url:[::BAD:DEAD:BEEF:2e80:0:0]", UrlDetectorOptions.Default, "[::BAD:DEAD:BEEF:2e80:0:0]");
			RunTest("check out my url:[::BAD:DEAD:BEEF:2e80:0:0] ", UrlDetectorOptions.Default, "[::BAD:DEAD:BEEF:2e80:0:0]");
		}

		[Fact]
		public void TestIpv6BadUrls()
		{
			RunTest("[fe80:aaaa:aaaa:aaaa:3dd0:7f8e:57b7:34d5f]", UrlDetectorOptions.Default);
			RunTest("[bcad::kkkk:aaaa:3dd0:7f8e:57b7:34d5]", UrlDetectorOptions.Default);
			RunTest("[:BAD:BEEF:0:8a2e:370:7334", UrlDetectorOptions.Default);
			RunTest("[:::]", UrlDetectorOptions.Default);
			RunTest("[lalala:we]", UrlDetectorOptions.Default);
			RunTest("[:0]", UrlDetectorOptions.Default);
			RunTest("[:0:]", UrlDetectorOptions.Default);
			RunTest("::]", UrlDetectorOptions.Default);
			RunTest("[:", UrlDetectorOptions.Default);
			RunTest("fe80:22:]3123:[adf]", UrlDetectorOptions.Default);
			RunTest("[][123[][ae][fae][de][:a][d]aef:E][f", UrlDetectorOptions.Default);
			RunTest("[]]]:d]", UrlDetectorOptions.Default);
			RunTest("[fe80:aaaa:aaaa:aaaa:3dd0:7f8e:57b7:34d5:addd:addd:adee]", UrlDetectorOptions.Default);
			RunTest("[][][]2[d][]][]]]:d][[[:d[e][aee:]af:", UrlDetectorOptions.Default);
			RunTest("[adf]", UrlDetectorOptions.Default);
			RunTest("[adf:]", UrlDetectorOptions.Default);
			RunTest("[adf:0]", UrlDetectorOptions.Default);
			RunTest("[:adf]", UrlDetectorOptions.Default);
			RunTest("[]", UrlDetectorOptions.Default);
		}

		[Fact]
		public void TestIpv6BadWithGoodUrls()
		{
			RunTest("[:::] [::] [bacd::]", UrlDetectorOptions.Default, "[::]", "[bacd::]");
			RunTest("[:0][::]", UrlDetectorOptions.Default, "[::]");
			RunTest("[:0:][::afaf]", UrlDetectorOptions.Default, "[::afaf]");
			RunTest("::] [fe80:aaaa:aaaa:aaaa::]", UrlDetectorOptions.Default, "[fe80:aaaa:aaaa:aaaa::]");
			RunTest("fe80:22:]3123:[adf] [fe80:aaaa:aaaa:aaaa::]", UrlDetectorOptions.Default, "[fe80:aaaa:aaaa:aaaa::]");
			RunTest("[][123[][ae][fae][de][:a][d]aef:E][f", UrlDetectorOptions.Default);
			RunTest("[][][]2[d][]][]]]:d][[[:d[e][aee:]af:", UrlDetectorOptions.Default);
		}

		[Fact]
		public void TestIpv6BadWithGoodUrlsEmbedded()
		{
			RunTest("[fe80:aaaa:aaaa:aaaa:[::]3dd0:7f8e:57b7:34d5f]", UrlDetectorOptions.Default, "[::]");
			RunTest("[b[::7f8e]:55]akjef[::]", UrlDetectorOptions.Default, "[::7f8e]:55", "[::]");
			RunTest("[bcad::kkkk:aaaa:3dd0[::7f8e]:57b7:34d5]akjef[::]", UrlDetectorOptions.Default, "[::7f8e]:57", "[::]");
		}

		[Fact]
		public void TestIpv6BadWithGoodUrlsWeirder()
		{
			RunTest("[:[::]", UrlDetectorOptions.Default, "[::]");
			RunTest("[:] [feed::]", UrlDetectorOptions.Default, "[feed::]");
			RunTest(":[::feee]:]", UrlDetectorOptions.Default, "[::feee]");
			RunTest(":[::feee]:]]", UrlDetectorOptions.Default, "[::feee]");
			RunTest("[[:[::feee]:]", UrlDetectorOptions.Default, "[::feee]");
		}

		[Fact]
		public void TestIpv6BasicHtml()
		{
			RunTest(
				"<script type=\"text/javascript\">var a = '[AAbb:AAbb:AAbb::]', b=\"[::bbbb:]\"</script><a href=\"[::cccc:]\">[::ffff:]</a>",
				UrlDetectorOptions.HTML, "[AAbb:AAbb:AAbb::]", "[::bbbb:]", "[::cccc:]", "[::ffff:]");
		}

		[Fact]
		public void TestIpv6BracketMatching()
		{
			RunTest(
				"MY url ([::AAbb:] ) is very cool. the domain [[::ffff:]] is popular and when written like this {[::BBBe:]} it looks like code",
				UrlDetectorOptions.BRACKET_MATCH, "[::AAbb:]", "[::ffff:]", "[::BBBe:]");
		}

		[Fact]
		public void TestIpv6ConsecutiveGoodUrls()
		{
			RunTest("[::afaf][eaea::][::]", UrlDetectorOptions.Default, "[::afaf]", "[eaea::]", "[::]");
			RunTest("[::afaf]www.google.com", UrlDetectorOptions.Default, "[::afaf]", "www.google.com");
			RunTest("[lalala:we][::]", UrlDetectorOptions.Default, "[::]");
			RunTest("[::fe][::]", UrlDetectorOptions.Default, "[::fe]", "[::]");
			RunTest("[aaaa::][:0:][::afaf]", UrlDetectorOptions.Default, "[::afaf]", "[aaaa::]");
		}

		[Fact]
		public void TestIpv6DoubleSchemeWithDomain()
		{
			RunTest("http://http://[::2e80:0:0]", UrlDetectorOptions.Default, "http://[::2e80:0:0]");
			RunTest("make sure its right here http://http://[::2e80:0:0]", UrlDetectorOptions.Default, "http://[::2e80:0:0]");
		}

		[Fact]
		public void TestIpv6EmptyPort()
		{
			RunTest("http://[::AAbb:]://foo.html", UrlDetectorOptions.Default, "http://[::AAbb:]://foo.html");
			RunTest("make sure its right here http://[::AAbb:]://foo.html", UrlDetectorOptions.Default,
				"http://[::AAbb:]://foo.html");
		}

		[Fact]
		public void TestIpv6FtpWithUsernameAndPassword()
		{
			RunTest("ftp with username is ftp://username:password@[::2e80:0:0]", UrlDetectorOptions.Default,
				"ftp://username:password@[::2e80:0:0]");
		}

		[Fact]
		public void TestIpv6IncorrectParsingHtmlWithBadOptions()
		{
			RunTest("<a href=\"http://[::AAbb:]/\">google.com</a>", UrlDetectorOptions.Default,
				//Doesn't have the http since it was read as "http:// and goes to the end.
				"[::AAbb:]/\">google.com</a>");
		}

		[Fact]
		public void TestIpv6LongUrlWithInheritedScheme()
		{
			RunTest(
				"<link rel=\"stylesheet\" href=\"//[AAbb:AAbb:AAbb::]/en.wikipedia.org/load.php?debug=false&amp;lang=en&amp;modules=ext.gadget.DRN-wizard%2CReferenceTooltips%2Ccharinsert%2Cteahouse%7Cext.wikihiero%7Cmediawiki.legacy.commonPrint%2Cshared%7Cmw.PopUpMediaTransform%7Cskins.vector&amp;only=styles&amp;skin=vector&amp;*\" />",
				UrlDetectorOptions.HTML,
				"//[AAbb:AAbb:AAbb::]/en.wikipedia.org/load.php?debug=false&amp;lang=en&amp;modules=ext.gadget.DRN-wizard%2CReferenceTooltips%2Ccharinsert%2Cteahouse%7Cext.wikihiero%7Cmediawiki.legacy.commonPrint%2Cshared%7Cmw.PopUpMediaTransform%7Cskins.vector&amp;only=styles&amp;skin=vector&amp;*");
		}

		[Fact]
		public void TestIpv6MultipleSchemes()
		{
			RunTest("http://http://http://[::2e80:0:0]", UrlDetectorOptions.Default, "http://[::2e80:0:0]");
			RunTest("make sure its right here http://http://[::2e80:0:0]", UrlDetectorOptions.Default, "http://[::2e80:0:0]");
			RunTest("http://ftp://https://[::2e80:0:0]", UrlDetectorOptions.Default, "https://[::2e80:0:0]");
			RunTest("make sure its right here http://ftp://https://[::2e80:0:0]", UrlDetectorOptions.Default,
				"https://[::2e80:0:0]");
		}

		[Fact]
		public void TestIpv6NewLinesAndTabsAreDelimiters()
		{
			RunTest(
				"Do newlines and tabs break? [::2e80:0:0]/hello/\nworld [::BEEF:ADD:BEEF]\t/stuff/ [AAbb:AAbb:AAbb::]/\thello [::2e80:0:0\u0000]/hello world",
				UrlDetectorOptions.Default,
				"[::2e80:0:0]/hello/", "[::BEEF:ADD:BEEF]", "[AAbb:AAbb:AAbb::]/");
		}

		[Fact]
		public void TestIpv6QuoteMatching()
		{
			RunTest(
				"my website is \"[AAbb:AAbb:AAbb::]\" but my email is \"vshlos@[AAbb:AAbb:AAbb::]\" \" [::AAbb:]\" \" [::] \"www.abc.com\"",
				UrlDetectorOptions.QUOTE_MATCH, "[AAbb:AAbb:AAbb::]", "vshlos@[AAbb:AAbb:AAbb::]", "[::AAbb:]", "[::]",
				"www.abc.com");
		}

		[Fact]
		public void TestIpv6UrlEncodedColon()
		{
			RunTest("http%3A//[::AAbb:]", UrlDetectorOptions.Default, "http%3A//[::AAbb:]");
			RunTest("hello http%3A//[::AAbb:]", UrlDetectorOptions.Default, "http%3A//[::AAbb:]");
		}

		[Fact]
		public void TestIpv6WithPort()
		{
			RunTest("http://[AAbb:AAbb:AAbb::]:8080/helloworld", UrlDetectorOptions.Default,
				"http://[AAbb:AAbb:AAbb::]:8080/helloworld");
		}

		///<summary>
		/// https://github.com/linkedin/URL-Detector/issues/12
		/// </summary>
		[Fact]
		public void TestIssue12()
		{
			RunTest("http://user:pass@host.com host.com", UrlDetectorOptions.Default, "http://user:pass@host.com", "host.com");
		}

		///<summary>
		/// https://github.com/linkedin/URL-Detector/issues/13
		/// </summary>
		[Fact]
		public void TestIssue13()
		{
			RunTest("user@github.io/page", UrlDetectorOptions.Default, "user@github.io/page");
			RunTest("name@gmail.com", UrlDetectorOptions.Default, "name@gmail.com");
			RunTest("name.lastname@gmail.com", UrlDetectorOptions.Default, "name.lastname@gmail.com");
			RunTest("gmail.com@gmail.com", UrlDetectorOptions.Default, "gmail.com@gmail.com");
			RunTest("first.middle.reallyreallyreallyreallyreallyreallyreallyreallyreallyreallylonglastname@gmail.com", UrlDetectorOptions.Default,
				"first.middle.reallyreallyreallyreallyreallyreallyreallyreallyreallyreallylonglastname@gmail.com");
		}

		/// <summary>
		/// https://github.com/linkedin/URL-Detector/issues/15
		/// </summary>
		[Fact]
		public void TestIssue15()
		{
			RunTest(".............:::::::::::;;;;;;;;;;;;;;;::...............................................:::::::::::::::::::::::::::::....................",
				UrlDetectorOptions.Default);
		}

		/// <summary>
		/// https://github.com/linkedin/URL-Detector/issues/16
		/// </summary>
		[Fact]
		public void TestIssue16()
		{
			RunTest("://VIVE MARINE LE PEN//:@.", UrlDetectorOptions.Default);
		}

		[Fact]
		public void TestLongUrl()
		{
			RunTest("google.com.google.com is kind of a valid url", UrlDetectorOptions.Default, "google.com.google.com");
		}

		[Fact]
		public void TestLongUrlWithInheritedScheme()
		{
			RunTest(
				"<link rel=\"stylesheet\" href=\"//bits.wikimedia.org/en.wikipedia.org/load.php?debug=false&amp;lang=en&amp;modules=ext.gadget.DRN-wizard%2CReferenceTooltips%2Ccharinsert%2Cteahouse%7Cext.wikihiero%7Cmediawiki.legacy.commonPrint%2Cshared%7Cmw.PopUpMediaTransform%7Cskins.vector&amp;only=styles&amp;skin=vector&amp;*\" />",
				UrlDetectorOptions.HTML,
				"//bits.wikimedia.org/en.wikipedia.org/load.php?debug=false&amp;lang=en&amp;modules=ext.gadget.DRN-wizard%2CReferenceTooltips%2Ccharinsert%2Cteahouse%7Cext.wikihiero%7Cmediawiki.legacy.commonPrint%2Cshared%7Cmw.PopUpMediaTransform%7Cskins.vector&amp;only=styles&amp;skin=vector&amp;*");
		}

		[Fact]
		public void TestMultipleSchemes()
		{
			RunTest("http://http://www.google.com", UrlDetectorOptions.Default, "http://www.google.com");
			RunTest("make sure it's right here http://http://www.google.com", UrlDetectorOptions.Default,
				"http://www.google.com");
			RunTest("http://http://http://www.google.com", UrlDetectorOptions.Default, "http://www.google.com");
			RunTest("make sure it's right here http://http://http://www.google.com", UrlDetectorOptions.Default,
				"http://www.google.com");
			RunTest("http://ftp://https://www.google.com", UrlDetectorOptions.Default, "https://www.google.com");
			RunTest("make sure its right here http://ftp://https://www.google.com", UrlDetectorOptions.Default,
				"https://www.google.com");
		}

		[Fact]
		public void TestNewLinesAndTabsAreDelimiters()
		{
			RunTest(
				"Do newlines and tabs break? google.com/hello/\nworld www.yahoo.com\t/stuff/ yahoo.com/\thello news.ycombinator.com\u0000/hello world",
				UrlDetectorOptions.Default,
				"google.com/hello/", "www.yahoo.com", "yahoo.com/", "news.ycombinator.com");
		}

		[Fact]
		public void TestNonStandardDots()
		{
			RunTest(
				"www\u3002google\u3002com username:password@www\uFF0Eyahoo\uFF0Ecom http://www\uFF61facebook\uFF61com http://192\u3002168\uFF0E0\uFF611/",
				UrlDetectorOptions.Default,
				"www\u3002google\u3002com", "username:password@www\uFF0Eyahoo\uFF0Ecom", "http://www\uFF61facebook\uFF61com",
				"http://192\u3002168\uFF0E0\uFF611/");
		}

		[Fact]
		public void TestNonStandardDotsBacktracking()
		{
			RunTest("\u9053 \u83dc\u3002\u3002\u3002\u3002", UrlDetectorOptions.Default);
		}

		[Fact]
		public void TestNumbersAreNotDetected()
		{
			//make sure pure numbers don't work, but domains with numbers do.
			RunTest("Do numbers work? such as 3.1415 or 4.com", UrlDetectorOptions.Default, "4.com");
		}

		[Fact]
		public void TestNumericIpAddress()
		{
			RunTest("http://3232235521/helloworld", UrlDetectorOptions.Default, "http://3232235521/helloworld");
		}

		[Fact]
		public void TestNumericIpAddressWithPort()
		{
			RunTest("http://3232235521:8080/helloworld", UrlDetectorOptions.Default, "http://3232235521:8080/helloworld");
		}

		[Fact]
		public void TestOctalIpAddress()
		{
			RunTest("http://030000001353/bobobo", UrlDetectorOptions.Default, "http://030000001353/bobobo");
			RunTest("ooh look i can find it in text http://030000001353/bobo like this", UrlDetectorOptions.Default,
				"http://030000001353/bobo");
			RunTest("browsers dont support this without a scheme look 030000001353/bobo", UrlDetectorOptions.Default);
		}

		[Fact]
		public void TestParseJavascript()
		{
			RunTest("var url = 'www.abc.com';\n" + "var url = \"www.def.com\";", UrlDetectorOptions.JAVASCRIPT, "www.abc.com",
				"www.def.com");
		}

		[Fact]
		public void TestParseJson()
		{
			RunTest("{\"url\": \"www.google.com\", \"hello\": \"world\", \"anotherUrl\":\"http://www.yahoo.com\"}",
				UrlDetectorOptions.JSON, "www.google.com", "http://www.yahoo.com");
		}

		[Fact]
		public void TestParseXml()
		{
			RunTest("<url attr=\"www.def.com\">www.abc.com</url><url href=\"hello.com\" />", UrlDetectorOptions.XML,
				"www.abc.com", "www.def.com", "hello.com");
		}

		[Fact]
		public void TestQuoteMatching()
		{
			//test quote matching with no html
			RunTest(
				"my website is \"www.google.com\" but my email is \"vshlos@gmail.com\" \" www.abcd.com\" \" hello.com \"www.abc.com\"",
				UrlDetectorOptions.QUOTE_MATCH, "www.google.com", "vshlos@gmail.com", "www.abcd.com", "hello.com",
				"www.abc.com");
		}

		[Fact]
		public void TestSingleLevelDomain()
		{
			RunTest("localhost:9000/lalala hehe", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "localhost:9000/lalala");
			RunTest("http://localhost lasdf", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "http://localhost");
			RunTest("localhost:9000/lalala", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "localhost:9000/lalala");
			RunTest("192.168.1.1/lalala", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "192.168.1.1/lalala");
			RunTest("http://localhost", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "http://localhost");
			RunTest("//localhost", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "//localhost");
			RunTest("asf//localhost", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "asf//localhost");
			RunTest("hello/", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "hello/");
			RunTest("go/", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "go/");
			RunTest("hello:password@go//", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "hello:password@go//");
			RunTest("hello:password@go", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "hello:password@go");
			RunTest("hello:password@go lala", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "hello:password@go");
			RunTest("hello.com..", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN, "hello.com.");
			RunTest("a/", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN);
			RunTest("asdflocalhost aksdjfhads", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN);
			RunTest("/", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN);
			RunTest("////", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN);
			RunTest("hi:", UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN);
			RunTest("http://localhost", UrlDetectorOptions.Default);
			RunTest("localhost:9000/lalala", UrlDetectorOptions.Default);
		}

		[Fact]
		public void TestTwoBasicUrls()
		{
			RunTest("the url google.com is a lot better then www.google.com.", UrlDetectorOptions.Default, "google.com",
				"www.google.com.");
		}

		[Fact]
		public void TestUncommonFormatUsernameAndPassword()
		{
			RunTest("weird url with username is username:password@www.google.com", UrlDetectorOptions.Default,
				"username:password@www.google.com");
		}

		[Fact]
		public void TestUrlEncodedBadPath()
		{
			RunTest("%2ewtfismyip", UrlDetectorOptions.Default);
			RunTest("wtfismyip%2e", UrlDetectorOptions.Default);
			RunTest("wtfismyip%2ecom%2e", UrlDetectorOptions.Default, "wtfismyip%2ecom%2e");
			RunTest("wtfismyip%2ecom.", UrlDetectorOptions.Default, "wtfismyip%2ecom.");
			RunTest("%2ewtfismyip%2ecom", UrlDetectorOptions.Default, "wtfismyip%2ecom");
		}

		[Fact]
		public void TestUrlEncodedColon()
		{
			RunTest("http%3A//google.com", UrlDetectorOptions.Default, "http%3A//google.com");
			RunTest("hello http%3A//google.com", UrlDetectorOptions.Default, "http%3A//google.com");
		}

		[Fact]
		public void TestUrlEncodedDot()
		{
			RunTest("hello www%2ewtfismyip%2ecom", UrlDetectorOptions.Default, "www%2ewtfismyip%2ecom");
			RunTest("hello wtfismyip%2ecom", UrlDetectorOptions.Default, "wtfismyip%2ecom");
			RunTest("http://wtfismyip%2ecom", UrlDetectorOptions.Default, "http://wtfismyip%2ecom");
			RunTest("make sure its right here http://wtfismyip%2ecom", UrlDetectorOptions.Default, "http://wtfismyip%2ecom");
		}

		[Fact]
		public void TestUrlWithEmptyPort()
		{
			RunTest("http://wtfismyip.com://foo.html", UrlDetectorOptions.Default, "http://wtfismyip.com://foo.html");
			RunTest("make sure its right here http://wtfismyip.com://foo.html", UrlDetectorOptions.Default,
				"http://wtfismyip.com://foo.html");
		}

		[Fact]
		public void TestWrongSpacingInSentence()
		{
			RunTest("I would not like to work at salesforce.com, it looks like a crap company.and not cool!",
				UrlDetectorOptions.Default, "salesforce.com", "company.and");
		}
	}
}