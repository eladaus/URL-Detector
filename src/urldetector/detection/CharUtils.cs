using System;
using System.Collections.Generic;
using System.Text;

namespace urldetector.detection
{
	public static class CharUtils
	{

		/// <summary>
		/// Checks if character is a valid hex character.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static bool IsHex(char a)
		{
			return a >= '0' && a <= '9' || a >= 'a' && a <= 'f' || a >= 'A' && a <= 'F';
		}


		/// <summary>
		/// Checks if character is a valid alphabetic character.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static bool IsAlpha(char a)
		{
			return a >= 'a' && a <= 'z' || a >= 'A' && a <= 'Z';
		}


		/// <summary>
		/// Checks if character is a valid numeric character.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static bool IsNumeric(char a)
		{
			return a >= '0' && a <= '9';
		}


		/// <summary>
		/// Checks if character is a valid alphanumeric character.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static bool IsAlphaNumeric(char a)
		{
			return IsAlpha(a) || IsNumeric(a);
		}


		/// <summary>
		/// Checks if character is a valid unreserved character. This is defined by the RFC 3986 ABNF
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static bool IsUnreserved(char a)
		{
			return IsAlphaNumeric(a) || a == '-' || a == '.' || a == '_' || a == '~';
		}

		
		/// <summary>
		/// Checks if character is a dot. Heres the doc:
		/// http://docs.oracle.com/javase/6/docs/api/java/net/IDN.html#toASCII%28java.lang.String,%20int%29
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static bool IsDot(char a)
		{
			return a == '.' || a == '\u3002' || a == '\uFF0E' || a == '\uFF61';
		}


		public static bool IsWhiteSpace(char a)
		{
			return a == ' ' || a == '\n' || a == '\t' || a == '\r';
		}


		/// <summary>
		/// Splits a string without the use of a regex, which could split either by isDot() or %2e
		/// @param input the input string that will be split by dot
		/// @return an array of strings that is a partition of the original string split by dot
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string[] SplitByDot(string input)
		{
			var splitList = new List<string>();
			var section = new StringBuilder();
			if (string.IsNullOrEmpty(input))
			{
				return new[] {""};
			}

			var reader = new InputTextReader(input);
			while (!reader.Eof())
			{
				var curr = reader.Read();
				if (IsDot(curr))
				{
					splitList.Add(section.ToString());
					section.Length = 0;
				}
				else if (curr == '%' && reader.CanReadChars(2) && reader.Peek(2).Equals("2e", StringComparison.InvariantCultureIgnoreCase))
				{
					reader.Read();
					reader.Read(); //advance past the 2e
					splitList.Add(section.ToString());
					section.Length = 0;
				}
				else
				{
					section.Append(curr);
				}
			}

			splitList.Add(section.ToString());
			return splitList.ToArray();
		}
	}
}