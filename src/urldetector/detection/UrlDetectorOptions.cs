namespace urldetector.detection
{
	/// <summary>
	/// The options to use when detecting urls. This enum is used as a bit mask to be able to set multiple options at once.
	/// </summary>
	public enum UrlDetectorOptions
	{
		/// <summary>
		/// Default options, no special checks.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Matches quotes in the beginning and end of string.
		/// If a string starts with a quote, then the ending quote will be eliminated. For example,
		/// "http://linkedin.com" will pull out just 'http://linkedin.com' instead of 'http://linkedin.com"'
		/// </summary>
		QUOTE_MATCH = 1, // 00000001

		/// <summary>
		/// Matches single quotes in the beginning and end of a string.
		/// </summary>
		SINGLE_QUOTE_MATCH = 2, // 00000010

		/// <summary>
		/// Matches brackets and closes on the second one.
		/// Same as quote matching but works for brackets such as (), {}, [].
		/// </summary>
		BRACKET_MATCH = 4, // 000000100

		/// <summary>
		/// Checks for bracket characters and more importantly quotes to start and end strings.
		/// </summary>
		JSON = 5, //00000101

		/// <summary>
		/// Checks JSON format or but also looks for a single quote.
		/// </summary>
		JAVASCRIPT = 7, //00000111

		/// <summary>
		/// Checks for xml characters and uses them as ending characters as well as quotes.
		/// This also includes quote_matching.
		/// </summary>
		XML = 9, //00001001

		/// <summary>
		/// Checks all of the rules besides brackets. This is XML but also can contain javascript.
		/// </summary>
		HTML = 27, //00011011

		/// <summary>
		/// Checks for single level domains as well. Ex: go/, http://localhost
		/// </summary>
		ALLOW_SINGLE_LEVEL_DOMAIN = 32 //00100000

		/////// <summary>
		//// The numeric value.
		////  /
		////private int _value;

		/////// <summary>
		//// Creates a new Options enum
		//// @param value The numeric value of the enum
		////  /
		////UrlDetectorOptions(int value) {
		////  this._value = value;
		////}

		/////// <summary>
		//// Checks if the current options have the specified flag.
		//// @param flag The flag to check for.
		//// @return True if this flag is active, else false.
		////  /
		////public bool hasFlag(UrlDetectorOptions flag) {
		////  return (_value & flag._value) == flag._value;
		////}

		/////// <summary>
		//// Gets the numeric value of the enum
		//// @return The numeric value of the enum
		////  /
		////public int getValue() {
		////  return _value;
		////}
	}
}