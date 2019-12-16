using System;
using System.Globalization;
using System.Text;
using urldetector.legacy.eladaus;

namespace urldetector.legacy.detection
{
	/// <summary>
	/// The domain name reader reads input from a InputTextReader and validates if the content being read is a valid domain name.
	/// After a domain name is read, the returning status is what to do next. If the domain is valid but a specific character is found,
	/// the next state will be to read another part for the rest of the url. For example, if a "?" is found at the end and the
	/// domain is valid, the return state will be to read a query string.
	/// </summary>
	public class LegacyDomainNameReader
	{
		/// <summary>
		/// This is the readonly return state of reading a domain name.
		/// </summary>
		public enum ReaderNextState
		{
			/// <summary>
			/// Trying to read the domain name caused it to be invalid.
			/// </summary>
			InvalidDomainName,

			/// <summary>
			/// The domain name is found to be valid.
			/// </summary>
			ValidDomainName,

			/// <summary>
			/// Finished reading, next step should be to read the fragment.
			/// </summary>
			ReadFragment,

			/// <summary>
			/// Finished reading, next step should be to read the path.
			/// </summary>
			ReadPath,

			/// <summary>
			/// Finished reading, next step should be to read the port.
			/// </summary>
			ReadPort,

			/// <summary>
			/// Finished reading, next step should be to read the query string.
			/// </summary>
			ReadQueryString,

			/// <summary>
			/// This was actually not a domain at all.
			/// </summary>
			ReadUserPass
		}

		/// <summary>
		/// The minimum length of a ascii based top level domain.
		/// </summary>
		private static readonly int MIN_TOP_LEVEL_DOMAIN = 2;

		/// <summary>
		/// The maximum length of a ascii based top level domain.
		/// </summary>
		private static readonly int MAX_TOP_LEVEL_DOMAIN = 22;

		/// <summary>
		/// The maximum number that the url can be in a url that looks like:
		/// http://123123123123/path
		/// </summary>
		private static readonly long MAX_NUMERIC_DOMAIN_VALUE = 4294967295L;

		/// <summary>
		/// The minimum number the url can be in a url that looks like:
		/// http://123123123123/path
		/// </summary>
		private static readonly long MIN_NUMERIC_DOMAIN_VALUE = 16843008L;

		/// <summary>
		/// If the domain name is an ip address, for each part of the address, whats the minimum value?
		/// </summary>
		private static readonly int MIN_IP_PART = 0;

		/// <summary>
		/// If the domain name is an ip address, for each part of the address, whats the maximum value?
		/// </summary>
		private static readonly int MAX_IP_PART = 255;

		/// <summary>
		/// The start of the utf character code table which indicates that this character is an international character.
		/// Everything below this value is either a-z,A-Z,0-9 or symbols that are not included in domain name.
		/// </summary>
		private static readonly int INTERNATIONAL_CHAR_START = 192;

		/// <summary>
		/// The maximum length of each label in the domain name.
		/// </summary>
		private static readonly int MAX_LABEL_LENGTH = 64;

		/// <summary>
		/// The maximum number of labels in a single domain name.
		/// </summary>
		private static readonly int MAX_NUMBER_LABELS = 127;

		/// <summary>
		/// The maximum domain name length.
		/// </summary>
		private static readonly int MAX_DOMAIN_LENGTH = 255;

		/// <summary>
		/// Encoded hex dot.
		/// </summary>
		private static readonly string HEX_ENCODED_DOT = "2e";

		/// <summary>
		/// Contains the handler for each character match.
		/// </summary>
		//private readonly CharacterHandler _characterHandler;
		private readonly Action<char> _characterHandler;

		/// <summary>
		/// Contains the input stream to read.
		/// </summary>
		private readonly LegacyInputTextReader _reader;

		/// <summary>
		/// The currently written string buffer.
		/// </summary>
		private readonly StringBuilder _buffer;

		/// <summary>
		/// The domain name started with a partial domain name found. This is the original string of the domain name only.
		/// </summary>
		private readonly string _current;

		/// <summary>
		/// Keeps track of the number of characters since the last "."
		/// </summary>
		private int _currentLabelLength;

		/// <summary>
		/// Keeps track the number of dots that were found in the domain name.
		/// </summary>
		private int _dots;

		/// <summary>
		/// Keeps track if the entire domain name is numeric.
		/// </summary>
		private bool _numeric;

