using System;
using System.Collections.Generic;
using System.Text;
using urldetector.eladaus;

namespace urldetector.detection
{
	public class UrlDetector
	{
		/// <summary>
		/// The states to use to continue writing or not.
		/// </summary>
		private enum ReadEndState
		{
			/// <summary>
			/// The current url is valid.
			/// </summary>
			ValidUrl,

			/// <summary>
			/// The current url is invalid.
			/// </summary>
			InvalidUrl
		}

		///// <summary>
		///// Contains the string to check for and remove if the scheme is this.
		///// </summary>
		//private static readonly string HTML_MAILTO = "mailto:";

		/// <summary>
		/// Valid protocol schemes. 
		/// </summary>
		private HashSet<string> ValidSchemesSuffixed { get; } = new HashSet<string>();
		private HashSet<string> ValidSchemesNames { get; } = new HashSet<string>();
		
		
		/// <summary>
		/// Take a list of strings like 'ftp', 'http', 'attachment' and append them as a full
		/// searchable instance to the collection of schemes to find in the input, like
		/// 'ftp://', 'ftp%3a//', 'http://', 'http%3a//' etc
		/// </summary>
		/// <param name="validSchemes"></param>
		private void SetValidSchemes(IEnumerable<string> validSchemes)
		{
			ValidSchemesNames.Clear();
			ValidSchemesSuffixed.Clear();

			foreach (var validScheme in validSchemes)
			{
				var lowerInvariant = validScheme.Trim().ToLowerInvariant();
				ValidSchemesNames.Add(lowerInvariant);
				ValidSchemesSuffixed.Add(lowerInvariant + "://");
				ValidSchemesSuffixed.Add(lowerInvariant + "%3a//");
			}
		}

		/// <summary>
		/// Return a readonly copy of the schemes that the UrlDetector is currently configured to detect
		/// </summary>
		/// <returns></returns>
		public HashSet<string> ListValidSchemes()
		{
			return new HashSet<string>(ValidSchemesNames);
		}


		/// <summary>
		/// Stores options for detection.
		/// </summary>
		private readonly UrlDetectorOptions _options;

		/// <summary>
		/// The input stream to read.
		/// </summary>
		private readonly InputTextReader _reader;


		/// <summary>
		/// Buffer to store temporary urls inside of.
		/// </summary>
		private readonly StringBuilder _buffer = new StringBuilder();

		/// <summary>
		/// Keeps the count of special characters used to match quotes and different types of brackets.
		/// </summary>
		private readonly Dictionary<char, int> _characterMatch = new Dictionary<char, int>();

		/// <summary>
		/// Keeps track of certain indices to create a Url object.
		/// </summary>
		private UrlMarker _currentUrlMarker = new UrlMarker();

		/// <summary>
		/// If we see a '[', didn't find an ipv6 address, and the bracket option is on, then look for urls inside the brackets.
		/// </summary>
		private bool _dontMatchIpv6;


		/// <summary>
		/// Has the scheme been found in this iteration?
		/// </summary>
		private bool _hasScheme;


		/// <summary>
		/// If the first character in the url is a quote, then look for matching quote at the end.
		/// </summary>
		private bool _quoteStart;

		/// <summary>
		/// If the first character in the url is a single quote, then look for matching quote at the end.
		/// </summary>
		private bool _singleQuoteStart;

		/// <summary>
		/// Stores the found urls.
		/// </summary>
		private readonly List<Url> _urlList = new List<Url>();


		/// <summary>
		/// Creates a new UrlDetector object used to find urls inside of text.
		/// @param content The content to search inside of.
		/// @param options The UrlDetectorOptions to use when detecting the content.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="options"></param>
		/// <param name="validSchemes"></param>
		public UrlDetector(string content, UrlDetectorOptions options, HashSet<string> validSchemes = null)
		{
			_reader = new InputTextReader(content);
			_options = options;

			if (validSchemes == null || validSchemes.Count == 0) validSchemes = new HashSet<string>
			{
				"http", "https", "ftp", "ftps"
			};

			SetValidSchemes(validSchemes);
		}


		/// <summary>
		/// Detects the urls and returns a list of detected url strings.
		/// @return A list with detected urls.
		/// </summary>
		/// <returns></returns>
		public List<Url> Detect()
		{
			ReadDefault();
			return _urlList;
		}


