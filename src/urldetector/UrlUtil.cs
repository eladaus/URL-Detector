using System;
using System.Collections.Generic;
using System.Text;
using urldetector.detection;
using urldetector.eladaus;

namespace urldetector
{
	public class UrlUtil
	{
		private UrlUtil()
		{
		}

		/// <summary>
		/// Decodes the url by iteratively removing hex characters with backtracking.
		/// For example: %2525252525252525 becomes %
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string Decode(string url)
		{
			var stringBuilder = new StringBuilder(url);
			var nonDecodedPercentIndices = new Stack<int>();
			var i = 0;
			while (i < stringBuilder.Length - 2)
			{
				var curr = stringBuilder[i];
				if (curr == '%')
				{
					if (CharUtils.IsHex(stringBuilder[i + 1]) && CharUtils.IsHex(stringBuilder[i + 2]))
					{
						var startIndex = i + 1;
						var endIndex = i + 3;
						var decodedChar = (char) Convert.ToInt16(stringBuilder.ToString().Substring(startIndex, endIndex-startIndex), 16);

						stringBuilder.Remove(i, endIndex - i); //delete the % and two hex digits
						stringBuilder.Insert(i, decodedChar); //add decoded character

						if (decodedChar == '%')
						{
							i--; //backtrack one character to check for another decoding with this %.
						}
						else if (!nonDecodedPercentIndices.IsEmpty() && CharUtils.IsHex(decodedChar)
						                                             && CharUtils.IsHex(stringBuilder[i - 1]) && i - nonDecodedPercentIndices.Peek() == 2)
						{
							//Go back to the last non-decoded percent sign if it's decodable.
							//We only need to go back if it's of form %[HEX][HEX]
							i = nonDecodedPercentIndices.Pop() - 1; //backtrack to the % sign.
						}
						else if (!nonDecodedPercentIndices.IsEmpty() && i == stringBuilder.Length - 2)
						{
							//special case to handle %[HEX][Unknown][end of string]
							i = nonDecodedPercentIndices.Pop() - 1; //backtrack to the % sign.
						}
					}
					else
					{
						nonDecodedPercentIndices.Push(i);
					}
				}

				i++;
			}

			return stringBuilder.ToString();
		}

		/// <summary>
		/// Removes TAB (0x09), CR (0x0d), and LF (0x0a) from the URL
		/// @param urlPart The part of the url we are canonicalizing
		/// </summary>
		/// <param name="urlPart"></param>
		/// <returns></returns>
		protected internal static string RemoveSpecialSpaces(string urlPart)
		{
			var stringBuilder = new StringBuilder(urlPart);
			for (var i = 0; i < stringBuilder.Length; i++)
			{
				var curr = stringBuilder[i];
				if (CharUtils.IsWhiteSpace(curr))
				{
					stringBuilder.Remove(i, 1);
				}
			}

			return stringBuilder.ToString();
		}


		/// <summary>
		/// Replaces all special characters in the url with hex strings.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string Encode(string url)
		{
			var encoder = new StringBuilder();
			foreach (var chr in url.ToCharArray())
			{
				var chrByte = (byte) chr;
				if (chrByte <= 32 || chrByte >= 127 || chr == '#' || chr == '%')
				{
					encoder.Append("%" + BitConverter.ToString(new[] {chrByte}));	// Dale equivalent
				}
				else
				{
					encoder.Append(chr);
				}
			}
			return encoder.ToString();
		}


		/// <summary>
		/// Removes all leading and trailing dots; replaces consecutive dots with a single dot
		/// Ex: ".lalal.....com." -&gt; "lalal.com"
		/// </summary>
		/// <param name="host"></param>
		/// <returns></returns>
		public static string RemoveExtraDots(string host)
		{
			var stringBuilder = new StringBuilder();
			var reader = new InputTextReader(host);
			while (!reader.Eof())
			{
				var curr = reader.Read();
				stringBuilder.Append(curr);
				if (curr == '.')
				{
					var possibleDot = curr;
					while (possibleDot == '.' && !reader.Eof())
					{
						possibleDot = reader.Read();
					}

					if (possibleDot != '.')
					{
						stringBuilder.Append(possibleDot);
					}
				}
			}

			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] == '.')
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}

			if (stringBuilder.Length > 0 && stringBuilder[0] == '.')
			{
				stringBuilder.Remove(0, 1);
			}

			return stringBuilder.ToString();
		}
	}
}