		/// <summary>
		/// Detection option of this reader.
		/// </summary>
		private readonly LegacyUrlDetectorOptions _options;

		/// <summary>
		/// Keeps track if we are seeing an ipv6 type address.
		/// </summary>
		private bool _seenBracket;

		/// <summary>
		/// Keeps track if we have seen a full bracket set "[....]"; used for ipv6 type address.
		/// </summary>
		private bool _seenCompleteBracketSet;

		/// <summary>
		/// Keeps track where the domain name started. This is non zero if the buffer starts with
		/// http://username:password@...
		/// </summary>
		private int _startDomainName;

		/// <summary>
		/// Keeps track of the number of characters in the top level domain.
		/// </summary>
		private int _topLevelLength;

		/// <summary>
		/// Keeps track if we have a zone index in the ipv6 address.
		/// </summary>
		private bool _zoneIndex;


		/// <summary>
		/// Creates a new instance of the DomainNameReader object.
		/// @param reader The input stream to read.
		/// @param buffer The string buffer to use for storing a domain name.
		/// @param current The current string that was thought to be a domain name.
		/// @param options The detector options of this reader.
		/// @param characterHandler The handler to call on each non-matching character to count matching quotes and stuff.
		/// </summary>
		public LegacyDomainNameReader(
			LegacyInputTextReader reader,
			StringBuilder buffer,
			string current,
			LegacyUrlDetectorOptions options,
			//CharacterHandler characterHandler
			Action<char> characterHandler
		)
		{
			_buffer = buffer;
			_current = current;
			_reader = reader;
			_options = options;
			_characterHandler = characterHandler;
		}

		/// <summary>
		/// Reads and parses the current string to make sure the domain name started where it was supposed to,
		/// and the current domain name is correct.
		/// @return The next state to use after reading the current.
		/// </summary>
		private ReaderNextState ReadCurrent()
		{
			if (_current != null)
			{
				//Handles the case where the string is ".hello"
				if (_current.Length == 1 && LegacyCharUtils.IsDot(_current[0]))
				{
					return ReaderNextState.InvalidDomainName;
				}

				if (_current.Length == 3 && _current.Equals("%" + HEX_ENCODED_DOT, StringComparison.InvariantCultureIgnoreCase))
				{
					return ReaderNextState.InvalidDomainName;
				}

				//The location where the domain name started.
				_startDomainName = _buffer.Length - _current.Length;

				//flag that the domain is currently all numbers and/or dots.
				_numeric = true;

				//If an invalid char is found, we can just restart the domain from there.
				var newStart = 0;

				var currArray = _current.ToCharArray();
				var length = currArray.Length;

				//hex special case
				var isAllHexSoFar = length > 2 && currArray[0] == '0' && (currArray[1] == 'x' || currArray[1] == 'X');

				var index = isAllHexSoFar ? 2 : 0;
				var done = false;

				while (index < length && !done)
				{
					//get the current character and update length counts.
					var curr = currArray[index];
					_currentLabelLength++;
					_topLevelLength = _currentLabelLength;

					//Is the length of the last part > 64 (plus one since we just incremented)
					if (_currentLabelLength > MAX_LABEL_LENGTH)
					{
						return ReaderNextState.InvalidDomainName;
					}

					if (LegacyCharUtils.IsDot(curr))
					{
						//found a dot. Increment dot count, and reset last length
						_dots++;
						_currentLabelLength = 0;
					}
					else if (curr == '[')
					{
						_seenBracket = true;
						_numeric = false;
					}
					else if (curr == '%' && index + 2 < length && LegacyCharUtils.IsHex(currArray[index + 1])
					         && LegacyCharUtils.IsHex(currArray[index + 2]))
					{
						//handle url encoded dot
						if (currArray[index + 1] == '2' && currArray[index + 2] == 'e')
						{
							_dots++;
							_currentLabelLength = 0;
						}
						else
						{
							_numeric = false;
						}

						index += 2;
					}
					else if (isAllHexSoFar)
					{
						//if it's a valid character in the domain that is not numeric
						if (!LegacyCharUtils.IsHex(curr))
						{
							_numeric = false;
							isAllHexSoFar = false;
							index--; //backtrack to rerun last character knowing it isn't hex.
						}
					}
					else if (LegacyCharUtils.IsAlpha(curr) || curr == '-' || curr >= INTERNATIONAL_CHAR_START)
					{
						_numeric = false;
					}
					else if (!LegacyCharUtils.IsNumeric(curr) && !_options.HasFlag(LegacyUrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN))
					{
						//if its not _numeric and not alphabetical, then restart searching for a domain from this point.
						newStart = index + 1;
						_currentLabelLength = 0;
						_topLevelLength = 0;
						_numeric = true;
						_dots = 0;
						done = true;
					}

					index++;
				}

				//An invalid character for the domain was found somewhere in the current buffer.
				//cut the first part of the domain out. For example:
				// http://asdf%asdf.google.com <- asdf.google.com is still valid, so restart from the %
				if (newStart > 0)
				{
					//make sure the location is not at the end. Otherwise the thing is just invalid.
					if (newStart < _current.Length)
					{
						_buffer.Clear();
						_buffer.Append(_current.Substring(newStart));

						//_buffer.Replace(0, _buffer.Length(), _current.javaSubstring(newStart));

						//cut out the previous part, so now the domain name has to be from here.
						_startDomainName = 0;
					}

					//now after cutting if the buffer is just "." newStart > current (last character in current is invalid)
					if (newStart >= _current.Length || _buffer.ToString().Equals("."))
					{
						return ReaderNextState.InvalidDomainName;
					}
				}
			}
			else
			{
				_startDomainName = _buffer.Length;
			}

			//all else is good, return OK
			return ReaderNextState.ValidDomainName;
		}

