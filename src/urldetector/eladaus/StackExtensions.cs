using System.Collections.Generic;

namespace urldetector.eladaus
{
	public static class StackExtensions
	{
		public static bool IsEmpty<T>(this Stack<T> stack)
		{
			return stack.Count == 0;
		}
	}
}