		/// <summary>
		/// The default input reader which looks for specific flags to start detecting the url.
		/// </summary>
		private void ReadDefault()
		{
			//Keeps track of the number of characters read to be able to later cut out the domain name.
			var length = 0;

			//until end of string read the contents
			while (!_reader.Eof())
			{
				//read the next char to process.
				var curr = _reader.Read();

				switch (curr)
				{
					case ' ':
						//space was found, check if it's a valid single level domain.
						if (_options.HasFlag(UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN) && _buffer.Length > 0 && _hasScheme)
						{
							_reader.GoBack();
							if (!ReadDomainName(_buffer.ToString().Substring(length)))
							{
								ReadEnd(ReadEndState.InvalidUrl);
							};
						}

						_buffer.Append(curr);
						ReadEnd(ReadEndState.InvalidUrl);
						length = 0;
						break;
					case '%':
						if (_reader.CanReadChars(2))
						{
							if (_reader.Peek(2).Equals("3a", StringComparison.InvariantCultureIgnoreCase))
							{
								_buffer.Append(curr);
								_buffer.Append(_reader.Read());
								_buffer.Append(_reader.Read());
								length = ProcessColon(length);
							}
							else if (CharUtils.IsHex(_reader.PeekChar(0)) && CharUtils.IsHex(_reader.PeekChar(1)))
							{
								_buffer.Append(curr);
								_buffer.Append(_reader.Read());
								_buffer.Append(_reader.Read());

								if (!ReadDomainName(_buffer.ToString().Substring(length)))
								{
									ReadEnd(ReadEndState.InvalidUrl);
								}
								length = 0;
							}
						}

						break;
					case '\u3002': //non-standard dots
					case '\uFF0E':
					case '\uFF61':
					case '.': //"." was found, read the domain name using the start from length.
						_buffer.Append(curr);
						if (!ReadDomainName(_buffer.ToString().Substring(length)))
						{
							ReadEnd(ReadEndState.InvalidUrl);
						}
						length = 0;
						break;
					case '@': //Check the domain name after a username
						if (_buffer.Length > 0)
						{
							_currentUrlMarker.SetIndex(UrlPart.USERNAME_PASSWORD, length);
							_buffer.Append(curr);
							if (!ReadDomainName(null))
							{
								ReadEnd(ReadEndState.InvalidUrl);
							}
							length = 0;
						}

						break;
					case '[':
						if (_dontMatchIpv6)
						{
							//Check if we need to match characters. If we match characters and this is a start or stop of range,
							//either way reset the world and start processing again.
							if (CheckMatchingCharacter(curr) != CharacterMatch.CharacterNotMatched)
							{
								ReadEnd(ReadEndState.InvalidUrl);
								length = 0;
							}
						}

						var beginning = _reader.GetPosition();

						//if it doesn't have a scheme, clear the buffer.
						if (!_hasScheme)
						{
							_buffer.Remove(0, _buffer.Length);
						}

						_buffer.Append(curr);

						if (!ReadDomainName(_buffer.ToString().Substring(length)))
						{
							//if we didn't find an ipv6 address, then check inside the brackets for urls
							ReadEnd(ReadEndState.InvalidUrl);
							_reader.Seek(beginning);
							_dontMatchIpv6 = true;
						}

						length = 0;
						break;
					case '/':
						// "/" was found, then we either read a scheme, or if we already read a scheme, then
						// we are reading a url in the format http://123123123/asdf

						if (_hasScheme || _options.HasFlag(UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN) && _buffer.Length > 1)
						{
							//we already have the scheme, so then we already read:
							//http://something/ <- if something is all numeric then its a valid url.
							//OR we are searching for single level domains. We have buffer length > 1 condition
							//to weed out infinite backtrack in cases of html5 roots

							//unread this "/" and continue to check the domain name starting from the beginning of the domain
							_reader.GoBack();
							if (!ReadDomainName(_buffer.ToString().Substring(length)))
							{
								ReadEnd(ReadEndState.InvalidUrl);
							}

							length = 0;
						}
						else
						{
							//we don't have a scheme already, then clear state, then check for html5 root such as: "//google.com/"
							// remember the state of the quote when clearing state just in case its "//google.com" so its not cleared.
							ReadEnd(ReadEndState.InvalidUrl);
							_buffer.Append(curr);
							_hasScheme = ReadHtml5Root();
							length = _buffer.Length;
						}

						break;
					case ':':
						//add the ":" to the url and check for scheme/username
						_buffer.Append(curr);
						length = ProcessColon(length);
						break;
					default:
						//Check if we need to match characters. If we match characters and this is a start or stop of range,
						//either way reset the world and start processing again.
						if (CheckMatchingCharacter(curr) != CharacterMatch.CharacterNotMatched)
						{
							ReadEnd(ReadEndState.InvalidUrl);
							length = 0;
						}
						else
						{
							_buffer.Append(curr);
						}

						break;
				}
			}

			if (_options.HasFlag(UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN) && _buffer.Length > 0 && _hasScheme)
			{
				if (!ReadDomainName(_buffer.ToString().Substring(length)))
				{
					ReadEnd(ReadEndState.InvalidUrl);
				}
			}
		}

		
		/// <summary>
		/// We found a ":" and is now trying to read either scheme, username/password
		/// </summary>
		/// <param name="length">first index of the previous part (could be beginning of the buffer, beginning of the username/password, or beginning</param>
		/// <returns>new index of where the domain starts</returns>
		private int ProcessColon(int length)
		{
			if (_hasScheme)
			{
				//read it as username/password if it has scheme
				if (!ReadUserPass(length))
				{
					//unread the ":" so that the domain reader can process it
					_reader.GoBack();

					// Check buffer length before clearing it; set length to 0 if buffer is empty
					if (_buffer.Length > 0)
					{
						_buffer.Remove(_buffer.Length - 1, 1);
					}
					else
					{
						length = 0;
					}

					var backtrackOnFail = _reader.GetPosition() - _buffer.Length + length;
					if (!ReadDomainName(_buffer.ToString().Substring(length)))
					{
						//go back to length location and restart search
						_reader.Seek(backtrackOnFail);
						ReadEnd(ReadEndState.InvalidUrl);
					}

					length = 0;
				}
				else
				{
					length = 0;
				}
			}
			else if (ReadScheme() && _buffer.Length > 0)
			{
				_hasScheme = true;
				length = _buffer.Length; //set length to be right after the scheme
			}
			else if (_buffer.Length > 0 && _options.HasFlag(UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN)
			                            && _reader.CanReadChars(1))
			{
				//takes care of case like hi:
				_reader.GoBack(); //unread the ":" so readDomainName can take care of the port
				_buffer.Remove(_buffer.Length - 1, 1);
				if (!ReadDomainName(_buffer.ToString()))
				{
					ReadEnd(ReadEndState.InvalidUrl);
				}
			}
			else
			{
				ReadEnd(ReadEndState.InvalidUrl);
				length = 0;
			}

			return length;
		}

