using System;

namespace urldetector.legacy
{
	public class LegacyUrlMarker
	{
		private int _fragmentIndex = -1;
		private int _hostIndex = -1;

		private string _originalUrl;
		private int _pathIndex = -1;
		private int _portIndex = -1;
		private int _queryIndex = -1;
		private int _schemeIndex = -1;
		private int _usernamePasswordIndex = -1;

		public LegacyUrl CreateUrl()
		{
			return new LegacyUrl(this);
		}

		public void SetOriginalUrl(string originalUrl)
		{
			_originalUrl = originalUrl;
		}

		public string GetOriginalUrl()
		{
			return _originalUrl;
		}

		public void SetIndex(LegacyUrlPart urlPart, int index)
		{
			switch (urlPart)
			{
				case LegacyUrlPart.SCHEME:
					_schemeIndex = index;
					break;
				case LegacyUrlPart.USERNAME_PASSWORD:
					_usernamePasswordIndex = index;
					break;
				case LegacyUrlPart.HOST:
					_hostIndex = index;
					break;
				case LegacyUrlPart.PORT:
					_portIndex = index;
					break;
				case LegacyUrlPart.PATH:
					_pathIndex = index;
					break;
				case LegacyUrlPart.QUERY:
					_queryIndex = index;
					break;
				case LegacyUrlPart.FRAGMENT:
					_fragmentIndex = index;
					break;
			}
		}

		/// <summary>
		/// @param urlPart The part you want the index of
		/// @return Returns the index of the part
		/// </summary>
		public int IndexOf(LegacyUrlPart urlPart)
		{
			switch (urlPart)
			{
				case LegacyUrlPart.SCHEME:
					return _schemeIndex;
				case LegacyUrlPart.USERNAME_PASSWORD:
					return _usernamePasswordIndex;
				case LegacyUrlPart.HOST:
					return _hostIndex;
				case LegacyUrlPart.PORT:
					return _portIndex;
				case LegacyUrlPart.PATH:
					return _pathIndex;
				case LegacyUrlPart.QUERY:
					return _queryIndex;
				case LegacyUrlPart.FRAGMENT:
					return _fragmentIndex;
				default:
					return -1;
			}
		}

		public void UnsetIndex(LegacyUrlPart urlPart)
		{
			SetIndex(urlPart, -1);
		}

		/// <summary>
		/// This is used in TestUrlMarker to set indices more easily.
		/// @param indices array of indices of size 7
		/// </summary>
		public LegacyUrlMarker SetIndices(int[] indices)
		{
			if (indices == null || indices.Length != 7)
			{
				throw new Exception("Malformed index array.");
			}

			SetIndex(LegacyUrlPart.SCHEME, indices[0]);
			SetIndex(LegacyUrlPart.USERNAME_PASSWORD, indices[1]);
			SetIndex(LegacyUrlPart.HOST, indices[2]);
			SetIndex(LegacyUrlPart.PORT, indices[3]);
			SetIndex(LegacyUrlPart.PATH, indices[4]);
			SetIndex(LegacyUrlPart.QUERY, indices[5]);
			SetIndex(LegacyUrlPart.FRAGMENT, indices[6]);
			return this;
		}
	}
}