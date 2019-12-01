using System.Collections.Generic;

namespace urldetector.eladaus
{
	public static class DictionaryExtensions
	{
		public static void IncrementCount(this Dictionary<char, int> dict, char key, int value)
		{
			if (dict.ContainsKey(key))
			{
				dict[key] = dict[key] + value;
			}
			else
			{
				dict[key] = value;
			}
		}
	}
}