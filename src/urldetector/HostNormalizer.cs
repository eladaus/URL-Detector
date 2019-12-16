using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using urldetector.detection;
using urldetector.eladaus;

namespace urldetector
{
	/// <summary>
	/// Normalizes the host by converting hex characters to the actual textual representation, changes ip addresses
	/// to a formal format. Then re-encodes the readonly host name.
	/// </summary>
	public class HostNormalizer
	{
		private static readonly long MAX_NUMERIC_DOMAIN_VALUE = 4294967295L;
		private static readonly int MAX_IPV4_PART = 255;
		private static readonly int MIN_IP_PART = 0;
		private static readonly int MAX_IPV6_PART = 0xFFFF;
		private static readonly int IPV4_MAPPED_IPV6_START_OFFSET = 12;
		private static readonly int NUMBER_BYTES_IN_IPV4 = 4;

		public byte[] Bytes { get; private set; }
		private string _host;
		private string _normalizedHost;

		public HostNormalizer(string host)
		{
			_host = host;
			Bytes = null;

			NormalizeHost();
		}

		private void NormalizeHost()
		{

			if (string.IsNullOrEmpty(_host))
			{
				return;
			}

			string host;
			try
			{
				host = AsciiTextEncodingExtensions.EncodeNonAsciiCharacters(_host);
			}
			catch (Exception)
			{
				//occurs when the url is invalid. Just return
				return;
			}

			host = host.ToLowerInvariant();
			host = UrlUtil.Decode(host);

			Bytes = TryDecodeHostToIp(host);



			if (Bytes != null)
			{
				try
				{
					var ipAddressFromBytes = new IPAddress(Bytes);

					if (ipAddressFromBytes.AddressFamily == AddressFamily.InterNetworkV6)
					{
						if (ipAddressFromBytes.IsIPv4MappedToIPv6)
						{
							host = ipAddressFromBytes.MapToIPv4().ToString();
						}
						else
						{
							host = ipAddressFromBytes.ToString();
							host = "[" + host + "]";
						}
					}
					else
					{
						host = ipAddressFromBytes.ToString();
					}
				}
				catch (Exception)
				{
					return;
				}
			}

			if (string.IsNullOrEmpty(host))
			{
				return;
			}

			host = UrlUtil.RemoveExtraDots(host);

			_normalizedHost = UrlUtil.Encode(host).Replace("\\x", "%");
		}


		/// <summary>
		/// Checks if the host is an ip address. Returns the byte representation of it
		/// </summary>
		/// <param name="host"></param>
		/// <returns></returns>
		private static byte[] TryDecodeHostToIp(string host)
		{
			if (host.StartsWith("[") && host.EndsWith("]"))
			{
				return TryDecodeHostToIPv6(host);
			}

			return TryDecodeHostToIPv4(host);
		}


		/// <summary>
		/// This covers cases like:
		/// Hexadecimal:        0x1283983
		/// Decimal:            12839273
		/// Octal:              037362273110
		/// Dotted Decimal:     192.168.1.1
		/// Dotted Hexadecimal: 0xfe.0x83.0x18.0x1
		/// Dotted Octal:       0301.00.046.00
		/// Dotted Mixed:       0x38.168.077.1
		/// if ipv4 was found, _bytes is set to the byte representation of the ipv4 address
		/// </summary>
		/// <param name="host"></param>
		/// <returns></returns>
		private static byte[] TryDecodeHostToIPv4(ReadOnlySpan<char> host)
		{
			string[] parts = CharUtils.SplitByDot(host.ToString());
			int numParts = parts.Length;

			if (numParts != 4 && numParts != 1)
			{
				return null;
			}

			byte[] bytes = new byte[16];

			//An ipv4 mapped ipv6 bytes will have the 11th and 12th byte as 0xff
			bytes[10] = (byte)0xff;
			bytes[11] = (byte)0xff;
			for (int i = 0; i < parts.Length; i++)
			{
				Span<char> parsedNum;
				int @base;
				if (parts[i].StartsWith("0x"))
				{ 
					//hex
					parsedNum = parts[i].AsSpan().Slice(2).ToArray();
					@base = 16;
				}
				else if (parts[i].StartsWith("0"))
				{ 
					//octal
					parsedNum = parts[i].AsSpan().Slice(1).ToArray();
					@base = 8;
				}
				else
				{ 
					//decimal
					parsedNum = parts[i].ToCharArray();
					@base = 10;
				}

				long section;
				if (parsedNum == null || parsedNum.IsEmpty)
				{
					section = 0;
				}
				else
				{
					try
					{
						if (16 == @base)
						{
							var isParsed = long.TryParse(parsedNum, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out section);
							if (!isParsed)
							{
								return null;
							}
						}
						else if (8 == @base && OctalEncodingHelper.LooksLikeOctal(parsedNum))
						{
							section = Convert.ToInt32(parsedNum.ToString(), @base);
						}
						else
						{
							var isParsed = long.TryParse(parsedNum, out section);
							if (!isParsed)
							{
								return null;
							}
						}
					}
					catch (Exception)
					{
						return null;
					}
				}

				if (numParts == 4 && section > MAX_IPV4_PART || //This would look like 288.1.2.4
					numParts == 1 && section > MAX_NUMERIC_DOMAIN_VALUE || //This would look like 4294967299
					section < MIN_IP_PART)
				{
					return null;
				}
				// bytes 13->16 is where the ipv4 address of an ipv4-mapped-ipv6-address is stored.
				if (numParts == 4)
				{
					var b = Convert.ToByte(section);
					bytes[IPV4_MAPPED_IPV6_START_OFFSET + i] = b; // section.byteValue();
				}
				else
				{ 
					// numParts == 1
					int index = IPV4_MAPPED_IPV6_START_OFFSET;
					bytes[index++] = (byte)((section >> 24) & 0xFF);
					bytes[index++] = (byte)((section >> 16) & 0xFF);
					bytes[index++] = (byte)((section >> 8) & 0xFF);
					bytes[index] = (byte)(section & 0xFF);
					return bytes;
				}
			}

			return bytes;

		}


