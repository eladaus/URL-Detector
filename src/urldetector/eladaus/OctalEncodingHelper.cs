using System;

namespace urldetector.eladaus
{
	public static class OctalEncodingHelper
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


        /// <summary>
        /// Try to parse an octal number as a LONG from a span of characters.
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParseOctal(ReadOnlySpan<char> chars, out long result)
        {
            long tmpResult = result = 0;

            foreach (char c in chars)
            {
                // fast ASCII check for '0'–'7'
                if ((uint)(c - '0') > 7)
                {
                    return false;
                }

                // multiply by 8 and add digit
                tmpResult = (tmpResult << 3) + (c - '0');
            }

            result = tmpResult;
            return true;
        }


        /// <summary>
        /// Try to parse an octal number as an INT from a span of characters
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParseOctal(ReadOnlySpan<char> chars, out int result)
        {
            // Apparently, a const is inlined by the compiler; you avoid repeated
            // property-access logic (even though int.MaxValue is itself a
            // compile-time constant, reading a local const makes the IL a bit simpler)
            const int max = int.MaxValue;
            int tmp = 0;

            foreach (char c in chars)
            {
                int digit = c - '0';
                if ((uint)digit > 7)       // not 0–7
                {
                    result = 0;
                    return false;
                }

                // overflow check: tmp*8 + digit <= Max
                // ↔ tmp <= (Max - digit) / 8
                if (tmp > (max - digit) >> 3)
                {
                    result = 0;
                    return false;
                }

                tmp = (tmp << 3) + digit;
            }

            result = tmp;
            return true;
        }
    }

}