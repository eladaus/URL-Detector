using System.Collections.Generic;

namespace urldetector.legacy.eladaus
{
	public static class LegacyStackExtensions
	{
		public static bool IsEmpty<T>(this Stack<T> stack)
		{
			return stack.Count == 0;
		}
	}
}