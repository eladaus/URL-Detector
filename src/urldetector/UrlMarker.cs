using System;

namespace urldetector
{
	public class UrlMarker
	{
		private int _fragmentIndex = -1;
		private int _hostIndex = -1;

		private string _originalUrl;
		private int _pathIndex = -1;
		private int _portIndex = -1;
		private int _queryIndex = -1;
		private int _schemeIndex = -1;
		private int _usernamePasswordIndex = -1;

		public Url CreateUrl()
		{
			return new Url(this);
		}

		public void SetOriginalUrl(string originalUrl)
		{
			_originalUrl = originalUrl;
		}

		public string GetOriginalUrl()
		{
			return _originalUrl;
		}

		public void SetIndex(UrlPart urlPart, int index)
		{
			switch (urlPart)
			{
				case UrlPart.SCHEME:
					_schemeIndex = index;
					break;
				case UrlPart.USERNAME_PASSWORD:
					_usernamePasswordIndex = index;
					break;
				case UrlPart.HOST:
					_hostIndex = index;
					break;
				case UrlPart.PORT:
					_portIndex = index;
					break;
				case UrlPart.PATH:
					_pathIndex = index;
					break;
				case UrlPart.QUERY:
					_queryIndex = index;
					break;
				case UrlPart.FRAGMENT:
					_fragmentIndex = index;
					break;
			}
		}

		/// <summary>
		/// @param urlPart The part you want the index of
		/// @return Returns the index of the part
		/// </summary>
		public int IndexOf(UrlPart urlPart)
		{
			switch (urlPart)
			{
				case UrlPart.SCHEME:
					return _schemeIndex;
				case UrlPart.USERNAME_PASSWORD:
					return _usernamePasswordIndex;
				case UrlPart.HOST:
					return _hostIndex;
				case UrlPart.PORT:
					return _portIndex;
				case UrlPart.PATH:
					return _pathIndex;
				case UrlPart.QUERY:
					return _queryIndex;
				case UrlPart.FRAGMENT:
					return _fragmentIndex;
				default:
					return -1;
			}
		}

		public void UnsetIndex(UrlPart urlPart)
		{
			SetIndex(urlPart, -1);
		}

		/// <summary>
		/// This is used in TestUrlMarker to set indices more easily.
		/// @param indices array of indices of size 7
		/// </summary>
		public UrlMarker SetIndices(int[] indices)
		{
			if (indices == null || indices.Length != 7)
			{
				throw new Exception("Malformed index array.");
			}

			SetIndex(UrlPart.SCHEME, indices[0]);
			SetIndex(UrlPart.USERNAME_PASSWORD, indices[1]);
			SetIndex(UrlPart.HOST, indices[2]);
			SetIndex(UrlPart.PORT, indices[3]);
			SetIndex(UrlPart.PATH, indices[4]);
			SetIndex(UrlPart.QUERY, indices[5]);
			SetIndex(UrlPart.FRAGMENT, indices[6]);
			return this;
		}
	}
}