		/// <summary>
		/// Gets the number of times the current character was seen in the document. Only special characters are tracked.
		/// @param curr The character to look for.
		/// @return The number of times that character was seen
		/// </summary>
		private int GetCharacterCount(char curr)
		{
			return (_characterMatch.ContainsKey(curr)) ? _characterMatch[curr] : 0;
		}

		/// <summary>
		/// Increments the counter for the characters seen and return if this character matches a special character
		/// that might require stopping reading the url.
		/// @param curr The character to check.
		/// @return The state that this character requires.
		/// </summary>
		private CharacterMatch CheckMatchingCharacter(char curr)
		{
			//This is a quote and we are matching quotes.
			if (curr == '\"' && _options.HasFlag(UrlDetectorOptions.QUOTE_MATCH)
			    || curr == '\'' && _options.HasFlag(UrlDetectorOptions.SINGLE_QUOTE_MATCH))
			{
				bool quoteStart;
				if (curr == '\"')
				{
					quoteStart = _quoteStart;

					//remember that a double quote was found.
					_quoteStart = true;
				}
				else
				{
					quoteStart = _singleQuoteStart;

					//remember that a single quote was found.
					_singleQuoteStart = true;
				}

				//increment the number of quotes found.
				var currVal = GetCharacterCount(curr) + 1;
				_characterMatch.IncrementCount(curr, currVal);

				//if there was already a quote found, or the number of quotes is even, return that we have to stop, else its a start.
				return quoteStart || currVal % 2 == 0 ? CharacterMatch.CharacterMatchStop : CharacterMatch.CharacterMatchStart;
			}

			if (_options.HasFlag(UrlDetectorOptions.BRACKET_MATCH) && (curr == '[' || curr == '{' || curr == '('))
			{
				//Look for start of bracket
				_characterMatch.IncrementCount(curr, GetCharacterCount(curr) + 1);
				return CharacterMatch.CharacterMatchStart;
			}

			if (_options.HasFlag(UrlDetectorOptions.XML) && curr == '<')
			{
				//If its html, look for "<"
				_characterMatch.IncrementCount(curr, GetCharacterCount(curr) + 1);
				return CharacterMatch.CharacterMatchStart;
			}

			if (_options.HasFlag(UrlDetectorOptions.BRACKET_MATCH) && (curr == ']' || curr == '}' || curr == ')')
			    || _options.HasFlag(UrlDetectorOptions.XML) && curr == '>')
			{
				//If we catch a end bracket increment its count and get rid of not ipv6 flag
				var currVal = GetCharacterCount(curr) + 1;
				_characterMatch.IncrementCount(curr, currVal);

				//now figure out what the start bracket was associated with the closed bracket.
				var match = '\0';
				switch (curr)
				{
					case ']':
						match = '[';
						break;
					case '}':
						match = '{';
						break;
					case ')':
						match = '(';
						break;
					case '>':
						match = '<';
						break;
				}

				//If the number of open is greater then the number of closed, return a stop.
				return GetCharacterCount(match) > currVal
					? CharacterMatch.CharacterMatchStop
					: CharacterMatch.CharacterMatchStart;
			}

			//Nothing else was found.
			return CharacterMatch.CharacterNotMatched;
		}

