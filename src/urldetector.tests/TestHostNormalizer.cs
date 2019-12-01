using System.Collections.Generic;
using Xunit;

namespace urldetector.tests
{
	public class TestHostNormalizer
	{
		
		public static IEnumerable<object[]> GetIpAddresses =>
			new List<object[]>
			{
				new object[] {"[fefe::]", "[fefe::]"},
				new object[] {"[::ffff]", "[::ffff]"},
				new object[] {"[::255.255.255.255]", "[::ffff:ffff]"},
				new object[] {"[::]", "[::]"},
				new object[] {"[::1]", "[::1]"},
				new object[] {"[aAaA::56.7.7.5]", "[aaaa::3807:705]"},
				new object[] {"[BBBB:ab:f78F:f:DDDD:bab:56.7.7.5]", "[bbbb:ab:f78f:f:dddd:bab:3807:705]"},
				new object[] {"[Aaaa::1]", "[aaaa::1]"},
				new object[] {"[::192.167.2.2]", "[::c0a7:202]"},
				new object[] {"[0:ffff::077.0x22.222.11]", "[0:ffff::3f22:de0b]"},
				new object[] {"[0::ffff:077.0x22.222.11]", "63.34.222.11"},
				new object[] {"192.168.1.1", "192.168.1.1"},
				new object[] {"0x92.168.1.1", "146.168.1.1"},
				new object[] {"3279880203", "195.127.0.11"}
			};


		[Theory]
		[MemberData(nameof(GetIpAddresses))]
		public void TestIpHostNormalizationAndGetBytes(string original, string expectedHost)
		{
			HostNormalizer hostNormalizer = new HostNormalizer(original);
			var normalizedHost = hostNormalizer.GetNormalizedHost();
			Assert.Equal(expectedHost, normalizedHost);

			/*
			 InetAddress address = InetAddress.getByName(expectedHost);
			byte[] expectedBytes;
			if (address instanceof Inet4Address) {
			  expectedBytes = new byte[16];
			  expectedBytes[10] = (byte) 0xff;
			  expectedBytes[11] = (byte) 0xff;
			  System.arraycopy(address.getAddress(), 0, expectedBytes, 12, 4);
			} else {
			  expectedBytes = address.getAddress();
			}
			Assert.True(Arrays.equals(hostNormalizer.getBytes(), expectedBytes));
			*/
		}


		
		public static IEnumerable<object[]> GetNormalHosts =>
			new List<object[]>
			{
				new object[] {"sALes.com"},
				new object[] {"33r.nEt"},
				new object[] {"173839.com"},
				new object[] {"192.168.-3.1"},
				new object[] {"[::-34:50]"},
				new object[] {"[-34::192.168.34.-3]"}
			};


		[Theory]
		[MemberData(nameof(GetNormalHosts))]
		public void TestSanityAddresses(string host)
		{
			HostNormalizer hostNormalizer = new HostNormalizer(host);
			Assert.Equal(hostNormalizer.GetNormalizedHost(), host.ToLower());
			Assert.Null(hostNormalizer.GetBytes());
		}
	}
}