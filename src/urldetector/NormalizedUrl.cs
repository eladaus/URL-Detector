namespace urldetector
{
	/// <summary>
	/// Returns a normalized version of a url instead of the original url string.
	/// </summary>
	public class NormalizedUrl : Url
	{
		private byte[] _hostBytes;

		private bool _isPopulated;

		public NormalizedUrl(UrlMarker urlMarker) : base(urlMarker)
		{
		}


		/// <summary>
		/// Returns a normalized url given a single url.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public new static NormalizedUrl Create(string url) /*throws MalformedURLException*/
		{
			return Url.Create(url).Normalize();
		}


		public override string GetHost()
		{
			if (GetRawHost() == null)
			{
				PopulateHostAndHostBytes();
			}

			return GetRawHost();
		}


		public override string GetPath()
		{
			if (GetRawPath() == null)
			{
				SetRawPath(new PathNormalizer().NormalizePath(base.GetPath()));
			}

			return GetRawPath();
		}


		/// <summary>
		/// Returns the byte representation of the ip address. If the host is not an ip address, it returns null.
		/// </summary>
		/// <returns></returns>
		public override byte[] GetHostBytes()
		{
			if (_hostBytes == null)
			{
				PopulateHostAndHostBytes();
			}

			return _hostBytes;
		}


		private void PopulateHostAndHostBytes()
		{
			if (!_isPopulated)
			{
				var hostNormalizer = new HostNormalizer(base.GetHost());
				SetRawHost(hostNormalizer.GetNormalizedHost());
				_hostBytes = hostNormalizer.GetBytes();
				_isPopulated = true;
			}
		}
	}
}