		/// <summary>
		/// Checks if the url is in the format:
		/// //google.com/static/js.js
		/// @return True if the url is in this format and was matched correctly.
		/// </summary>
		private bool ReadHtml5Root()
		{
			//end of input then go away.
			if (_reader.Eof())
			{
				return false;
			}

			//read the next character. If its // then return true.
			var curr = _reader.Read();
			if (curr == '/')
			{
				_buffer.Append(curr);
				return true;
			}

			//if its not //, then go back and reset by 1 character.
			_reader.GoBack();
			ReadEnd(ReadEndState.InvalidUrl);
			return false;
		}

		/// <summary>
		/// Reads the scheme and allows returns true if the scheme is in our allowed collection (e.g. http(s?):// or ftp(s?)://)
		/// @return True if the scheme was found, else false.
		/// </summary>
		private bool ReadScheme()
		{
			//////Check if we are checking html and the length is longer than mailto:
			////if (_options.HasFlag(UrlDetectorOptions.HTML) && _buffer.Length >= HTML_MAILTO.Length)
			////{
			////	//Check if the string is actually mailto: then just return nothing.
			////	if (HTML_MAILTO.Equals(_buffer.ToString().Substring(_buffer.Length - HTML_MAILTO.Length), StringComparison.CurrentCultureIgnoreCase))
			////	{
			////		return ReadEnd(ReadEndState.InvalidUrl);
			////	}
			////}

			var originalLength = _buffer.Length;
			var numSlashes = 0;

			while (!_reader.Eof())
			{
				var curr = _reader.Read();

				//if we match a slash, look for a second one.
				if (curr == '/')
				{
					_buffer.Append(curr);
					if (numSlashes == 1)
					{
						//return only if its an approved protocol. This can be expanded to allow others
						if (ValidSchemesSuffixed.Contains(_buffer.ToString().ToLowerInvariant()))
						{
							_currentUrlMarker.SetIndex(UrlPart.SCHEME, 0);
							return true;
						}

						return false;
					}

					numSlashes++;
				}
				else if (curr == ' ' || CheckMatchingCharacter(curr) != CharacterMatch.CharacterNotMatched)
				{
					//if we find a space or end of input, then nothing found.
					_buffer.Append(curr);
					return false;
				}
				else if (curr == '[')
				{
					//if we're starting to see an ipv6 address
					_reader.GoBack(); //unread the '[', so that we can start looking for ipv6
					return false;
				}
				else if (originalLength > 0 || numSlashes > 0 || !CharUtils.IsAlpha(curr))
				{
					// if it's not a character a-z or A-Z then assume we aren't matching scheme, but instead
					// matching username and password.
					_reader.GoBack();
					return ReadUserPass(0);
				}
			}

			return false;
		}