		/// <summary>
		/// Reads the Dns and returns the next state the state machine should take in throwing this out, or continue processing
		/// if this is a valid domain name.
		/// @return The next state to take.
		/// </summary>
		public ReaderNextState ReadDomainName()
		{
			//Read the current, and if its bad, just return.
			if (ReadCurrent() == ReaderNextState.InvalidDomainName)
			{
				return ReaderNextState.InvalidDomainName;
			}

			//If this is the first domain part, check if it's ip address in is hexa
			//similar to what is done on 'readCurrent' method
			bool isAllHexSoFar = (_current == null || _current.Equals(""))
			                        && _reader.CanReadChars(3) &&
			                        ("0x".Equals(_reader.Peek(2), StringComparison.InvariantCultureIgnoreCase));

			if (isAllHexSoFar) 
			{
				//Append hexa radix symbol characters (0x)
				_buffer.Append(_reader.Read());
				_buffer.Append(_reader.Read());
				_currentLabelLength += 2;
				_topLevelLength = _currentLabelLength;
			}

			//while not done and not end of string keep reading.
			var done = false;
			while (!done && !_reader.Eof())
			{
				var curr = _reader.Read();

				if (curr == '/')
				{
					//continue by reading the path
					return CheckDomainNameValid(ReaderNextState.ReadPath, curr);
				}

				if (curr == ':' && (!_seenBracket || _seenCompleteBracketSet))
				{
					//Don't check for a port if it's in the middle of an ipv6 address
					//continue by reading the port.
					return CheckDomainNameValid(ReaderNextState.ReadPort, curr);
				}

				if (curr == '?')
				{
					//continue by reading the query string
					return CheckDomainNameValid(ReaderNextState.ReadQueryString, curr);
				}

				if (curr == '#')
				{
					//continue by reading the fragment
					return CheckDomainNameValid(ReaderNextState.ReadFragment, curr);
				}
				else if (curr == '@')
				{
					//this may not have been a domain after all, but rather a username/password instead
					_reader.GoBack();
					return ReaderNextState.ReadUserPass;
				}
				else if (LegacyCharUtils.IsDot(curr)
				    || curr == '%' && _reader.CanReadChars(2) && _reader.Peek(2).Equals(HEX_ENCODED_DOT, StringComparison.InvariantCultureIgnoreCase))
				{
					//if the current character is a dot or a urlEncodedDot

					//handles the case: hello..
					if (_currentLabelLength < 1)
					{
						done = true;
					}
					else
					{
						//append the "." to the domain name
						_buffer.Append(curr);

						//if it was not a normal dot, then it is url encoded
						//read the next two chars, which are the hex representation
						if (!LegacyCharUtils.IsDot(curr))
						{
							_buffer.Append(_reader.Read());
							_buffer.Append(_reader.Read());
						}

						//increment the dots only if it's not part of the zone index and reset the last length.
						if (!_zoneIndex)
						{
							_dots++;
							_currentLabelLength = 0;
						}

						//if the length of the last section is longer than or equal to 64, it's too long to be a valid domain
						if (_currentLabelLength >= MAX_LABEL_LENGTH)
						{
							return ReaderNextState.InvalidDomainName;
						}
					}
				}
				else if (_seenBracket && (LegacyCharUtils.IsHex(curr) || curr == ':' || curr == '[' || curr == ']' || curr == '%')
				                      && !_seenCompleteBracketSet)
				{
					//if this is an ipv6 address.
					switch (curr)
					{
						case ':':
							_currentLabelLength = 0;
							break;
						case '[':
							// if we read another '[', we need to restart by re-reading from this bracket instead.
							_reader.GoBack();
							return ReaderNextState.InvalidDomainName;
						case ']':
							_seenCompleteBracketSet = true; //means that we already have a complete ipv6 address.
							_zoneIndex = false; //set this back off so that we can keep counting dots after ipv6 is over.
							break;
						case '%': //set flag to subtract subsequent dots because it's part of the zone index
							_zoneIndex = true;
							break;
						default:
							_currentLabelLength++;
							break;
					}

					_numeric = false;
					_buffer.Append(curr);
				}
				else if (LegacyCharUtils.IsAlphaNumeric(curr) || curr == '-' || curr >= INTERNATIONAL_CHAR_START)
				{
					//Valid domain name character. Either a-z, A-Z, 0-9, -, or international character
					if (_seenCompleteBracketSet)
					{
						//covers case of [fe80::]www.google.com
						_reader.GoBack();
						done = true;
					}
					else
					{
						if (isAllHexSoFar && !LegacyCharUtils.IsHex(curr)) {
							_numeric = false;
						}
						//if its not numeric, remember that;
						if (!isAllHexSoFar && !LegacyCharUtils.IsNumeric(curr)) {
							_numeric = false;
						}

						//append to the states.
						_buffer.Append(curr);
						_currentLabelLength++;
						_topLevelLength = _currentLabelLength;
					}
				}
				else if (curr == '[' && !_seenBracket)
				{
					_seenBracket = true;
					_numeric = false;
					_buffer.Append(curr);
				}
				else if (curr == '[' && _seenCompleteBracketSet)
				{
					//Case where [::][ ...
					_reader.GoBack();
					done = true;
				}
				else if (curr == '%' && _reader.CanReadChars(2) && LegacyCharUtils.IsHex(_reader.PeekChar(0))
				         && LegacyCharUtils.IsHex(_reader.PeekChar(1)))
				{
					//append to the states.
					_buffer.Append(curr);
					_buffer.Append(_reader.Read());
					_buffer.Append(_reader.Read());
					_currentLabelLength += 3;
					_topLevelLength = _currentLabelLength;
				}
				else
				{
					//called to increment the count of matching characters
					//_characterHandler.addCharacter(curr);

					_characterHandler(curr);

					//invalid character, we are done.
					done = true;
				}
			}

			//Check the domain name to make sure its ok.
			return CheckDomainNameValid(ReaderNextState.ValidDomainName, null);
		}

