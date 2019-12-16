using System;

namespace urldetector.legacy.eladaus
{
	public static class LegacyOctalEncodingHelper
	{
		public static bool LooksLikeOctal(ReadOnlySpan<char> chars)
		{
			foreach (var c in chars)
			{
				var isOctal = c >= '0' && c <= '7';

				if (!isOctal)
				{
					return false;
				}
			}
			return true;
		}
	}
}