		/// <summary>
		/// Reads the input and looks for a username and password.
		/// Handles:
		/// http://username:password@...
		/// @return True if a valid username and password was found.
		/// </summary>
		/// <param name="beginningOfUsername">beginningOfUsername Index of the buffer of where the username began</param>
		/// <returns></returns>
		private bool ReadUserPass(int beginningOfUsername)
		{
			//The start of where we are.
			var start = _buffer.Length;

			//keep looping until "done"
			var done = false;

			//if we had a dot in the input, then it might be a domain name and not a username and password.
			var rollback = false;
			while (!done && !_reader.Eof())
			{
				var curr = _reader.Read();

				// if we hit this, then everything is ok and we are matching a domain name.
				if (curr == '@')
				{
					_buffer.Append(curr);
					_currentUrlMarker.SetIndex(UrlPart.USERNAME_PASSWORD, beginningOfUsername);
					return ReadDomainName("");
				}

				if (CharUtils.IsDot(curr) || curr == '[')
				{
					//everything is still ok, just remember that we found a dot or '[' in case we might need to backtrack
					_buffer.Append(curr);
					rollback = true;
				}
				else if (curr == '#' || curr == ' ' || curr == '/'
				         || CheckMatchingCharacter(curr) != CharacterMatch.CharacterNotMatched)
				{
					//one of these characters indicates we are invalid state and should just return.
					rollback = true;
					done = true;
				}
				else
				{
					//all else, just append character assuming its ok so far.
					_buffer.Append(curr);
				}
			}

			if (rollback)
			{
				//got to here, so there is no username and password. (We didn't find a @)
				var distance = _buffer.Length - start;
				_buffer.Remove(start, _buffer.Length - start);

				var currIndex = Math.Max(_reader.GetPosition() - distance - (done ? 1 : 0), 0);
				_reader.Seek(currIndex);

				return false;
			}

			return ReadEnd(ReadEndState.InvalidUrl);
		}


		/// <summary>
		/// Try to read the current string as a domain name
		/// @return Whether the domain is valid or not.
		/// </summary>
		/// <param name="current">current The current string used</param>
		/// <returns>Whether the domain is valid or not.</returns>
		private bool ReadDomainName(string current)
		{
			var hostIndex = current == null ? _buffer.Length : _buffer.Length - current.Length;
			_currentUrlMarker.SetIndex(UrlPart.HOST, hostIndex);

			//create the domain name reader and specify the handler that will be called when a quote character
			//or something is found.

			// Dale port of Java nested anonymous function, passing an action
			var reader = new DomainNameReader(_reader, _buffer, current, _options, c => CheckMatchingCharacter(c));

			//Try to read the dns and act on the response.
			var state = reader.ReadDomainName();
			switch (state)
			{
				case DomainNameReader.ReaderNextState.ValidDomainName:
					return ReadEnd(ReadEndState.ValidUrl);
				case DomainNameReader.ReaderNextState.ReadFragment:
					return ReadFragment();
				case DomainNameReader.ReaderNextState.ReadPath:
					return ReadPath();
				case DomainNameReader.ReaderNextState.ReadPort:
					return ReadPort();
				case DomainNameReader.ReaderNextState.ReadUserPass:
					int host = _currentUrlMarker.IndexOf(UrlPart.HOST);
					_currentUrlMarker.UnsetIndex(UrlPart.HOST);
					return ReadUserPass(host);
				case DomainNameReader.ReaderNextState.ReadQueryString:
					return ReadQueryString();
				default:
					return false;
			}
		}


		/// <summary>
		/// Reads the fragments which is the part of the url starting with #
		/// </summary>
		/// <returns>If a valid fragment was read true, else false.</returns>
		private bool ReadFragment()
		{
			_currentUrlMarker.SetIndex(UrlPart.FRAGMENT, _buffer.Length - 1);

			while (!_reader.Eof())
			{
				var curr = _reader.Read();

				//if it's the end or space, then a valid url was read.
				if (curr == ' ' || CheckMatchingCharacter(curr) != CharacterMatch.CharacterNotMatched)
				{
					return ReadEnd(ReadEndState.ValidUrl);
				}

				//otherwise keep appending.
				_buffer.Append(curr);
			}

			//if we are here, anything read is valid.
			return ReadEnd(ReadEndState.ValidUrl);
		}


		/// <summary>
		/// Try to read the query string.
		/// </summary>
		/// <returns>True if the query string was valid.</returns>
		private bool ReadQueryString()
		{
			_currentUrlMarker.SetIndex(UrlPart.QUERY, _buffer.Length - 1);

			while (!_reader.Eof())
			{
				var curr = _reader.Read();

				if (curr == '#')
				{
					//fragment
					_buffer.Append(curr);
					return ReadFragment();
				}

				if (curr == ' ' || CheckMatchingCharacter(curr) != CharacterMatch.CharacterNotMatched)
				{
					//end of query string
					return ReadEnd(ReadEndState.ValidUrl);
				}

				//all else add to buffer.
				_buffer.Append(curr);
			}

			//a valid url was read.
			return ReadEnd(ReadEndState.ValidUrl);
		}