		/// <summary>
		/// Checks the current state of this object and returns if the valid state indicates that the
		/// object has a valid domain name. If it does, it will return append the last character
		/// and return the validState specified.
		/// @param validState The state to return if this check indicates that the dns is ok.
		/// @param lastChar The last character to add if the domain is ok.
		/// @return The validState if the domain is valid, else ReaderNextState.InvalidDomainName
		/// </summary>
		private ReaderNextState CheckDomainNameValid(ReaderNextState validState, char? lastChar)
		{
			var valid = false;

			//Max domain length is 255 which includes the trailing "."
			//most of the time this is not included in the url.
			//If the _currentLabelLength is not 0 then the last "." is not included so add it.
			//Same with number of labels (or dots including the last)
			var lastDotLength =
				_buffer.Length > 3 && _buffer.ToString().Substring(_buffer.Length - 3).Equals("%" + HEX_ENCODED_DOT, StringComparison.CurrentCultureIgnoreCase)
					? 3
					: 1;

			var domainLength = _buffer.Length - _startDomainName + (_currentLabelLength > 0 ? lastDotLength : 0);
			var dotCount = _dots + (_currentLabelLength > 0 ? 1 : 0);
			if (domainLength >= MAX_DOMAIN_LENGTH || dotCount > MAX_NUMBER_LABELS)
			{
				valid = false;
			}
			else if (_numeric)
			{
				var testDomain = _buffer.ToString().Substring(_startDomainName).ToLowerInvariant();
				valid = IsValidIpv4(testDomain);
			}
			else if (_seenBracket)
			{
				var testDomain = _buffer.ToString().Substring(_startDomainName).ToLowerInvariant();
				valid = IsValidIpv6(testDomain);
			}
			else if (_currentLabelLength > 0 && _dots >= 1 || _dots >= 2 && _currentLabelLength == 0
			                                               || _options.HasFlag(LegacyUrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN) && _dots == 0)
			{
				var topStart = _buffer.Length - _topLevelLength;
				if (_currentLabelLength == 0)
				{
					topStart--;
				}

				topStart = Math.Max(topStart, 0);

				// get the first 4 characters of the top level domain
				// original
				var endIndex = topStart + Math.Min(4, _buffer.Length - topStart);
				var topLevelStart = _buffer.ToString().Substring(topStart, endIndex - topStart);

				//There is no size restriction if the top level domain is international (starts with "xn--")
				valid =
					topLevelStart.Equals("xn--", StringComparison.InvariantCultureIgnoreCase) ||
					_topLevelLength >= MIN_TOP_LEVEL_DOMAIN && _topLevelLength <= MAX_TOP_LEVEL_DOMAIN;
			}

			if (valid)
			{
				//if it's valid, add the last character (if specified) and return the valid state.
				if (lastChar != null)
				{
					_buffer.Append(lastChar);
				}

				return validState;
			}

			//Roll back one char if its invalid to handle: "00:41.<br />"
			//This gets detected as 41.br otherwise.
			_reader.GoBack();

			//return invalid state.
			return ReaderNextState.InvalidDomainName;
		}