		/// <summary>
		/// Recommendation for IPv6 Address Text Representation
		/// http://tools.ietf.org/html/rfc5952
		///
		/// If ipv6 was found, _bytes is set to the byte representation of the ipv6 address
		/// </summary>
		/// <param name="host"></param>
		/// <returns></returns>
		private static byte[] TryDecodeHostToIPv6(string host)
		{
			var ip = host.Substring(1, host.Length - 2);

			var parts = new List<string>(ip.Split(':'));

			if (parts.Count < 3)
			{
				return null;
			}

			//Check for embedded ipv4 address
			//string lastPart = parts.get(parts.size() - 1);
			var lastPart = parts.Last().AsSpan();
			var zoneIndexStart = lastPart.LastIndexOf("%");
			var lastPartWithoutZoneIndex = zoneIndexStart == -1 ? lastPart : lastPart.Slice(0, zoneIndexStart);
			byte[] ipv4Address = null;
			if (!IsHexSection(lastPartWithoutZoneIndex))
			{
				ipv4Address = TryDecodeHostToIPv4(lastPartWithoutZoneIndex);
			}

			byte[] bytes = new byte[16];
			//How many parts do we need to fill by the end of this for loop?
			int totalSize = ipv4Address == null ? 8 : 6;
			//How many zeroes did we fill in the case of double colons? Ex: [::1] will have numberOfFilledZeroes = 7
			int numberOfFilledZeroes = 0;
			//How many sections do we have to parse through? Ex: [fe80:ff::192.168.1.1] size = 3, another ex: [a:a::] size = 4
			int size = ipv4Address == null ? parts.Count : parts.Count - 1;
			for (int i = 0; i < size; i++)
			{
				int lenPart = parts[i].Length;
				if (lenPart == 0 && i != 0 && i != parts.Count - 1)
				{
					numberOfFilledZeroes = totalSize - size;
					for (int k = i; k < numberOfFilledZeroes + i; k++)
					{
						// C# guess
						Array.ConstrainedCopy(SectionToTwoBytes(0), 0, bytes, k * 2, 2);
					}
				}
				int section;

				if (lenPart == 0)
				{
					section = 0;
				}
				else
				{
					var wasParsed = int.TryParse(parts[i], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out section);
					if (!wasParsed) return null;
				}

				if (section > MAX_IPV6_PART || section < MIN_IP_PART)
				{
					return null;
				}
				Array.ConstrainedCopy(SectionToTwoBytes(section), 0, bytes, (numberOfFilledZeroes + i) * 2, 2);
			}

			if (ipv4Address != null)
			{
				Array.ConstrainedCopy(ipv4Address, IPV4_MAPPED_IPV6_START_OFFSET, bytes, IPV4_MAPPED_IPV6_START_OFFSET,
					NUMBER_BYTES_IN_IPV4);
			}
			return bytes;

		}

		private static bool IsHexSection(ReadOnlySpan<char> section)
		{
			for (var i = 0; i < section.Length; i++)
			{
				if (!CharUtils.IsHex(section[i]))
				{
					return false;
				}
			}

			return true;
		}

		private static byte[] SectionToTwoBytes(int section)
		{
			var bytes = new byte[2];
			bytes[0] = (byte)((section >> 8) & 0xff);
			bytes[1] = (byte)(section & 0xff);
			return bytes;
		}

		public byte[] GetBytes()
		{
			return Bytes;
		}

		public string GetNormalizedHost()
		{
			return _normalizedHost;
		}
	}
}