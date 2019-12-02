using System;

namespace urldetector.eladaus
{
	public class MalformedUrlException : Exception
	{
		public MalformedUrlException(string message) : base(message)
		{
		}
	}
}