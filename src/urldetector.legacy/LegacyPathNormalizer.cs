using System.Collections.Generic;
using System.Text;
using urldetector.legacy.eladaus;

namespace urldetector.legacy
{
	public class LegacyPathNormalizer
	{
		/// <summary>
		/// Normalizes the path by doing the following:
		/// remove special spaces, decoding hex encoded characters,
		/// gets rid of extra dots and slashes, and re-encodes it once
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string NormalizePath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return path;
			}

			path = LegacyUrlUtil.Decode(path);
			path = SanitizeDotsAndSlashes(path);
			return LegacyUrlUtil.Encode(path);
		}


		/// <summary>
		/// 1. Replaces "/./" with "/" recursively.
		/// 2. "/blah/asdf/.." -&gt; "/blah"
		/// 3. "/blah/blah2/blah3/../../blah4" -&gt; "/blah/blah4"
		/// 4. "//" -&gt; "/"
		/// 5. Adds a slash at the end if there isn't one
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static string SanitizeDotsAndSlashes(string path)
		{
			var stringBuilder = new StringBuilder(path);
			var slashIndexStack = new Stack<int>();
			var index = 0;
			while (index < stringBuilder.Length - 1)
			{
				if (stringBuilder[index] == '/')
				{
					slashIndexStack.Push(index);
					if (stringBuilder[index + 1] == '.')
					{
						if (index < stringBuilder.Length - 2 && stringBuilder[index + 2] == '.')
						{
							//If it looks like "/../" or ends with "/.."
							if (index < stringBuilder.Length - 3 && stringBuilder[index + 3] == '/'
							    || index == stringBuilder.Length - 3)
							{
								var endOfPath = index == stringBuilder.Length - 3;
								slashIndexStack.Pop();
								var endIndex = index + 3;
								// backtrack so we can detect if this / is part of another replacement
								index = slashIndexStack.IsEmpty() ? -1 : slashIndexStack.Pop() - 1;
								var startIndex = endOfPath ? index + 1 : index;
								stringBuilder.Remove(startIndex + 1, endIndex - (startIndex + 1));
							}
						}
						else if (index < stringBuilder.Length - 2 && stringBuilder[index + 2] == '/'
						         || index == stringBuilder.Length - 2)
						{
							var endOfPath = index == stringBuilder.Length - 2;
							slashIndexStack.Pop();
							var startIndex = endOfPath ? index + 1 : index;
							stringBuilder.Remove(startIndex, index + 2 - startIndex); // "/./" -> "/"
							index--; // backtrack so we can detect if this / is part of another replacement
						}
					}
					else if (stringBuilder[index + 1] == '/')
					{
						slashIndexStack.Pop();
						stringBuilder.Remove(index, 1);
						index--;
					}
				}

				index++;
			}

			if (stringBuilder.Length == 0)
			{
				stringBuilder.Append("/"); //Every path has at least a slash
			}

			return stringBuilder.ToString();
		}
	}
}