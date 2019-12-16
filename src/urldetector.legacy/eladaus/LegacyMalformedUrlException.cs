using System;

namespace urldetector.legacy.eladaus
{
	public class LegacyMalformedUrlException : Exception
	{
		public LegacyMalformedUrlException(string message) : base(message)
		{
		}
	}
}