		/// <summary>
		/// Try to read the port of the url.
		/// </summary>
		/// <returns>True if a valid port was read.</returns>
		private bool ReadPort()
		{
			_currentUrlMarker.SetIndex(UrlPart.PORT, _buffer.Length);
			//The length of the port read.
			var portLen = 0;
			while (!_reader.Eof())
			{
				//read the next one and remember the length
				var curr = _reader.Read();
				portLen++;

				if (curr == '/')
				{
					//continue to read path
					_buffer.Append(curr);
					return ReadPath();
				}

				if (curr == '?')
				{
					//continue to read query string
					_buffer.Append(curr);
					return ReadQueryString();
				}

				if (curr == '#')
				{
					//continue to read fragment.
					_buffer.Append(curr);
					return ReadFragment();
				}

				if (CheckMatchingCharacter(curr) == CharacterMatch.CharacterMatchStop || !CharUtils.IsNumeric(curr))
				{
					//if we got here, then what we got so far is a valid url. don't append the current character.
					_reader.GoBack();

					//no port found; it was something like google.com:hello.world
					if (portLen == 1)
					{
						//remove the ":" from the end.
						_buffer.Remove(_buffer.Length - 1, 1);
					}

					_currentUrlMarker.UnsetIndex(UrlPart.PORT);
					return ReadEnd(ReadEndState.ValidUrl);
				}

				//this is a valid character in the port string.
				_buffer.Append(curr);
			}

			//found a correct url
			return ReadEnd(ReadEndState.ValidUrl);
		}


		/// <summary>
		/// Tries to read the path
		/// </summary>
		/// <returns>True if the path is valid.</returns>
		private bool ReadPath()
		{
			_currentUrlMarker.SetIndex(UrlPart.PATH, _buffer.Length - 1);
			while (!_reader.Eof())
			{
				//read the next char
				var curr = _reader.Read();

				if (curr == ' ' || CheckMatchingCharacter(curr) != CharacterMatch.CharacterNotMatched)
				{
					//if end of state and we got here, then the url is valid.
					return ReadEnd(ReadEndState.ValidUrl);
				}

				//append the char
				_buffer.Append(curr);

				//now see if we move to another state.
				if (curr == '?')
				{
					//if ? read query string
					return ReadQueryString();
				}

				if (curr == '#')
				{
					//if # read the fragment
					return ReadFragment();
				}
			}

			//end of input then this url is good.
			return ReadEnd(ReadEndState.ValidUrl);
		}


		/// <summary>
		/// The url has been read to here. Remember the url if its valid, and reset state.
		/// </summary>
		/// <param name="state">The state indicating if this url is valid. If its valid it will be added to the list of urls.</param>
		/// <returns>True if the url was valid</returns>
		private bool ReadEnd(ReadEndState state)
		{
			//if the url is valid and greater then 0
			if (state == ReadEndState.ValidUrl && _buffer.Length > 0)
			{
				//get the last character. if its a quote, cut it off.
				var len = _buffer.Length;
				var startIndex = len - 1;
				if (_quoteStart && _buffer[startIndex] == '\"')
				{
					_buffer.Remove(startIndex, len - startIndex);
				}

				//Add the url to the list of good urls.
				if (_buffer.Length > 0)
				{
					_currentUrlMarker.SetOriginalUrl(_buffer.ToString());
					_urlList.Add(_currentUrlMarker.CreateUrl());
				}
			}

			//clear out the buffer.
			_buffer.Remove(0, _buffer.Length);

			//reset the state of internal objects.
			_quoteStart = false;
			_hasScheme = false;
			_dontMatchIpv6 = false;
			_currentUrlMarker = new UrlMarker();

			//return true if valid.
			return state == ReadEndState.ValidUrl;
		}


		/// <summary>
		/// The response of character matching.
		/// </summary>
		private enum CharacterMatch
		{
			/// <summary>
			/// The character was not matched.
			/// </summary>
			CharacterNotMatched,

			/// <summary>
			/// A character was matched with requires a stop.
			/// </summary>
			CharacterMatchStop,

			/// <summary>
			/// The character was matched which is a start of parentheses.
			/// </summary>
			CharacterMatchStart
		}
	}
}