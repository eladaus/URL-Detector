using System;
using System.Collections.Generic;
using System.Text;
using urldetector.legacy.detection;
using urldetector.legacy.eladaus;

namespace urldetector.legacy
{
	/// <summary>
	/// Creating own Uri class since java.net.Uri would throw parsing exceptions
	/// for URL's considered ok by browsers.
	/// Also to avoid further conflict, this does stuff that the normal Uri object doesn't do:
	/// - Converts http://google.com/a/b/.//./../c to http://google.com/a/c
	/// - Decodes repeatedly so that http://host/%2525252525252525 becomes http://host/%25 while normal decoders
	/// would make it http://host/%25252525252525 (one less 25)
	/// - Removes tabs and new lines: http://www.google.com/foo\tbar\rbaz\n2 becomes "http://www.google.com/foobarbaz2"
	/// - Converts IP addresses: http://3279880203/blah becomes http://195.127.0.11/blah
	/// - Strips fragments (anything after #)
	/// </summary>
	public class LegacyUrl
	{
		private static readonly string DEFAULT_SCHEME = "http";
		private static readonly Dictionary<string, int> SCHEME_PORT_MAP;
		private string _fragment;
		private string _host;
		private readonly string _originalUrl;
		private string _password;
		private string _path;
		private int _port;
		private string _query;
		private string _scheme;


		private readonly LegacyUrlMarker _urlMarker;
		private string _username;

		static LegacyUrl()
		{
			SCHEME_PORT_MAP = new Dictionary<string, int>();
			SCHEME_PORT_MAP.Add("http", 80);
			SCHEME_PORT_MAP.Add("https", 443);
			SCHEME_PORT_MAP.Add("ftp", 21);
		}

		protected internal LegacyUrl(LegacyUrlMarker urlMarker)
		{
			_urlMarker = urlMarker;
			_originalUrl = urlMarker.GetOriginalUrl();
		}


		/// <summary>
		/// Returns a url given a single url.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static LegacyUrl Create(string url)
		{
			var formattedString = LegacyUrlUtil.RemoveSpecialSpaces(url.Trim().Replace(" ", "%20"));
			var urls = new LegacyUrlDetector(formattedString, LegacyUrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN).Detect();
			if (urls.Count == 1)
			{
				return urls[0];
			}

			if (urls.Count == 0)
			{
				throw new LegacyMalformedUrlException("We couldn't find any urls in string: " + url);
			}

			throw new LegacyMalformedUrlException("We found more than one url in string: " + url);
		}


		/// <summary>
		/// Returns a normalized url given a url object
		/// </summary>
		/// <returns></returns>
		public LegacyNormalizedUrl Normalize()
		{
			return new LegacyNormalizedUrl(_urlMarker);
		}


		/// <summary>
		/// Note that this includes the fragment
		/// @return Formats the url to: [scheme]://[username]:[password]@[host]:[port]/[path]?[query]#[fragment]
		/// </summary>
		/// <returns></returns>
		public string GetFullUrl()
		{
			var fragment = GetFragment();
			if (string.IsNullOrEmpty(fragment))
			{
				fragment = string.Empty;
			}
			return GetFullUrlWithoutFragment() + fragment;
		}


		/// <summary>
		/// Formats the url to: [scheme]://[username]:[password]@[host]:[port]/[path]?[query]
		/// </summary>
		/// <returns></returns>
		public string GetFullUrlWithoutFragment()
		{
			var url = new StringBuilder();
			if (!string.IsNullOrEmpty(GetScheme()))
			{
				url.Append(GetScheme());
				url.Append(":");
			}

			url.Append("//");

			if (!string.IsNullOrEmpty(GetUsername()))
			{
				url.Append(GetUsername());
				if (!string.IsNullOrEmpty(GetPassword()))
				{
					url.Append(":");
					url.Append(GetPassword());
				}

				url.Append("@");
			}

			url.Append(GetHost());
			if (GetPort() > 0 && GetPort() != SCHEME_PORT_MAP[GetScheme()])
			{
				url.Append(":");
				url.Append(GetPort());
			}

			url.Append(GetPath());
			url.Append(GetQuery());

			return url.ToString();
		}

		public string GetScheme()
		{
			if (_scheme == null)
			{
				if (Exists(LegacyUrlPart.SCHEME))
				{
					// Start with whatever (possibly dirty) scheme data the parser gave us. 
					_scheme = GetPart(LegacyUrlPart.SCHEME);

					var schemeLowered = _scheme.ToLowerInvariant();

					// See if the parser handed us a dirty scheme, e.g. input of ':u(https://test.co' -> scheme of ':u(https://'
					
					// Most of the time, we assume we got a clean one, e.g. 'http://' for hashset lookup speed:
					if (LegacyUriSchemeLookup.UriSchemeNamesSuffixed.Contains(schemeLowered))
					{
						_scheme = LegacyUriSchemeLookup.DesuffixUriScheme(_scheme);
					}
					else 
					{
						// Otherwise, we got a dirty one, lets find the longest matching suffix we can (e.g. sftp:// and ftp:// would
						// both match a dirty input of ':u(sftp://mysite.com' but obviously we want sftp as the longer, more accurate match
						for (var i = LegacyUriSchemeLookup.UriSchemeNamesSuffixedOrdered.Count-1; i >= 0; i--)
						{
							var compareScheme = LegacyUriSchemeLookup.UriSchemeNamesSuffixedOrdered[i];
							if (schemeLowered.EndsWith(compareScheme))
							{
								_scheme = _scheme.Remove(0, _scheme.Length - compareScheme.Length);
								_scheme = LegacyUriSchemeLookup.DesuffixUriScheme(_scheme);
								break;
							}
						}
					}
				}
				else if (!_originalUrl.StartsWith("//"))
				{
					_scheme = DEFAULT_SCHEME;
				}
			}

			return string.IsNullOrEmpty(_scheme) ? string.Empty : _scheme;
		}

