using System.Text;

namespace urldetector.eladaus
{
	public static class StringBuilderExtensions
	{
		public static string ToString(this StringBuilder sb, int startIndex)
		{
			return sb.ToString(startIndex, sb.Length - startIndex);
		}
	}
}