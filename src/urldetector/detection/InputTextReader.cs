using System;

namespace urldetector.detection
{
	/// <summary>
	/// Class used to read a text input character by character. This also gives the ability to backtrack.
	/// </summary>
	public class InputTextReader
	{
		
		/// <summary>
		/// The content to read.
		/// </summary>
		private readonly char[] _content;


		/// <summary>
		/// The current position in the content we are looking at.
		/// </summary>
		private int _index;


		/// <summary>
		/// Creates a new instance of the InputTextReader using the content to read.
		/// </summary>
		/// <param name="content">param content The content to read.</param>
		public InputTextReader(string content)
		{
			_content = content.ToCharArray();
		}


		/// <summary>
		/// Reads a single char from the content stream and increments the index.
		/// @return The next available character.
		/// </summary>
		/// <returns></returns>
		public char Read()
		{
			var chr = _content[_index++];
			return CharUtils.IsWhiteSpace(chr) ? ' ' : chr;
		}


		/// <summary>
		/// Peeks at the next number of chars and returns as a string without incrementing the current index.
		/// @param numberChars The number of chars to peek.
		/// </summary>
		/// <param name="numberChars"></param>
		/// <returns></returns>
		public string Peek(int numberChars)
		{
			return new string(_content, _index, numberChars);
		}


		/// <summary>
		/// Gets the character in the array offset by the current index.
		/// @param offset The number of characters to offset.
		/// @return The character at the location of the index plus the provided offset.
		/// </summary>
		/// <param name="offset"></param>
		/// <returns></returns>
		public char PeekChar(int offset)
		{
			if (!CanReadChars(offset))
			{
				throw new IndexOutOfRangeException();
			}

			return _content[_index + offset];
		}


		/// <summary>
		/// Returns true if the reader has more the specified number of chars.
		/// @param numberChars The number of chars to see if we can read.
		/// @return True if we can read this number of chars, else false.
		/// </summary>
		/// <param name="numberChars"></param>
		/// <returns></returns>
		public bool CanReadChars(int numberChars)
		{
			return _content.Length >= _index + numberChars;
		}


		/// <summary>
		/// Checks if the current stream is at the end.
		/// @return True if the stream is at the end and no more can be read.
		/// </summary>
		/// <returns></returns>
		public bool Eof()
		{
			return _content.Length <= _index;
		}


		/// <summary>
		/// Gets the current position in the stream.
		/// @return The index to the current position.
		/// </summary>
		/// <returns></returns>
		public int GetPosition()
		{
			return _index;
		}

		
		/// <summary>
		/// Moves the index to the specified position.
		/// @param position The position to set the index to.
		/// </summary>
		/// <param name="position"></param>
		public void Seek(int position)
		{
			_index = position;
		}


		/// <summary>
		/// Goes back a single character.
		/// </summary>
		public void GoBack()
		{
			_index--;
		}
	}
}