		public string GetUsername()
		{
			if (_username == null)
			{
				PopulateUsernamePassword();
			}

			return string.IsNullOrEmpty(_username) ? string.Empty : _username;
		}

		public string GetPassword()
		{
			if (_password == null)
			{
				PopulateUsernamePassword();
			}

			return string.IsNullOrEmpty(_password) ? string.Empty : _password;
		}

		public virtual string GetHost()
		{
			if (_host == null)
			{
				_host = GetPart(LegacyUrlPart.HOST);
				if (Exists(LegacyUrlPart.PORT))
				{
					_host = _host.Substring(0, _host.Length - 1);
				}
			}

			return _host;
		}


		/// <summary>
		/// port = 0 means it hasn't been set yet. port = -1 means there is no port
		/// </summary>
		/// <returns></returns>
		public int GetPort()
		{
			if (_port == 0)
			{
				var portString = GetPart(LegacyUrlPart.PORT);
				if (portString != null && !string.IsNullOrEmpty(portString))
				{
					var wasParsed = Int32.TryParse(portString, out _port);
					if (!wasParsed)
					{
						_port = -1;
					}
				}
				else if (SCHEME_PORT_MAP.ContainsKey(GetScheme()))
				{
					_port = SCHEME_PORT_MAP[GetScheme()];
				}
				else
				{
					_port = -1;
				}
			}

			return _port;
		}


		public virtual string GetPath()
		{
			if (_path == null)
			{
				_path = Exists(LegacyUrlPart.PATH) ? GetPart(LegacyUrlPart.PATH) : "/";
			}

			return _path;
		}


		public string GetQuery()
		{
			if (_query == null)
			{
				_query = GetPart(LegacyUrlPart.QUERY);
			}

			return string.IsNullOrEmpty(_query) ? string.Empty : _query;
		}


		public string GetFragment()
		{
			if (_fragment == null)
			{
				_fragment = GetPart(LegacyUrlPart.FRAGMENT);
			}

			return string.IsNullOrEmpty(_fragment) ? string.Empty : _fragment;
		}


		/// <summary>
		/// Always returns null for non normalized urls.
		/// </summary>
		/// <returns></returns>
		public virtual byte[] GetHostBytes()
		{
			return null;
		}


		public string GetOriginalUrl()
		{
			return _originalUrl;
		}


		private void PopulateUsernamePassword()
		{
			if (Exists(LegacyUrlPart.USERNAME_PASSWORD))
			{
				var usernamePassword = GetPart(LegacyUrlPart.USERNAME_PASSWORD);
				var usernamePasswordParts = usernamePassword.Substring(0, usernamePassword.Length - 1).Split(':');
				if (usernamePasswordParts.Length == 1)
				{
					_username = usernamePasswordParts[0];
				}
				else if (usernamePasswordParts.Length == 2)
				{
					_username = usernamePasswordParts[0];
					_password = usernamePasswordParts[1];
				}
			}
		}


		/// <summary>
		/// @param urlPart The url part we are checking for existence
		/// @return Returns true if the part exists.
		/// </summary>
		/// <param name="urlPart"></param>
		/// <returns></returns>
		private bool Exists(LegacyUrlPart? urlPart)
		{
			return urlPart != null && _urlMarker.IndexOf(urlPart.Value) >= 0;
		}


		/// <summary>
		/// For example, in http://yahoo.com/lala/, nextExistingPart(UrlPart.HOST) would return UrlPart.PATH
		/// @param urlPart The current url part
		/// @return Returns the next part; if there is no existing next part, it returns null
		/// </summary>
		/// <param name="urlPart"></param>
		/// <returns></returns>
		private LegacyUrlPart? NextExistingPart(LegacyUrlPart urlPart)
		{
			var nextPart = urlPart.GetNextPart();
			if (Exists(nextPart))
			{
				return nextPart;
			}

			if (nextPart == null)
			{
				return null;
			}

			return NextExistingPart(nextPart.Value);
		}


		/// <summary>
		/// @param part The part that we want. Ex: host, path
		/// </summary>
		/// <param name="part"></param>
		/// <returns></returns>
		private string GetPart(LegacyUrlPart part)
		{
			if (!Exists(part))
			{
				return null;
			}

			var nextPart = NextExistingPart(part);
			if (nextPart == null)
			{
				return _originalUrl.Substring(_urlMarker.IndexOf(part));
			}

			var beginIndex = _urlMarker.IndexOf(part);
			var endIndex = _urlMarker.IndexOf(nextPart.Value);
			return _originalUrl.Substring(beginIndex, endIndex - beginIndex);
		}


		protected void SetRawPath(string path)
		{
			_path = path;
		}


		protected void SetRawHost(string host)
		{
			_host = host;
		}


		protected string GetRawPath()
		{
			return _path;
		}

		protected string GetRawHost()
		{
			return _host;
		}

		protected LegacyUrlMarker GetUrlMarker()
		{
			return _urlMarker;
		}
	}
}