		/// <summary>
		/// Handles Hexadecimal, octal, decimal, dotted decimal, dotted hex, dotted octal.
		/// @param testDomain the string we're testing
		/// @return Returns true if it's a valid ipv4 address
		/// </summary>
		private bool IsValidIpv4(string testDomain)
		{
			var valid = false;
			if (testDomain.Length > 0)
			{
				//handling format without dots. Ex: http://2123123123123/path/a, http://0x8242343/aksdjf
				if (_dots == 0)
				{
					try
					{
						long value;
						if (testDomain.Length > 2 && testDomain[0] == '0' && testDomain[1] == 'x')
						{
							// hex
							var isParsed = long.TryParse(testDomain.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
							if (!isParsed)
							{
								return true;
							}
						}
						else if (testDomain[0] == '0')
						{
							// octal
							var possibleDomain = testDomain.Substring(1);
							if (LegacyOctalEncodingHelper.LooksLikeOctal(possibleDomain.AsSpan()))
							{
								value = Convert.ToInt64(possibleDomain, 8);
							}
							else
							{
								return false;
							}
						}
						else
						{
							// decimal
							var isParsed = long.TryParse(testDomain, out value);
							if (!isParsed)
							{
								return false;
							}
						}

						valid = value <= MAX_NUMERIC_DOMAIN_VALUE && value >= MIN_NUMERIC_DOMAIN_VALUE;
					}
					catch (Exception)
					{
						valid = false;
					}
				}
				else if (_dots == 3)
				{
					//Dotted decimal/hex/octal format
					var parts = LegacyCharUtils.SplitByDot(testDomain);
					valid = true;

					//check each part of the ip and make sure its valid.
					for (var i = 0; i < parts.Length && valid; i++)
					{
						var part = parts[i];
						if (part.Length > 0)
						{
							string parsedNum;
							int @base;
							if (part.Length > 2 && part[0] == '0' && part[1] == 'x')
							{
								//dotted hex
								parsedNum = part.Substring(2);
								@base = 16;
							}
							else if (part[0] == '0')
							{
								//dotted octal
								parsedNum = part.Substring(1);
								@base = 8;
							}
							else
							{
								//dotted decimal
								parsedNum = part;
								@base = 10;
							}

							int section;
							if (parsedNum.Length == 0)
							{
								section = 0;
							}
							else
							{
								// For efficiency, we try to avoid try/catch and instead use tryparse
								if (@base == 16)
								{
									var isParsed = int.TryParse(parsedNum, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out section);
									if (!isParsed) return false;
								}
								else if (@base == 10)
								{
									var isParsed = int.TryParse(parsedNum, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out section);
									if (!isParsed) return false;
								}
								else
								{
									// for other bases, fall back to try/catch
									if (@base == 8 && LegacyOctalEncodingHelper.LooksLikeOctal(parsedNum.AsSpan()))
									{
										try
										{
											section = Convert.ToInt32(parsedNum, @base);
										}
										catch (Exception)
										{
											return false;
										}
									}
									else
									{
										return false;
									}
									
								}
							}

							if (section < MIN_IP_PART || section > MAX_IP_PART)
							{
								valid = false;
							}
						}
						else
						{
							valid = false;
						}
					}
				}
			}

			return valid;
		}

		/// <summary>
		/// Sees that there's an open "[", and is now checking for ":"'s and stopping when there is a ']' or invalid character.
		/// Handles ipv4 formatted ipv6 addresses, zone indices, truncated notation.
		/// @return Returns true if it is a valid ipv6 address
		/// </summary>
		private bool IsValidIpv6(string testDomain)
		{
			var domainArray = testDomain.ToCharArray();

			// Return false if we don't see [....]
			// or if we only have '[]'
			// or if we detect [:8000: ...]; only [::8000: ...] is okay
			if (domainArray.Length < 3 || domainArray[domainArray.Length - 1] != ']' || domainArray[0] != '['
			    || domainArray[1] == ':' && domainArray[2] != ':')
			{
				return false;
			}

			var numSections = 1;
			var hexDigits = 0;
			var prevChar = '\0';
			//char prevChar = 0;

			//used to check ipv4 addresses at the end of ipv6 addresses.
			var lastSection = new StringBuilder();
			var hexSection = true;

			// If we see a '%'. Example: http://[::ffff:0xC0.0x00.0x02.0xEB%251]
			var zoneIndiceMode = false;

			//If doubleColonFlag is true, that means we've already seen one "::"; we're not allowed to have more than one.
			var doubleColonFlag = false;

			var index = 0;
			for (; index < domainArray.Length; index++)
			{
				switch (domainArray[index])
				{
					case '[': //found beginning of ipv6 address
						break;
					case '%':
					case ']': //found end of ipv6 address
						if (domainArray[index] == '%')
						{
							//see if there's a urlencoded dot
							if (domainArray.Length - index >= 2 && domainArray[index + 1] == '2' && domainArray[index + 2] == 'e')
							{
								lastSection.Append("%2e");
								index += 2;
								hexSection = false;
								break;
							}

							zoneIndiceMode = true;
						}

						if (!hexSection && (!zoneIndiceMode || domainArray[index] == '%'))
						{
							if (IsValidIpv4(lastSection.ToString()))
							{
								numSections++; //ipv4 takes up 2 sections.
							}
							else
							{
								return false;
							}
						}

						break;
					case ':':
						if (prevChar == ':')
						{
							if (doubleColonFlag)
							{
								//only allowed to have one "::" in an ipv6 address.
								return false;
							}

							doubleColonFlag = true;
						}

						//This means that we reached invalid characters in the previous section
						if (!hexSection)
						{
							return false;
						}

						hexSection = true; //reset hex to true
						hexDigits = 0; //reset count for hex digits
						numSections++;
						lastSection.Remove(0, lastSection.Length); //clear last section
						break;
					default:
						if (zoneIndiceMode)
						{
							if (!LegacyCharUtils.IsUnreserved(domainArray[index]))
							{
								return false;
							}
						}
						else
						{
							lastSection.Append(domainArray[index]); //collect our possible ipv4 address
							if (hexSection && LegacyCharUtils.IsHex(domainArray[index]))
							{
								hexDigits++;
							}
							else
							{
								hexSection = false; //non hex digit.
							}
						}

						break;
				}

				if (hexDigits > 4 || numSections > 8)
				{
					return false;
				}

				prevChar = domainArray[index];
			}

			//numSections != 1 checks for things like: [adf]
			//If there are more than 8 sections for the address or there isn't a double colon, then it's invalid.
			return numSections != 1 && (numSections >= 8 || doubleColonFlag);
		}

	}
}