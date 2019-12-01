using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace urldetector.eladaus
{
	public static class AsciiTextEncodingExtensions
	{

		/// <summary>
		/// https://stackoverflow.com/a/1615860/4413476
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string EncodeNonAsciiCharacters( string value ) 
		{
			var sb = new StringBuilder();
			foreach( char c in value ) 
			{
				if( c > 127 ) 
				{
					// This character is too big for ASCII
					string encodedValue = "\\u" + ((int) c).ToString( "x4" );
					sb.Append( encodedValue );
				}
				else 
				{
					sb.Append( c );
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// https://stackoverflow.com/a/1615860/4413476
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string DecodeEncodedNonAsciiCharacters( string value ) 
		{
			return Regex.Replace(
				value,
				@"\\u(?<Value>[a-zA-Z0-9]{4})",
				m => ((char) int.Parse( m.Groups["Value"].Value, NumberStyles.HexNumber )).ToString());
		}
	}
}